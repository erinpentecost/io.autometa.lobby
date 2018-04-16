using System;
using System.Collections.Generic;
using System.Linq;
using io.autometa.lobby.message;
using io.autometa.lobby.redissharp;
using Newtonsoft.Json;

namespace io.autometa.lobby
{
    public class RedisLobby : ILobby
    {
        private static int ExpirationTimeSec = 7200;

        private string host {get; set;}
        private int port {get;set;}
        public RedisLobby(string connectionAddress)
        {
            //https://docs.aws.amazon.com/AmazonElastiCache/latest/UserGuide/Endpoints.html#Endpoints.Find.Redis
            //lobby.sni07u.0001.usw2.cache.amazonaws.com:6379
            var splitUp = connectionAddress.Split(':', 2);
            this.host = splitUp[0];
            this.port = int.Parse(splitUp[1]);
        }


        ServerResponse<GameLobby> ILobby.CreateLobby(CreateGameLobby newLobby)
        {
            var vc = newLobby.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                r.Db = 0;
                GameLobby gl = new GameLobby();
                gl.clients = new List<GameClient>();
                gl.host = newLobby.owner;
                gl.game = newLobby.owner.game;
                gl.locked = false;
                gl.hidden = newLobby.hidden;
                gl.lobbyID = newLobby.owner.game.GenerateID()
                    + (gl.hidden ? "$" : string.Empty); // dumb magic character


                r.Set(gl.lobbyID, JsonConvert.SerializeObject(gl));
                r.Expire(gl.lobbyID, ExpirationTimeSec);

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<GameLobby> ILobby.JoinLobby(LobbyRequest request)
        {
            GameClient client = request.client;

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                r.Db = 0;
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(r.GetString(request.lobbyId));
                var blc = new ValidationCheck()
                    .Compose(!gl.locked, "game is locked")
                    .Compose(gl.game.gid == client.game.gid, "game api is mismatched")
                    .Compose(gl.host.uid != client.uid, "host can't join her own game")
                    .Compose(gl.clients.All(c => c.uid != client.uid), "already joined")
                    .Compose(gl.lobbyID == request.lobbyId, "lobby id changed");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                gl.clients.Add(client);

                r.Set(gl.lobbyID, Newtonsoft.Json.JsonConvert.SerializeObject(gl));

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<GameLobby> ILobby.LockLobby(LobbyRequest request)
        {
            GameClient owner = request.client;

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                r.Db = 0;
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(r.GetString(request.lobbyId));
                var blc = new ValidationCheck()
                    .Compose(!gl.locked, "game is locked")
                    .Compose(gl.game.gid == owner.game.gid, "game api is mismatched")
                    .Compose(gl.host.uid == owner.uid, "not the host")
                    .Compose(gl.lobbyID == request.lobbyId, "lobby id changed");
                if (!blc.result)
                {
                    return new ServerResponse<GameLobby>(
                        null,
                        blc);
                }

                gl.locked = true;

                r.Set(gl.lobbyID, Newtonsoft.Json.JsonConvert.SerializeObject(gl));

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<SearchResponse> ILobby.Search(GameClient client)
        {
            var vc = client.Validate();
            if (!vc.result)
            {
                return new ServerResponse<SearchResponse>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                r.Db = 0;
                // TODO: replace with SCAN
                SearchResponse sr = new SearchResponse();
                sr.lobbyID = new List<string>();
                sr.lobbyID.AddRange(r.GetKeys(client.game.gid+"*[^$]"));

                return new ServerResponse<SearchResponse>(sr, null);
            }
        }

        ServerResponse<GameLobby> ILobby.ReadLobby(LobbyRequest request)
        {
            GameClient client = request.client;

            var vc = request.Validate();
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                r.Db = 0;
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(r.GetString(request.lobbyId));
                var blc = new ValidationCheck()
                    .Compose(gl.game.gid == client.game.gid, "game api is mismatched")
                    .Compose(gl.lobbyID == request.lobbyId, "lobby id changed");
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