using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    /// This is an instance of an actual game lobby.
    [DataContract]
    public class GameLobby : IMessage
    {
        private static int maxMetaDataCount = 100;

        /// Lobby id that the game client is currently in.
        [DataMember]
        public string lobbyID {get; set;}

        [DataMember]
        public Game game {get; set;}

        [DataMember]
        public GameClient host {get; set;}

        /// List of all clients in a lobby
        [DataMember]
        public List<GameClient> clients {get; set;}

        /// Start time of lobby creation (not game start time)
        [DataMember]
        public DateTime creationTime {get; set;}

        /// <summary>
        /// Arbitrary metadata. Ignored by the lobby system.
        /// </summary>
        [DataMember]
        public Dictionary<string,string> metaData {get; set;}

        /// This will prevent the lobby from showing up on the search list.
        /// GameClients will need to know the lobbyID to make a direct connect.
        /// This is essentially a password-protected / non-public game.
        [DataMember]
        public bool hidden {get; set;}

        public ValidationCheck Validate()
        {
            var vc = new ValidationCheck()
            .Assert(ValidationCheck.BasicStringCheck(this.lobbyID, "lobbyID"))
            .Assert(host == null, "no host")
            .Assert(host.Validate)
            .Assert(metaData == null ? true : metaData.Count < maxMetaDataCount, "too many items in metadata ("+metaData.Count+"/"+maxMetaDataCount+")")
            .Assert(creationTime < DateTime.UtcNow.AddDays(1), "creation time is in the future");

            if (metaData != null)
            {
                foreach (var kv in metaData)
                {
                    if (!vc.result)
                    {
                        break;
                    }
                    vc.Assert(ValidationCheck.BasicStringCheck(kv.Key, "metaData key"));
                    vc.Assert(ValidationCheck.BasicStringCheck(kv.Value, "metaData value"));
                }
            }

            return vc;
        }
    }
}