using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// Connection options to use when creating a new connection to Redis.
    /// Unlike the stackexchange implementation, initialization is very lightweight.
    /// </summary>
    public class RedisOptions
    {
        private string _host;
        public string Host
        {
            get { return _host; }
            set {_host = value; }
        }
        private int _port;
        public int Port
        {
            get { return _port; }
            set {_port = value; }
        }

        private int _sendTimeout;
        public int SendTimeout
        {
            get { return _sendTimeout; }
            set {_sendTimeout = value; }
        }

        private ILogger _log;
        public ILogger Log
        {
            get { return _log; }
            set {_log = value; }
        }
        

        public RedisOptions(string host = "localhost", int port = 6379, int SendTimeout = 300, ILogger log = null)
        {
            this.Host = host;
            this.Port = port;
            this.SendTimeout = SendTimeout;
            this.Log = log;
        }
    }
}