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
                GameLobby gl = new GameLobby();
                gl.clients = new List<GameClient>();
                gl.host = newLobby.owner;
                gl.game = newLobby.owner.game;
                gl.lobbyID = newLobby.owner.game.GenerateID();
                gl.locked = false;
                gl.hidden = newLobby.hidden;

                r.Set(gl.lobbyID, JsonConvert.SerializeObject(gl));
                r.Expire(gl.lobbyID, ExpirationTimeSec);

                return new ServerResponse<GameLobby>(gl, null);
            }
        }

        ServerResponse<GameLobby> ILobby.JoinLobby(string lobbyID, GameClient client)
        {
            var vc = client.Validate()
            .Compose(ValidationCheck.BasicStringCheck(lobbyID))
            .Compose(lobbyID.StartsWith(client.game.gid), "lobby id is incorrect");
            if (!vc.result)
            {
                return new ServerResponse<GameLobby>(null, vc);
            }

            using (var r = new Redis(this.host, this.port))
            {
                GameLobby gl = JsonConvert.DeserializeObject<GameLobby>(r.GetString(lobbyID));
                var blc = new ValidationCheck()
                    .Compose(!gl.locked, "game is locked")
                    .Compose(gl.game.gid != client.game.gid, "game api is mismatched")
                    .Compose(gl.host.uid != client.uid, "host can't join her own game")
                    .Compose(gl.clients.All(c => c.uid != client.uid), "already joined")
                    .Compose(gl.lobbyID == lobbyID, "lobby id changed");
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

        ServerResponse<GameLobby> ILobby.LockLobby(string lobbyID, GameClient owner)
        {
            throw new System.NotImplementedException();
        }

        ServerResponse<List<GameLobby>> ILobby.Search(GameClient client)
        {
            throw new System.NotImplementedException();
        }

        
    }
}