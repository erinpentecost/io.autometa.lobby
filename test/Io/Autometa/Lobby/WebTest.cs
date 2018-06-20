using System;
using System.Net.Http;
using Xunit;
using Io.Autometa.Lobby.WebServer.Controllers;
using Newtonsoft.Json;
using System.Collections.Generic;
using Io.Autometa.Lobby.Server.Contract;
using System.Net.Http.Headers;
using System.Text;

namespace Io.Autometa.Lobby.Tests
{
    public class WebTest
    {
        private Random rand = new Random();
        private const string web = @"http://localhost:5000/";
        private static HttpClient client = new HttpClient();

        public WebTest()
        {
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTestAsync()
        {
            string gameType = nameof(CreateTestAsync) + rand.Next(1,100);

            var gameOptions = new LobbyController.CreateMessage();
            gameOptions.hidden = false;
            gameOptions.name = "hey its me the host";
            gameOptions.port = 8000;
            Dictionary<string,string> meta = new Dictionary<string, string>();
            meta.Add("ok","hello everyone");
            gameOptions.meta = meta;

            var path = web + "api/" + gameType;
            string body = JsonConvert.SerializeObject(gameOptions);
            HttpResponseMessage response =
                await client.PostAsync(path,
                new StringContent(body,
                    Encoding.UTF8,
                    "application/json"));

            Assert.True(response.IsSuccessStatusCode);

            var contentString = await response.Content.ReadAsStringAsync();

            var lobbyState = JsonConvert.DeserializeObject<GameLobby>(contentString);

            Assert.Equal(gameOptions.hidden, lobbyState.hidden);
            Assert.Equal(gameOptions.name.Replace(" ", "%20"), lobbyState.host.name);
            Assert.Equal(gameOptions.port, lobbyState.host.port);
            Assert.Equal(gameOptions.meta["ok"], lobbyState.metaData["ok"]);
        }

    }
}
