using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    /// This is an instance of a game that a user is running.
    [DataContract]
    public class GameClient : IMessage
    {
        /// IP address for the game client.
        /// This may be IPv4 or IPv6 or whatever.
        [DataMember]
        public string ip {get; set;}

        /// Communication port for the peer-to-peer connection.
        [DataMember]
        public int port {get; set;}

        /// Which game is this client using?
        [DataMember]
        public Game game {get; set;}

        /// User-configurable nickname to identify the game client.
        [DataMember]
        public string nickName {get; set;}

        /// Lobby id that the game client is currently in.
        [DataMember]
        public string lobbyID {get; set;}

        /// True if this is the host/lobby owner.!--
        /// Only the lobbyOwner can lock down a lobby.
        [DataMember]
        public bool lobbyOwner {get; set;}

        /// User unique id
        public string uid
        {
            get
            {
                return ip+":"+port.ToString();
            }
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck()
            .Compose(ValidationCheck.BasicStringCheck(this.ip))
            .Compose(this.port < 100, "port < 100")
            .Compose(ValidationCheck.BasicStringCheck(this.lobbyID))
            .Compose(this.game != null, "game is null")
            .Compose(this.game.Validate);
        }
    }
}