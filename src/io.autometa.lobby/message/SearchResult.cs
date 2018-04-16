using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    /// This is an instance of an actual game lobby.
    [DataContract]
    public class SearchResult : IMessage
    {
        [DataMember]
        public List<string> lobbyID {get; set;}

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }
    }
}