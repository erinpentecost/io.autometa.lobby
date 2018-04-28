using System.Collections.Generic;

namespace Io.Autometa.Lobby.Contract
{
    /// All these methods must have exactly 1 parameter,
    /// and should all return a ServerResponse.
    public interface ILobby
    {
        /// Create a new lobby, with the caller as the host
        ServerResponse<GameLobby> Create(CreateGameLobby newLobby);

        /// Join an existing lobby
        ServerResponse<GameLobby> Join(LobbyRequest request);

        /// Leave (or kick someone else, if you are the host)
        ServerResponse<GameLobby> Leave(LobbyRequest request);

        /// Get lobby information when all you have it the id
        ServerResponse<GameLobby> Read(ReadRequest request);

        /// Returns public games
        ServerResponse<SearchResponse> Search(Game game);
    }
}