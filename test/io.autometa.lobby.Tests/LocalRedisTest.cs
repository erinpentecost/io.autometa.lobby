using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using io.autometa.lobby;
using io.autometa.lobby.message;
using Newtonsoft.Json;

namespace io.autometa.lobby.Tests
{
    public class LocalRedisTest
    {
        ILobby r;

        Game testGame;

        public LocalRedisTest()
        {
            this.r = new RedisLobby("localhost:6379");
            this.testGame = new Game();
            this.testGame.api = 1;
            this.testGame.id = nameof(LocalRedisTest);
        }


        [Fact]
        public void Create()
        {
            message.GameClient gc = new GameClient();
            gc.game = this.testGame;
            gc.nickName = nameof(gc.nickName);
            gc.ip = "localhost";
            gc.port = 6969;
            
            CreateGameLobby cgl = new CreateGameLobby();
            cgl.owner = gc;
            cgl.hidden = false;

            var createResp = this.r.CreateLobby(cgl);
            Assert.True(createResp.valid.result, createResp.valid.ToString());

            var searchResp = this.r.Search(gc);
            Assert.True(searchResp.valid.result, searchResp.valid.reason.ToString());
            Assert.True(searchResp.response.lobbyID.Count > 0);
            Assert.True(searchResp.response.lobbyID
                .Any(id => id == createResp.response.lobbyID), "can't find lobby with id "+createResp.response.lobbyID);
            
            var readResp = this.r.ReadLobby(createResp.response.lobbyID, gc);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp.response));
        }
    }
}
