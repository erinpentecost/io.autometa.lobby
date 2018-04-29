using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    [DataContract]
    public class CreateGameLobbyRequest : IMessage
    {
        [DataMember]
        public Client owner {get; set;}

        [DataMember]
        public Game gameType {get; set;}

        /// <summary>
        /// This will prevent the lobby from showing up on the search list.
        /// GameClients will need to know the lobbyID to make a direct connect.
        /// This is essentially a password-protected / non-public game.
        /// </summary>
        /// <returns>true if the game is hidden</returns>
        [DataMember]
        public bool hidden {get; set;}

        /// <summary>
        /// Arbitrary metadata. Ignored by the lobby system.
        /// </summary>
        [DataMember]
        public Dictionary<string,string> metaData {get; set;}

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Assert(this.owner != null, "game client null")
            .Assert(this.owner.Validate);
        }
    }
}