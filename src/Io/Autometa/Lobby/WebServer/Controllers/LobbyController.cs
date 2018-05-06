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
using System.Web;

namespace Io.Autometa.Lobby.WebServer.Controllers
{
    [Route("api")]
    public class LobbyController : Controller
    {
        private IHttpContextAccessor contextAccessor;
        private Io.Autometa.Lobby.Server.RedisLobby lobby;

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

        /// <summary>
        /// Delegate that is called with information timing durations.
        /// </summary>
        /// <param name="category">category, like "redis"</param>
        /// <param name="duration">in ms</param>
        /// <returns>true, but doesn't matter</returns>
        private bool PublishTiming(string category, long duration)
        {
            IServerTiming serverTiming = context.RequestServices.GetRequiredService<IServerTiming>();
            serverTiming.Metrics.Add(new ServerTimingMetric(category, duration));
            return true;
        }

        /// <summary>
        /// Create a new game. The caller is the owner.
        /// </summary>
        /// <param name="gameType">GameType of the lobby. This should be unique for your game.</param>
        /// <param name="body">Construction options for the lobby.</param>
        /// <returns>New state of the lobby, which includes the automatically generated Lobby Id.</returns>
        [HttpPost("{gameType}")]
        public GameLobby CreateGame(string gameType, [FromBody]CreateMessage body)
        {

            return lobby.Create(
                gameType,
                context.Connection.RemoteIpAddress.ToString(),
                body.port,
                HttpUtility.UrlPathEncode(body.name),
                body.hidden,
                body.meta);
        }

        /// <summary>
        /// Search lobbies for a given gametype.
        /// </summary>
        /// <param name="gameType">Gametype of the lobby. This should be unique for your game.</param>
        /// <param name="metaKey">Optional parameter. If specified, only lobbies that contain a metaData key matching this value will be returned.</param>
        /// <param name="metaValue">Optional parameter. If specified, the key given by metaKey must also have this matching value.</param>
        /// <returns>List of games.</returns>
        [HttpGet("{gameType}")]
        public List<GameLobby> Search(string gameType, [FromQuery]string metaKey, [FromQuery]string metaValue)
        {
            return lobby.Search(gameType, metaKey, metaValue);
        }

        /// <summary>
        /// Read the most recent state for a game lobby. This will also reset the expiration time.
        /// </summary>
        /// <param name="gameType">Gametype of the lobby. This should be unique for your game.</param>
        /// <param name="lobbyId">Specific lobby to read.</param>
        /// <returns>New state of the lobby.</returns>
        [HttpGet("{gameType}/{lobbyId}")]
        public GameLobby Read(string gameType, string lobbyId)
        {
            return lobby.Read(gameType, lobbyId);
        }

        /// <summary>
        /// Leave a lobby. Can also be used to kick a different player if the caller is the host.
        /// You don't really have to use Join or Leave methods to use the api; they are optional.
        /// </summary>
        /// <param name="gameType">Gametype of the lobby. This should be unique for your game.</param>
        /// <param name="lobbyId">Specific lobby to remove a client from.</param>
        /// <param name="playerId">Unique-to-this-lobby player ID. This is the player you want to remove.</param>
        /// <param name="ip">Address of the client to kick.</param>
        /// <returns>New state of the lobby.</returns>
        [HttpDelete("{gameType}/{lobbyId}/{playerId}")]
        public GameLobby Leave(string gameType, string lobbyId, string playerId)
        {
            return lobby.Leave(gameType, lobbyId, context.Connection.RemoteIpAddress.ToString(), playerId);
        }

        /// <summary>
        /// Join an existing lobby as a client.
        /// You don't really have to use Join or Leave methods to use the api; they are optional.
        /// </summary>
        /// <param name="gameType">Gametype of the lobby. This should be unique for your game.</param>
        /// <param name="lobbyId">Specific lobby to join.</param>
        /// <param name="playerId">Unique-to-this-lobby player ID.</param>
        /// <param name="body">Construction options for the client.</param>
        /// <returns>New state of the lobby.</returns>
        [HttpPost("{gameType}/{lobbyId}/{playerId}")]
        public GameLobby Join(string gameType, string lobbyId, string playerId, [FromQuery]int port)
        {
            return lobby.Join(
                gameType,
                lobbyId,
                context.Connection.RemoteIpAddress.ToString(),
                port,
                playerId);
        }

        [DataContract]
        public class CreateMessage
        {
            /// <summary>
            /// This is the port that the game server expects clients to connect to.
            /// </summary>
            [DataMember]
            public int port;

            /// <summary>
            /// This string will be URL-encoded.
            /// </summary>
            [DataMember]
            public string name;

            /// <summary>
            /// Set this to true to prevent the game from appearing in search results.
            /// </summary>
            [DataMember]
            public bool hidden;

            /// <summary>
            /// This is completely optional meta data. It can be used to narrow down search results,
            /// or just used to communicate additional information to clients.
            /// This information can't be changed once the lobby is created.
            /// </summary>
            [DataMember]
            public Dictionary<string,string> meta;
        }
    }
}