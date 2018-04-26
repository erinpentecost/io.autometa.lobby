using System;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    /// This is a game API definition.
    [DataContract]
    public class LobbyRequest : IMessage
    {
        [DataMember]
        public string lobbyId {get;set;}

        /// This should be the version of the game.
        [DataMember]
        public GameClient client {get;set;}

        public LobbyRequest(string lobbyId, GameClient client)
        {
            this.lobbyId = lobbyId;
            this.client = client;
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Assert(ValidationCheck.BasicStringCheck(this.lobbyId, "id"))
            .Assert(client != null, "client is null")
            .Assert(lobbyId.StartsWith(client.game.gid), "lobby id is incorrect")
            .Assert(client.Validate);
        }
    }
}