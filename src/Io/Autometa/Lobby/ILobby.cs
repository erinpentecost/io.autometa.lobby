using System.Collections.Generic;
using Io.Autometa.Lobby.Message;

namespace Io.Autometa.Lobby
{
    /// All these methods must have exactly 1 parameter
    public interface ILobby
    {
        /// Create a new lobby, with the caller as the host
        ServerResponse<GameLobby> Create(CreateGameLobby newLobby);

        /// Join an existing lobby
        ServerResponse<GameLobby> Join(LobbyRequest request);

        /// Lock (close, shut down) a lobby.
        ServerResponse<GameLobby> Lock(LobbyRequest request);

        /// Get lobby information when all you have it the id
        ServerResponse<GameLobby> Read(LobbyRequest request);

        /// Returns public games
        ServerResponse<SearchResponse> Search(Game game);
    }
}