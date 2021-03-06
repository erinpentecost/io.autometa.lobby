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
        Random rand = new Random();

        public LocalRedisTest()
        {
            
            this.r = new RedisLobby("localhost:6379");
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

                r.Send(RedisCommand.DEL, "derp");
            }
        }

        [Fact]
        public void HostTwoTest()
        {
            var gameType = nameof(HostTwoTest) + rand.Next(0,100);

            // Create test lobby
            var createResp1 = this.r.Create(gameType, "localhost", 6969, "host1", false, null);

            // Find the lobby
            var searchResp1 = this.r.Search(gameType);
            Assert.Equal(1, searchResp1.Count);
            Assert.Equal(createResp1.lobbyId, searchResp1[0].lobbyId);

            // Create another one
            var createResp2 = this.r.Create(gameType, "localhost", 9696, "host2", false, null);

            // Find the lobby
            var searchResp2 = this.r.Search(gameType);
            Assert.Equal(1, searchResp2.Count);
            Assert.Equal(createResp2.lobbyId, searchResp2[0].lobbyId);
        }

        [Fact]
        public void SearchTest()
        {
            var gameType = nameof(SearchTest) + rand.Next(0,100);
            Dictionary<string,string> metaData = new Dictionary<string, string>();
            metaData.Add("duh", "buh");
            metaData.Add("huh", "guh");

            // Create test lobby
            var createResp1 = this.r.Create(gameType, "localhost1", 6969, "host1", false, metaData);
            var createResp2 = this.r.Create(gameType, "localhost2", 6969, "host1", true, metaData);
            metaData["huh"] = "hahaha";
            var createResp3 = this.r.Create(gameType, "localhost3", 6969, "host1", false, metaData);
            var createResp4 = this.r.Create(gameType, "localhost4", 6969, "host1", false, null);

            // Find the lobby
            var searchResp1 = this.r.Search(gameType, "huh");
            Assert.Equal(2, searchResp1.Count);

            var searchResp2 = this.r.Search(gameType, "huh", "guh");
            Assert.Equal(1, searchResp2.Count);
            Assert.Equal(createResp1.lobbyId, searchResp2[0].lobbyId);

            var searchResp3 = this.r.Search(gameType);
            Assert.Equal(3, searchResp3.Count);


            var searchResp4 = this.r.Search(gameType, "huh", "hahaha");
            Assert.Equal(1, searchResp4.Count);
            Assert.Equal(createResp3.lobbyId, searchResp4[0].lobbyId);
        }

        /// Does happy-path check versus a real redis instance
        /// running on the localhost with default port.
        [Fact]
        public void IntegrationTest()
        {
            var gameType = nameof(IntegrationTest) + rand.Next(0,100);

            // Create test lobby
            var createResp = this.r.Create(gameType, "localhost", 6969, "host", false, null);

            // Find the lobby
            var searchResp1 = this.r.Search(gameType);
            Assert.True(searchResp1.Count > 0);
            Assert.True(searchResp1
                .Any(lobby => lobby.lobbyId == createResp.lobbyId), "can't find lobby with id "+createResp.lobbyId);

            // Re-read the lobby
            var readResp1 = this.r.Read(gameType, createResp.lobbyId);
            Assert.Equal(
                JsonConvert.SerializeObject(createResp),
                JsonConvert.SerializeObject(readResp1));
            
            // Join the lobby!
            var jClient = new Server.Contract.Client();
            jClient.ip = "127.0.0.1";
            jClient.port = 9000;
            jClient.name = "name";
            var joinResp = this.r.Join(gameType, createResp.lobbyId, jClient.ip, jClient.port, jClient.name);

            // Read it again and verify we are in now
            var readResp2 = this.r.Read(gameType, createResp.lobbyId);
            Assert.NotEqual(
                JsonConvert.SerializeObject(createResp),
                JsonConvert.SerializeObject(readResp2));
            Assert.True(readResp2.clients.Count == 1);
            Assert.Equal(
                JsonConvert.SerializeObject(jClient),
                JsonConvert.SerializeObject(readResp2.clients[0]));

            // Leave the lobby
            var leaveResp = this.r.Leave(gameType, createResp.lobbyId, jClient.ip, jClient.name);
            Assert.True(leaveResp.clients.Count == 0);

            // Host leaves the lobby
            var closeLobbyResp = this.r.Leave(gameType, createResp.lobbyId, createResp.host.ip, createResp.host.name);

            // Re-read the lobby
            bool deleted = false;
            try
            {
                this.r.Read(gameType, createResp.lobbyId);
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
