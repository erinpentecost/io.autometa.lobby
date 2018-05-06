using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Lib.AspNetCore.ServerTiming;
using Lib.AspNetCore.ServerTiming.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Serialization;

using Io.Autometa.Lobby.Server;
using Io.Autometa.Lobby.Server.Contract;

namespace Io.Autometa.Lobby.WebServer.Controllers
{
    [Route("api")]
    public class LobbyController : Controller
    {
        private IHttpContextAccessor contextAccessor;

        public LobbyController(IHttpContextAccessor httpContextAccessor)
        {
            contextAccessor = httpContextAccessor;
            lobby = new RedisLobby(
                    Environment.GetEnvironmentVariable("ElasticacheConnectionString"),
                    PublishTiming);
        }

        private HttpContext context
        {
            get
            {
                return this.contextAccessor.HttpContext;
            }
        }

        private Io.Autometa.Lobby.Server.RedisLobby lobby;

        private bool PublishTiming(string category, long duration)
        {
            IServerTiming serverTiming = context.RequestServices.GetRequiredService<IServerTiming>();
            serverTiming.Metrics.Add(new ServerTimingMetric(category, duration));
            return true;
        }

        [HttpPost("{gameType}")]
        public GameLobby CreateGame(string gameType, [FromBody]CreateMessage body)
        {

            return lobby.Create(
                gameType,
                context.Connection.RemoteIpAddress.ToString(),
                body.port,
                body.name,
                body.hidden,
                body.meta);
        }

        [HttpGet("{gameType}")]
        public List<GameLobby> Search(string gameType, [FromQuery]string metaKey, [FromQuery]string metaValue)
        {
            return lobby.Search(gameType, metaKey, metaValue);
        }

        [HttpGet("{gameType}/{lobbyId}")]
        public GameLobby Read(string gameType, string lobbyId)
        {
            return lobby.Read(gameType, lobbyId);
        }

        [HttpDelete("{gameType}/{lobbyId}")]
        public GameLobby Leave(string gameType, string lobbyId, [FromQuery]string ip)
        {
            return lobby.Leave(gameType, lobbyId, ip, context.Connection.RemoteIpAddress.ToString());
        }

        [HttpPut("{gameType}/{lobbyId}")]
        public GameLobby Join(string gameType, string lobbyId, [FromBody]JoinMessage body)
        {
            return lobby.Join(
                gameType,
                lobbyId,
                context.Connection.RemoteIpAddress.ToString(),
                body.port,
                body.name);
        }

        [DataContract]
        public class CreateMessage
        {
            [DataMember]
            public int port;
            [DataMember]
            public string name;
            [DataMember]
            public bool hidden;
            [DataMember]
            public Dictionary<string,string> meta;
        }

        [DataContract]
        public class JoinMessage
        {
            [DataMember]
            public int port;
            [DataMember]
            public string name;
        }
    }
}