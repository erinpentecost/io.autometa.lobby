using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Io.Autometa.Lobby.Contract;
using Io.Autometa.Redis;
using Newtonsoft.Json;

namespace Io.Autometa.Lobby
{
    public class RedisLobby : ILobby
    {
        private static string ExpirationTimeSec = 600.ToString();
        private static int maxLobbySize = 30;
        private static int maxSearchReturnSize = 100;

        private RedisOptions opt {get; set;}

        private string userIp {get;}

        private static string EnsureSingleLua =
@"if redis.call(""EXISTS"",KEYS[1]) == 1 then
    redis.call(""DEL"",KEYS[2])
end
redis.call(""SET"",KEYS[1],KEYS[2])";

        public RedisLobby(string connectionAddress, string userIp)
        {
            //https://docs.aws.amazon.com/AmazonElastiCache/latest/UserGuide/Endpoints.html#Endpoints.Find.Redis
            //lobby.sni07u.0001.usw2.cache.amazonaws.com:6379
            var splitUp = connectionAddress.Split(':', 2);
            this.opt = new RedisOptions();
            this.opt.Host = splitUp[0];
            this.opt.Port = int.Parse(splitUp[1]);
            this.userIp = userIp;
        }


        ServerResponse<GameLobby> ILobby.Create(CreateGameLobby newLobby)
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
                gl.locked = false;
                gl.hidden = newLobby.hidden;
                gl.lobbyID = newLobby.owner.game.GenerateID()
                    + (gl.hidden ? "$" : string.Empty); // dumb magic character

                r.Send(RedisCommand.EVAL, EnsureSingleLua, "2", gl.host.ip, gl.lobbyID);

                r.Send(RedisCommand.SETEX, gl.lobbyID, ExpirationTimeSec, JsonConvert.SerializeObject(gl));

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
                        new ValidationCheck(false, "lobby '"+request.lobbyId+"' does not exist.")
                    );
                }
                string lobbyStr = Encoding.UTF8.GetString(foundGame);
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                var blc = new ValidationCheck()
                    .Assert(!gl.locked, "game is locked")
                    .Assert(gl.game.gid == client.game.gid, "game api is mismatched")
                    .Assert(gl.host.uid != client.uid, "host can't join her own game")
                    .Assert(gl.clients.All(c => c.uid != client.uid), "already joined")
                    .Assert(gl.lobbyID == request.lobbyId, "lobby id changed")
                    .Assert(gl.clients.Count <= maxLobbySize, "lobby is full ("+maxLobbySize+")");
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

        ServerResponse<GameLobby> ILobby.Lock(LobbyRequest request)
        {
            GameClient owner = request.client;
            owner.ip = this.userIp; // override user-supplied ip

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new RedisClient(this.opt))
            {
                string lobbyStr = Encoding.UTF8.GetString(r.Send(RedisCommand.GET, request.lobbyId));
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(lobbyStr);
                var blc = new ValidationCheck()
                    .Assert(!gl.locked, "game is locked")
                    .Assert(gl.game.gid == owner.game.gid, "game api is mismatched")
                    .Assert(gl.host.uid == owner.uid, "not the host")
                    .Assert(gl.lobbyID == request.lobbyId, "lobby id changed");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                gl.locked = true;

                r.Send(RedisCommand.DEL, gl.lobbyID);

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<SearchResponse> ILobby.Search(Game game)
        {
            var vc = game.Validate();
            
            if (!vc.result)
            {
                return new ServerResponse<SearchResponse>(null, vc);
            }

            SearchResponse sr = new SearchResponse();
            sr.lobbyID = new List<string>();
            using (var r = new RedisClient(this.opt))
            {
                foreach (var searchRes in r.Scan(RedisCommand.SCAN, game.gid+"*[^$]"))
                {
                    sr.lobbyID.AddRange(searchRes);
                    if (sr.lobbyID.Count > maxSearchReturnSize)
                    {
                        sr.lobbyID.RemoveRange(maxLobbySize, maxLobbySize - sr.lobbyID.Count);
                        break;
                    }
                }
            }

            return new ServerResponse<SearchResponse>(sr, null);
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
                        new ValidationCheck(false, "lobby '"+request.lobbyId+"' does not exist.")
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