using System.Collections.Generic;
using io.autometa.lobby.message;

namespace io.autometa.lobby
{
    /// All these methods must have exactly 1 parameter
    public interface ILobby
    {
        /// The list of clients should have at least one entry when this 
        /// is called, with one of those being the lobby owner.
        ServerResponse<GameLobby> Create(CreateGameLobby newLobby);
        ServerResponse<GameLobby> Join(LobbyRequest request);
        ServerResponse<GameLobby> Lock(LobbyRequest request);

        ServerResponse<GameLobby> Read(LobbyRequest request);

        /// Returns public games and private lobbies in which this client is a member
        ServerResponse<SearchResponse> Search(GameClient client);
    }
}