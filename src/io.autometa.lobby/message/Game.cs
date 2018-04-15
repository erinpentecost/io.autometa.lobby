using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    /// This is a game API definition.
    [DataContract]
    public class Game : IMessage
    {
        /// The lobby can support multiple games.
        /// This needs to be unique per game.
        [DataMember]
        public string id {get;set;}

        /// This should be the version of the game.
        [DataMember]
        public int api {get; set;}

        /// Game unique id
        public string gid
        {
            get
            {
                return id+"v"+api.ToString();
            }
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Compose(ValidationCheck.BasicStringCheck(this.id, "id"));
        }
    }
}