using System.Collections.Generic;
using Io.Autometa.Lobby.Message;

namespace Io.Autometa.Lobby
{
    public class EchoLobby : ILobby
    {
        private List<object> message {get;}
        private ValidationCheck vc {get;}

        public EchoLobby(object message)
        {
            this.message = new List<object>();
            this.message.Add(new { Echo = "This is a debug echo response." });
            this.message.Add(message);
            this.vc = new ValidationCheck();
            this.vc.result = false;
            this.vc.reason = this.message;
        }

        ServerResponse<GameLobby> ILobby.Create(CreateGameLobby newLobby)
        {
            return new ServerResponse<GameLobby>(null, this.vc);
        }

        ServerResponse<GameLobby> ILobby.Join(LobbyRequest request)
        {
            return new ServerResponse<GameLobby>(null, this.vc);
        }

        ServerResponse<GameLobby> ILobby.Lock(LobbyRequest request)
        {
            return new ServerResponse<GameLobby>(null, this.vc);
        }

        ServerResponse<GameLobby> ILobby.Read(ReadRequest request)
        {
            return new ServerResponse<GameLobby>(null, this.vc);
        }

        ServerResponse<SearchResponse> ILobby.Search(Game client)
        {
            return new ServerResponse<SearchResponse>(null, this.vc);
        }
    }
}