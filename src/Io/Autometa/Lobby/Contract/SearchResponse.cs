using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    /// This is an instance of an actual game lobby.
    [DataContract]
    public class SearchResponse : IMessage
    {
        [DataMember]
        public List<GameLobby> lobbies {get; set;}

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }
    }
}