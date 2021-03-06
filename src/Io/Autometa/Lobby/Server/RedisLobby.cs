using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Io.Autometa.Lobby.Server.Contract;
using Io.Autometa.Redis;
using Newtonsoft.Json;

namespace Io.Autometa.Lobby.Server
{
    public class RedisLobby
    {
        private static string ExpirationTimeSec = 300.ToString();
        private static int maxLobbySize = 30;

        // It can actually be a little over this, depends on how big
        // the batch result from Redis is.
        private static int maxSearchReturnSize = 100;

        private const string redisCategory = "redis";

        private const string lobbyPrefix = "LOBBY/";
        private const string hostPrefix = "HOST/";

        private Func<string, long, bool> publishTimingStats;

        private RedisOptions opt { get; set; }

        public const char SecretPrefix = 'S';

        // Ensure that only one instance of keys[2] exists, based on keys[1]
        // Note that SETEX may be deprecated at some point
        // This is a "bad" redis script because one of the keys that is operated
        // on is not called out as a KEYS argument.
        private static string EnsureSingleLua =
@"
local existingkey = redis.pcall(""GET"",KEYS[1])
redis.pcall('DEL',existingkey)
local setres = redis.call(""SETEX"",KEYS[1],""" + ExpirationTimeSec + @""",KEYS[2])
return setres";

        // The lobbyID is very much a magic string, and care
        // should be taken if you mess with it.
        // Redis search methods depend on it being a certain
        // format: "gameType-[$]shortId"
        private string CreateKey(string gameType, string lobbyID)
        {
            return (lobbyPrefix + gameType + "-" + lobbyID).ToUpperInvariant();
        }

        public RedisLobby(string connectionAddress, Func<string, long, bool> publishTimingStats=null)
        {
            if (string.IsNullOrEmpty(connectionAddress))
            {
                throw new ArgumentNullException(nameof(connectionAddress));
            }

            var splitUp = connectionAddress.Split(new char[]{':'}, 2, StringSplitOptions.RemoveEmptyEntries);
            this.opt = new RedisOptions();
            this.opt.Host = splitUp[0];
            this.opt.Port = int.Parse(splitUp[1]);

            this.publishTimingStats = publishTimingStats ?? new Func<string, long, bool>((n,d) => (true));
        }


        public GameLobby Create(string gameType, string ip, int port, string name, bool hidden, Dictionary<string,string> meta)
        {
            new ValidationCheck()
                .Assert(ValidationCheck.BasicStringCheck(gameType, nameof(gameType)))
                .Assert(ValidationCheck.BasicStringCheck(ip, nameof(ip)))
                .Assert(ValidationCheck.BasicStringCheck(name, nameof(name)))
                .Assert(port > 1024, nameof(port) + " is privileged")
                .Throw();

            gameType = gameType.ToUpperInvariant().Trim();
            ip = ip.ToUpperInvariant().Trim();

            using (var r = new RedisClient(this.opt))
            {
                GameLobby gl = new GameLobby();
                gl.clients = new List<Client>();
                gl.creationTime = DateTime.UtcNow;
                gl.host = new Client();
                gl.host.port = port;
                gl.host.name = name;
                gl.host.ip = ip;
                gl.gameType = gameType;
                gl.hidden = hidden;
                // The lobbyID is very much a magic string, and care
                // should be taken if you mess with it.
                // Redis search methods depend on it being a certain
                // format: "gameType-[s]shortId"
                gl.lobbyId = IdGenerator.GetId(hidden);

                gl.metaData = meta ?? new Dictionary<string, string>();

                string lobbyKey = CreateKey(gl.gameType, gl.lobbyId);

                var pipe = new RedisPipeline()
                    // only allow one game to be hosted per IP address
                    .Send(RedisCommand.EVAL, EnsureSingleLua, "2", hostPrefix + gl.host.ip, lobbyKey)
                    // actually create the lobby
                    .Send(RedisCommand.SET, lobbyKey, JsonConvert.SerializeObject(gl), "NX", "EX", ExpirationTimeSec);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var resp = r.Send(pipe);
                this.publishTimingStats(redisCategory, sw.ElapsedMilliseconds);

                if (resp[1] == null)
                {
                    throw new LobbyException(500, "lobby already exists");
                }

                return gl;
            }
        }

        public GameLobby Join(string gameType, string lobbyId, string ip, int port, string name)
        {
            new ValidationCheck()
                .Assert(ValidationCheck.BasicStringCheck(gameType, nameof(gameType)))
                .Assert(ValidationCheck.BasicStringCheck(lobbyId, nameof(lobbyId)))
                .Assert(ValidationCheck.BasicStringCheck(name, nameof(name)))
                .Assert(ValidationCheck.BasicStringCheck(ip, nameof(ip)))
                .Assert(port > 1024, nameof(port) + " is privileged")
                .Throw();
            
            gameType = gameType.ToUpperInvariant();
            lobbyId = lobbyId.ToUpperInvariant();

            Client client = new Client();
            client.port = port;
            client.name = name;
            client.ip = ip;

            string lobbyKey = CreateKey(gameType, lobbyId);

            using (var r = new RedisClient(this.opt))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var foundGame = r.Send(RedisCommand.GET, lobbyKey);
                sw.Stop();

                if (foundGame == null)
                {
                    throw new LobbyException(404, "lobby does not exist");
                }
                string lobbyStr = Encoding.UTF8.GetString(foundGame);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                new ValidationCheck()
                    .Assert(gl.host.uid != client.uid, "host can't join her own game")
                    .Assert(gl.clients.All(c => c.uid != client.uid), "already joined")
                    .Assert(gl.clients.All(c => c.name != client.name), "must provide a unique name")
                    .Assert(gl.gameType == gameType, "lobby type changed")
                    .Assert(gl.lobbyId == lobbyId, "lobby id changed")
                    .Assert(gl.clients.Count <= maxLobbySize, "lobby is full (" + maxLobbySize + ")")
                    .Throw();

                gl.clients.Add(client);

                sw.Start();
                var resp = r.Send(RedisCommand.SET, lobbyKey, JsonConvert.SerializeObject(gl), "XX", "EX", ExpirationTimeSec);
                this.publishTimingStats(redisCategory, sw.ElapsedMilliseconds);

                if (resp == null)
                {
                    throw new LobbyException(404, "lobby no longer exists");
                }

                return gl;
            }
        }

        public GameLobby Leave(string gameType, string lobbyId, string callingIp, string kickName=null)
        {
            new ValidationCheck()
                .Assert(ValidationCheck.BasicStringCheck(gameType, nameof(gameType)))
                .Assert(ValidationCheck.BasicStringCheck(lobbyId, nameof(lobbyId)))
                .Assert(ValidationCheck.BasicStringCheck(kickName, nameof(kickName)))
                .Assert(ValidationCheck.BasicStringCheck(callingIp, nameof(callingIp)))
                .Throw();
            
            gameType = gameType.ToUpperInvariant();
            lobbyId = lobbyId.ToUpperInvariant();

            // unlike other methods, the Host can force a different user to Leave
            string lobbyKey = CreateKey(gameType, lobbyId);

            using (var r = new RedisClient(this.opt))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var foundGame = r.Send(RedisCommand.GET, lobbyKey);
                sw.Stop();

                if (foundGame == null)
                {
                    throw new LobbyException(404, "lobby does not exist");
                }
                string lobbyStr = Encoding.UTF8.GetString(foundGame);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                new ValidationCheck()
                    .Assert(gl.lobbyId == lobbyId, "lobby id changed")
                    .Assert(gl.gameType == gameType, "gametype changed")
                    .Throw();

                // Determine the client of the caller
                Client caller = gl.clients
                    .Concat(new Client[]{gl.host})
                    .First(c => string.Equals(c.ip, callingIp, StringComparison.InvariantCultureIgnoreCase));

                // Determine who is being kicked. If kickName is null, assume the caller is leaving.
                Client kickee = caller;
                if (!string.IsNullOrWhiteSpace(kickName))
                {
                    kickee = gl.clients
                        .Concat(new Client[]{gl.host})
                        .FirstOrDefault(c => string.Equals(c.name, kickName, StringComparison.InvariantCultureIgnoreCase));
                }

                new ValidationCheck()
                    .Assert(caller != null, "caller must be in the client list")
                    .Assert(kickee != null, "client to kick must be in the client list")
                    .Throw();

                // Caller is the host and is leaving their own game
                if ((gl.host == caller) && (gl.host == kickee))
                {
                    sw.Start();
                    r.Send(RedisCommand.DEL, lobbyKey);
                    sw.Stop();
                }
                // Host is kicking someone or someone volunteered to leave
                else if ((gl.host == caller) || (kickee == caller))
                {
                    gl.clients.RemoveAll(c => string.Equals(c.name, kickName, StringComparison.InvariantCultureIgnoreCase));

                    sw.Start();
                    var resp = r.Send(RedisCommand.SET, lobbyKey, JsonConvert.SerializeObject(gl), "XX", "EX", ExpirationTimeSec);
                    sw.Stop();

                    if (resp == null)
                    {
                        throw new LobbyException(404, "lobby no longer exists");
                    }
                }
                else
                {
                    throw new LobbyException(403, "only the host can kick other users");
                }

                this.publishTimingStats(redisCategory, sw.ElapsedMilliseconds);

                return gl;
            }
        }

        /// <summary>
        /// get a list of lobbies for a gametype.
        /// hidden games will not be returned
        /// </summary>
        /// <param name="gameType">gametype to search</param>
        /// <param name="metaKey">optional metadata key filter. the key must exist</param>
        /// <param name="metaValue">optional metadata value filter. the value must match for the given key.</param>
        /// <returns></returns>
        public List<GameLobby> Search(string gameType, string metaKey=null, string metaValue=null)
        {
            new ValidationCheck()
                .Assert(ValidationCheck.BasicStringCheck(gameType, nameof(gameType)))
                .Throw();

            gameType = gameType.ToUpperInvariant();

            // As it stands now, this search method will not scale well when there are many instances
            // of lobbies for the same game. maxSearchReturnSize*keyMult games are pulled first,
            // and then those are filtered by metaKey and metaValue. If none of those games in that
            // first scan match, then no games will be returned.
            // If this ever becomes a problem, I could create additional keys for metadata
            // inside Redis and use pattern matching SCAN on those keys. This just opens me up
            // to requiring a much larger redis instance, though, unless I want to further restrict
            // metaData.
            int keyMult = 1;
            Func<Dictionary<string, string>, bool> matches = (d) => true;
            if (!string.IsNullOrWhiteSpace(metaKey))
            {
                keyMult = 10;
                if (!string.IsNullOrWhiteSpace(metaValue))
                {
                    matches = (d) => d != null && d.ContainsKey(metaKey) && string.Equals(d[metaKey], metaValue);
                }
                else
                {
                    matches = (d) => d != null && d.ContainsKey(metaKey);
                }
            }

            var foundGames = new List<GameLobby>();
            RedisPipeline pipe = new RedisPipeline();
            using (var r = new RedisClient(this.opt))
            {
                // get initial set of keys
                Stopwatch sw = new Stopwatch();
                sw.Start();
                foreach (var searchRes in r.Scan(RedisCommand.SCAN, lobbyPrefix + gameType + "-[^" + SecretPrefix.ToString() + "]*"))
                {
                    foreach (var key in searchRes)
                    {
                        // build up messages to get more data
                        pipe.Send(RedisCommand.GET, key);

                        if (pipe.Length >= maxSearchReturnSize*keyMult)
                        {
                            break;
                        }
                    }
                    if (pipe.Length >= maxSearchReturnSize*keyMult)
                    {
                        break;
                    }
                }
                
                // get lobby data
                var readDetails = r.Send(pipe);
                this.publishTimingStats(redisCategory, sw.ElapsedMilliseconds);

                foreach (var lobby in readDetails)
                {
                    string lobbyStr = Encoding.UTF8.GetString(lobby);
                    GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);

                    if (matches(gl.metaData) && (gl.gameType == gameType))
                    {
                        foundGames.Add(gl);
                    }

                    if (foundGames.Count >= maxSearchReturnSize)
                    {
                        break;
                    }
                }
            }

            if (foundGames.Count == 0)
            {
                throw new LobbyException(404, "no games match search parameters");
            }

            return foundGames;
        }

        public GameLobby Read(string gameType, string lobbyId)
        {
            new ValidationCheck()
                .Assert(ValidationCheck.BasicStringCheck(gameType, nameof(gameType)))
                .Assert(ValidationCheck.BasicStringCheck(lobbyId, nameof(lobbyId)))
                .Throw();

            string lobbyKey = CreateKey(gameType, lobbyId);

            using (var r = new RedisClient(this.opt))
            {
                var p = new RedisPipeline()
                    .Send(RedisCommand.GET, lobbyKey)
                    .Send(RedisCommand.EXPIRE, lobbyKey);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var foundGame = r.Send(p);
                this.publishTimingStats(redisCategory, sw.ElapsedMilliseconds);

                if (foundGame[0] == null)
                {
                    throw new LobbyException(404, "lobby does not exist");
                }

                string lobbyStr = Encoding.UTF8.GetString(foundGame[0]);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);

                return gl;
            }
        }
    }
}