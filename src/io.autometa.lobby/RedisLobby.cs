using System.Collections.Generic;
using io.autometa.lobby.message;

namespace io.autometa.lobby
{
    public class RedisLobby : ILobby
    {
        ServerResponse<GameLobby> ILobby.CreateLobby(GameLobby newLobby)
        {
            throw new System.NotImplementedException();
        }

        ServerResponse<GameLobby> ILobby.JoinLobby(string lobbyID, GameClient client)
        {
            throw new System.NotImplementedException();
        }

        ServerResponse<GameLobby> ILobby.LockLobby(string lobbyID, GameClient owner)
        {
            throw new System.NotImplementedException();
        }

        ServerResponse<List<GameLobby>> ILobby.Search(GameClient client)
        {
            throw new System.NotImplementedException();
        }
    }
}