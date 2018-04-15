using System.Collections.Generic;
using io.autometa.lobby.message;

namespace io.autometa.lobby
{
    public class EchoLobby : ILobby
    {
        private List<object> message {get;}

        public EchoLobby(object message)
        {
            this.message = new List<object>();
            this.message.Add(new { Echo = "This is a debug echo response." });
            this.message.Add(message);
        }

        ServerResponse<GameLobby> ILobby.CreateLobby(GameLobby newLobby)
        {
            var vc = new ValidationCheck();
            vc.result = false;
            vc.reason = this.message;

            return new ServerResponse<GameLobby>(null, vc);
        }

        ServerResponse<GameLobby> ILobby.JoinLobby(string lobbyID, GameClient client)
        {
            var vc = new ValidationCheck();
            vc.result = false;
            vc.reason = this.message;

            return new ServerResponse<GameLobby>(null, vc);
        }

        ServerResponse<GameLobby> ILobby.LockLobby(string lobbyID, GameClient owner)
        {
            var vc = new ValidationCheck();
            vc.result = false;
            vc.reason = this.message;

            return new ServerResponse<GameLobby>(null, vc);
        }

        ServerResponse<List<GameLobby>> ILobby.Search(GameClient client)
        {
            var vc = new ValidationCheck();
            vc.result = false;
            vc.reason = this.message;

            return new ServerResponse<List<GameLobby>>(null, vc);
        }
    }
}