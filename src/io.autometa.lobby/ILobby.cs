using System.Collections.Generic;
using io.autometa.lobby.message;

namespace io.autometa.lobby
{
    public interface ILobby
    {
        /// The list of clients should have at least one entry when this 
        /// is called, with one of those being the lobby owner.
        ServerResponse<GameLobby> CreateLobby(CreateGameLobby newLobby);
        ServerResponse<GameLobby> JoinLobby(LobbyRequest request);
        ServerResponse<GameLobby> LockLobby(LobbyRequest request);

        ServerResponse<GameLobby> ReadLobby(LobbyRequest request);

        /// Returns public games and private lobbies in which this client is a member
        ServerResponse<SearchResponse> Search(GameClient client);
    }
}