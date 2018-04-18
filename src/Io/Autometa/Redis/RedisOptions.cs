using System;
using System.IO;
using System.Net.Sockets;

namespace Io.Autometa.Redis
{
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

        public RedisOptions(string host = "localhost", int port = 6379, int SendTimeout = 300)
        {
            this.Host = host;
            this.Port = port;
            this.SendTimeout = SendTimeout;
        }
    }
}