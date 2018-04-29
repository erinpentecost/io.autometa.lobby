using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using Io.Autometa.Lobby;
using Io.Autometa.Lobby.Contract;
using Newtonsoft.Json;
using System.IO;
using Io.Autometa.Schema;
using Io.Autometa.Lobby.Server;

namespace Io.Autometa.Lobby.Tests
{
    public class LocalRedisTest
    {
        ILobby r;

        Game testGame;

        public LocalRedisTest()
        {
            Random r = new Random();
            this.r = new RedisLobby("localhost:6379", "localhost");
            this.testGame = new Game();
            this.testGame.api = r.Next(0,100);
            this.testGame.id = nameof(LocalRedisTest);
        }

        private static string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static void DumpExample<T>(T example)
        {
            File.WriteAllText(Path.Combine(exeDir, typeof(T).GetFriendlyName() +".sample.json"), JsonConvert.SerializeObject(example));
        }

        /// Does happy-path check versus a real redis instance
        /// running on the localhost with default port.
        [Fact]
        public void IntegrationTest()
        {
            Client gcHost = new Client();
            gcHost.nickName = nameof(gcHost.nickName);
            gcHost.ip = "localhost";
            gcHost.port = 6969;
            DumpExample(gcHost);

            CreateGameLobbyRequest cgl = new CreateGameLobbyRequest();
            cgl.owner = gcHost;
            cgl.gameType = this.testGame;
            cgl.hidden = false;
            DumpExample(cgl);

            // Creat a new lobby
            var createResp = this.r.Create(cgl);
            AssertExt.Valid(createResp);
            DumpExample(createResp);

            // Find the lobby
            var searchResp1 = this.r.Search(this.testGame);
            AssertExt.Valid(searchResp1);
            Assert.True(searchResp1.response.lobbies.Count > 0);
            Assert.True(searchResp1.response.lobbies
                .Any(lobby => lobby.lobbyID == createResp.response.lobbyID), "can't find lobby with id "+createResp.response.lobbyID);
            DumpExample(searchResp1);

            // Re-read the lobby
            var readResp1 = this.r.Read(new ReadRequest(createResp.response.lobbyID, cgl.gameType));
            AssertExt.Valid(readResp1);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp1.response));
            DumpExample(readResp1);

            // Find the lobby as a different user.

            var searchResp2 = this.r.Search(this.testGame);
            AssertExt.Valid(searchResp2);
            Assert.True(searchResp2.response.lobbies.Count > 0);
            Assert.True(searchResp2.response.lobbies
                .Any(lobby => lobby.lobbyID == createResp.response.lobbyID), "can't find lobby with id "+createResp.response.lobbyID);
            
            // Re-read the lobby as a different user
            var readResp2 = this.r.Read(new ReadRequest(createResp.response.lobbyID, this.testGame));
            AssertExt.Valid(readResp2);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp.response),
                JsonConvert.SerializeObject(readResp2.response));
            
            // Join the lobby!
            Client gcUser = new Client();
            gcHost.nickName = "some client";
            gcUser.ip = "127.0.0.1";
            gcUser.port = 9000;
            var joinReq = new LobbyRequest(createResp.response.lobbyID, gcUser);
            DumpExample(joinReq);
            var joinResp = this.r.Join(joinReq);
            AssertExt.Valid(joinResp);
            DumpExample(joinResp);

            // Read it again and verify we are in now
            var readResp3 = this.r.Read(new ReadRequest(createResp.response.lobbyID, this.testGame));
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
