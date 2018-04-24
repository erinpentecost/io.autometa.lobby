using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Io.Autometa.LobbyContract
{
    [DataContract]
    public class ReadRequest : IMessage
    {
        [DataMember]
        public string lobbyId {get; set;}

        [DataMember]
        public Game game {get; set;}

        public ReadRequest(string lobbyId, Game game)
        {
            this.lobbyId = lobbyId;
            this.game = game;
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Assert(ValidationCheck.BasicStringCheck(this.lobbyId, "lobbyID"))
            .Assert(game.Validate);
        }
    }
}