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
            this.r = new RedisLobby("localhost:6379", "localhost");
            this.testGame = new Game();
            this.testGame.api = 1;
            this.testGame.id = nameof(LocalRedisTest);
        }


        /// Does happy-path check versus a real redis instance
        /// running on the localhost with default port.
        [Fact]
        public void IntegrationTest()
        {
            message.GameClient gcHost = new GameClient();
            gcHost.game = this.testGame;
            gcHost.nickName = nameof(gcHost.nickName);
            gcHost.ip = "localhost";
            gcHost.port = 6969;
            
            CreateGameLobby cgl = new CreateGameLobby();
            cgl.owner = gcHost;
            cgl.hidden = false;

            // Creat a new lobby
            var createResp = this.r.Create(cgl);
            AssertExt.Valid(createResp);

            // Find the lobby
            var searchResp1 = this.r.Search(gcHost);
            AssertExt.Valid(searchResp1);
            Assert.True(searchResp1.response.lobbyID.Count > 0);
            Assert.True(searchResp1.response.lobbyID
                .Any(id => id == createResp.response.lobbyID), "can't find lobby with id "+createResp.response.lobbyID);
            
            // Re-read the lobby
            var readResp1 = this.r.Read(new LobbyRequest(createResp.response.lobbyID, gcHost));
            AssertExt.Valid(readResp1);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp1.response));


            // Find the lobby as a different user.
            message.GameClient gcUser = new GameClient();
            gcUser.game = this.testGame;
            gcUser.nickName = "nonowner";
            gcUser.ip = "localhost";
            gcUser.port = 6960;
            
            var searchResp2 = this.r.Search(gcUser);
            AssertExt.Valid(searchResp2);
            Assert.True(searchResp2.response.lobbyID.Count > 0);
            Assert.True(searchResp2.response.lobbyID
                .Any(id => id == createResp.response.lobbyID), "can't find lobby with id "+createResp.response.lobbyID);
            
            // Re-read the lobby as a different user
            var readResp2 = this.r.Read(new LobbyRequest(createResp.response.lobbyID, gcUser));
            AssertExt.Valid(readResp2);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp2.response));
            
            // Join the lobby!
            var joinResp = this.r.Join(new LobbyRequest(createResp.response.lobbyID, gcUser));
            AssertExt.Valid(joinResp);

            // Read it again and verify we are in now
            var readResp3 = this.r.Read(new LobbyRequest(createResp.response.lobbyID, gcUser));
            AssertExt.Valid(readResp2);
            Assert.NotEqual(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp3.response));
            Assert.True(readResp3.response.clients.Count == 1);
            Assert.Equal(
                JsonConvert.SerializeObject(readResp3.response.clients[0]),
                JsonConvert.SerializeObject(gcUser));
        }
    }
}
