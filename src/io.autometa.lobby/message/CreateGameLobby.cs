using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    [DataContract]
    public class CreateGameLobby : IMessage
    {
        [DataMember]
        public GameClient owner {get; set;}

        /// This will prevent the lobby from showing up on the search list.
        /// GameClients will need to know the lobbyID to make a direct connect.
        /// This is essentially a password-protected / non-public game.
        [DataMember]
        public bool hidden {get; set;}

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Compose(this.owner != null, "game client null")
            .Compose(this.owner.Validate);
        }
    }
}