using System;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Message
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
            .Assert(ValidationCheck.BasicStringCheck(this.ip, "ip"))
            .Assert(this.port >= 100, "port < 100")
            .Assert(this.game != null, "game is null")
            .Assert(this.game.Validate);
        }
    }
}