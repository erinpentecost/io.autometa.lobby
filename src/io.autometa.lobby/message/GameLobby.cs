using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    /// This is an instance of an actual game lobby.
    [DataContract]
    public class GameLobby : IMessage
    {
        /// Lobby id that the game client is currently in.
        [DataMember]
        public string lobbyID {get; set;}

        /// List of all clients in a lobby
        [DataMember]
        public List<GameClient> clients {get; set;}

        /// Start time of lobby creation (not game start time)
        [DataMember]
        public DateTime creationTime {get; set;}

        /// No new clients can be added when a lobby is locked.
        /// This will prevent the lobby from showing up on the search list.
        [DataMember]
        public bool locked {get; set;}

        /// This will prevent the lobby from showing up on the search list.
        /// GameClients will need to know the lobbyID to make a direct connect.
        /// This is essentially a password-protected / non-public game.
        [DataMember]
        public bool hidden {get; set;}

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Compose(ValidationCheck.BasicStringCheck(this.lobbyID, "lobbyID"))
            .Compose(creationTime < DateTime.UtcNow.AddDays(1), "creation time is in the future");
        }
    }
}