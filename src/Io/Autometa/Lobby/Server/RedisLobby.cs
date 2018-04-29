using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Io.Autometa.Lobby.Contract;
using Io.Autometa.Redis;
using Newtonsoft.Json;

namespace Io.Autometa.Lobby.Server
{
    public class RedisLobby : ILobby
    {
        private static string ExpirationTimeSec = 600.ToString();
        private static int maxLobbySize = 30;

        // It can actually be a little over this, depends on how big
        // the batch result from Redis is.
        private static int maxSearchReturnSize = 100;

        private RedisOptions opt { get; set; }

        private string userIp { get; }

        // Ensure that only one instance of keys[2] exists, based on keys[1]
        private static string EnsureSingleLua =
@"redis.call(""DEL"",redis.call(""GET"",KEYS[1]))
redis.call(""SETEX"",KEYS[1]," + ExpirationTimeSec + @",KEYS[2])";

        public RedisLobby(string connectionAddress, string userIp)
        {
            if (string.IsNullOrEmpty(connectionAddress))
            {
                throw new ArgumentNullException(nameof(connectionAddress));
            }
            if (string.IsNullOrWhiteSpace(userIp))
            {
                throw new ArgumentNullException(nameof(userIp));
            }

            var splitUp = connectionAddress.Split(':', 2);
            this.opt = new RedisOptions();
            this.opt.Host = splitUp[0];
            this.opt.Port = int.Parse(splitUp[1]);
            this.userIp = userIp;
        }


        ServerResponse<GameLobby> ILobby.Create(CreateGameLobbyRequest newLobby)
        {
            var vc = newLobby.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new RedisClient(this.opt))
            {
                GameLobby gl = new GameLobby();
                gl.clients = new List<GameClient>();
                gl.creationTime = DateTime.UtcNow;
                gl.host = newLobby.owner;
                gl.host.ip = this.userIp; // override user-supplied ip
                gl.game = newLobby.owner.game;
                gl.hidden = newLobby.hidden;
                gl.lobbyID = newLobby.owner.game.GenerateID()
                    + (gl.hidden ? "$" : string.Empty); // dumb magic character
                
                gl.metaData = newLobby.metaData;

                var pipe = new RedisPipeline()
                    // only allow one game to be hosted per IP address
                    .Send(RedisCommand.EVAL, EnsureSingleLua, "2", "host:" + gl.host.ip, gl.lobbyID)
                    // actually create the lobby
                    .Send(RedisCommand.SETEX, gl.lobbyID, ExpirationTimeSec, JsonConvert.SerializeObject(gl));

                r.Send(pipe);

                if (!vc.result)
                {
                    return new ServerResponse<GameLobby>(null, vc);
                }

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<GameLobby> ILobby.Join(LobbyRequest request)
        {
            GameClient client = request.client;
            client.ip = this.userIp; // override user-supplied ip

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new RedisClient(this.opt))
            {
                var foundGame = r.Send(RedisCommand.GET, request.lobbyId);
                if (foundGame == null)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        new ValidationCheck(false, "lobby '" + request.lobbyId + "' does not exist.")
                    );
                }
                string lobbyStr = Encoding.UTF8.GetString(foundGame);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                var blc = new ValidationCheck()
                    .Assert(gl.game.gid == client.game.gid, "game api is mismatched")
                    .Assert(gl.host.uid != client.uid, "host can't join her own game")
                    .Assert(gl.clients.All(c => c.uid != client.uid), "already joined")
                    .Assert(gl.lobbyID == request.lobbyId, "lobby id changed")
                    .Assert(gl.clients.Count <= maxLobbySize, "lobby is full (" + maxLobbySize + ")");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                gl.clients.Add(client);

                r.Send(RedisCommand.SETEX, gl.lobbyID, ExpirationTimeSec, JsonConvert.SerializeObject(gl));

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        public ServerResponse<GameLobby> Leave(LobbyRequest request)
        {
            // unlike other methods, the Host can force a different user to Leave
            GameClient client = request.client;

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new RedisClient(this.opt))
            {
                var foundGame = r.Send(RedisCommand.GET, request.lobbyId);
                if (foundGame == null)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        new ValidationCheck(false, "lobby '" + request.lobbyId + "' does not exist.")
                    );
                }
                string lobbyStr = Encoding.UTF8.GetString(foundGame);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                var blc = new ValidationCheck()
                    .Assert(gl.game.gid == client.game.gid, "game api is mismatched")
                    .Assert(gl.lobbyID == request.lobbyId, "lobby id changed");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                // Caller is the host and is leaving their own game
                if ((gl.host.ip == this.userIp) && (client.ip == this.userIp))
                {
                    r.Send(RedisCommand.DEL, gl.lobbyID);
                }
                // Host is kicking someone or someone volunteered to leave
                else if ((gl.host.ip == this.userIp) || (client.ip == this.userIp))
                {
                    gl.clients.RemoveAll(c => c.ip == client.ip);
                    r.Send(RedisCommand.SETEX, gl.lobbyID, ExpirationTimeSec, JsonConvert.SerializeObject(gl));
                }
                else
                {
                    var vlc = new ValidationCheck(false, "permission denied");
                    return new ServerResponse<GameLobby>(null, vlc);
                }

                return new ServerResponse<GameLobby>(gl, null);
            }
        }
        ServerResponse<SearchResponse> ILobby.Search(Game game)
        {
            // ms
            long maxTime = 3000;
            Stopwatch runTime = new Stopwatch();
            runTime.Start();
            var vc = game.Validate();

            if (!vc.result)
            {
                return new ServerResponse<SearchResponse>(null, vc);
            }

            SearchResponse sr = new SearchResponse();
            sr.lobbies = new List<GameLobby>();
            RedisPipeline pipe = new RedisPipeline();
            using (var r = new RedisClient(this.opt))
            {
                // get keys
                foreach (var searchRes in r.Scan(RedisCommand.SCAN, game.gid + "*[^$]"))
                {
                    foreach (var key in searchRes)
                    {
                        // build up messages to get more data
                        pipe
                            .Send(RedisCommand.GET, key);

                        if ((pipe.Length >= maxSearchReturnSize) || runTime.ElapsedMilliseconds > maxTime/2)
                        {
                            break;
                        }
                    }
                    if ((pipe.Length >= maxSearchReturnSize) || runTime.ElapsedMilliseconds > maxTime/2)
                    {
                        break;
                    }
                }

                // get lobby data
                foreach (var lobby in r.Send(pipe))
                {
                    string lobbyStr = Encoding.UTF8.GetString(lobby);
                    GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);

                    sr.lobbies.Add(gl);

                    if (runTime.ElapsedMilliseconds > maxTime)
                    {
                        break;
                    }
                }
            }

            return new ServerResponse<SearchResponse>(
                sr,
                new ValidationCheck(true, "completed in " + runTime.Elapsed.TotalSeconds + " seconds"));
        }

        ServerResponse<GameLobby> ILobby.Read(ReadRequest request)
        {
            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new RedisClient(this.opt))
            {
                var p = new RedisPipeline()
                    .Send(RedisCommand.GET, request.lobbyId)
                    .Send(RedisCommand.EXPIRE, request.lobbyId);

                var foundGame = r.Send(p);

                if (foundGame[0] == null)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        new ValidationCheck(false, "lobby '" + request.lobbyId + "' does not exist.")
                    );
                }

                string lobbyStr = Encoding.UTF8.GetString(foundGame[0]);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                var blc = new ValidationCheck()
                    .Assert(gl.game.gid == request.game.gid, "game api is mismatched");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                return new ServerResponse<GameLobby>(gl, null);
            }
        }
    }
}