namespace Io.Autometa.Lobby.Server
{
    public class LobbyException : System.Exception
    {
        public int HttpCode = 500;
        public LobbyException() { }
        public LobbyException(int httpCode, string message) : base(message)
        {
            this.HttpCode = httpCode;
        }
        public LobbyException(int httpCode, string message, System.Exception inner) : base(message, inner)
        {
            this.HttpCode = httpCode;
        }
        protected LobbyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}