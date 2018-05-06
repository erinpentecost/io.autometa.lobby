using System;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Server.Contract
{
    /// This is an instance of a game that a user is running.
    [DataContract]
    public class Client
    {
        /// <summary>
        /// IP address for the game client.
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public string ip {get; set;}

        /// <summary>
        /// Communication port for the peer-to-peer connection.
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public int port {get; set;}

        /// <summary>
        /// User-configurable nickname to identify the game client.
        /// This must be unique per lobby.
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public string name {get; set;}

        /// <summary>
        /// User unique id based on ip + port.
        /// </summary>
        /// <returns></returns>
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
            .Assert(this.port >= 100, "port < 100");
        }
    }
}