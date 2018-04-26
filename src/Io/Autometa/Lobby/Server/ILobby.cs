using System.Collections.Generic;
using Io.Autometa.Lobby.Contract;
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
        ServerResponse<GameLobby> Read(ReadRequest request);

        /// Returns public games
        ServerResponse<SearchResponse> Search(Game game);
    }
}