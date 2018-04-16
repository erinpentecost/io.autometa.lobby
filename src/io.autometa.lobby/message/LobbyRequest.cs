using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
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
            .Compose(ValidationCheck.BasicStringCheck(this.lobbyId, "id"))
            .Compose(client != null, "client is null")
            .Compose(lobbyId.StartsWith(client.game.gid), "lobby id is incorrect")
            .Compose(client.Validate);
        }
    }
}