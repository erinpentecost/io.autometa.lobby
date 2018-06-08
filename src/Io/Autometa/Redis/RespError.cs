using System;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// This is an error returned by Redis.
    /// I didn't want this to be confused for a string return type.
    /// </summary>
    public class RespError
    {
        private string msg {get; }
        
        public RespError(string msg)
        {
            this.msg = msg;
        }

        public override string ToString()
        {
            return msg;
        }

        public static implicit operator string(RespError err)
        {
            return err.msg;
        }

        public static implicit operator RespError(string msg)
        {
            return new RespError(msg);
        }
    }
}