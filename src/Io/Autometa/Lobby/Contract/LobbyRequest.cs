using System;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    [DataContract]
    public class LobbyRequest : IMessage
    {
        [DataMember]
        public string lobbyId {get;set;}

        [DataMember]
        public Client client {get;set;}

        public LobbyRequest(string lobbyId, Client client)
        {
            this.lobbyId = lobbyId;
            this.client = client;
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Assert(ValidationCheck.BasicStringCheck(this.lobbyId, "id"))
            .Assert(client != null, "client is null")
            .Assert(client.Validate);
        }
    }
}