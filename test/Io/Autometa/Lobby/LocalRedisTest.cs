using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using Io.Autometa.Lobby;
using Newtonsoft.Json;
using System.IO;
using Io.Autometa.Lobby.Server;
using Io.Autometa.Redis;

namespace Io.Autometa.Lobby.Tests
{
    public class LocalRedisTest
    {
        RedisLobby r;
        private string gameType;

        public LocalRedisTest()
        {
            Random r = new Random();
            this.r = new RedisLobby("localhost:6379");
            this.gameType = nameof(LocalRedisTest) + r.Next(0,100);
        }

        [Fact]
        public void SetTest()
        {
            RedisOptions opt = new RedisOptions("localhost", 6379);
            using (var r = new RedisClient(opt))
            {
                var pipe1 = new RedisPipeline()
                    // only allow one game to be hosted per IP address
                    .Send(RedisCommand.GET, "derp")
                    // actually create the lobby
                    .Send(RedisCommand.SET, "derp", "hi", "NX", "EX", "900");

                var resp1 = r.Send(pipe1);
                Assert.NotEqual(null, resp1[1]);


                var pipe2 = new RedisPipeline()
                    // only allow one game to be hosted per IP address
                    .Send(RedisCommand.GET, "derp")
                    // actually create the lobby
                    .Send(RedisCommand.SET, "derp", "bye ", "NX", "EX", "900");

                var resp2 = r.Send(pipe2);
                Assert.Equal(null, resp2[1]);
            }
        }

        /// Does happy-path check versus a real redis instance
        /// running on the localhost with default port.
        [Fact]
        public void IntegrationTest()
        {
            // Create test lobby
            var createResp = this.r.Create(gameType, "localhost", 6969, "host", false, null);

            // Find the lobby
            var searchResp1 = this.r.Search(gameType);
            Assert.True(searchResp1.Count > 0);
            Assert.True(searchResp1
                .Any(lobby => lobby.lobbyID == createResp.lobbyID), "can't find lobby with id "+createResp.lobbyID);

            // Re-read the lobby
            var readResp1 = this.r.Read(gameType, createResp.lobbyID);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp),
                JsonConvert.SerializeObject(readResp1));
            
            // Join the lobby!
            var jClient = new Server.Contract.Client();
            jClient.ip = "127.0.0.1";
            jClient.port = 9000;
            jClient.nickName = "name";
            var joinResp = this.r.Join(gameType, createResp.lobbyID, jClient.ip, jClient.port, jClient.nickName);

            // Read it again and verify we are in now
            var readResp2 = this.r.Read(gameType, createResp.lobbyID);
            Assert.NotEqual(
                JsonConvert.SerializeObject(createResp),
                JsonConvert.SerializeObject(readResp2));
            Assert.True(readResp2.clients.Count == 1);
            Assert.Equal(
                JsonConvert.SerializeObject(jClient),
                JsonConvert.SerializeObject(readResp2.clients[0]));

            // Leave the lobby
            var leaveResp = this.r.Leave(gameType, createResp.lobbyID, jClient.ip, jClient.ip);
            Assert.True(readResp2.clients.Count == 0);

            // Host leaves the lobby
            var closeLobbyResp = this.r.Leave(gameType, createResp.lobbyID, createResp.host.ip, createResp.host.ip);

            // Re-read the lobby
            bool deleted = false;
            try
            {
                this.r.Read(gameType, createResp.lobbyID);
            }
            catch
            {
                // yay
                deleted = true;
            }
            Assert.True(deleted);

        }
    }
}
