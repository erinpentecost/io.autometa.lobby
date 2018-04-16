using System;
using System.IO;
using System.Net.Sockets;

namespace io.autometa.redis
{
    public class RedisOptions
    {
        private string _host = "localhost";
        public string Host => this.Host;
        private int _port = 6379;
        public int Port => this._port;

        private int _sendTimeout = 10000;
        public int SendTimeout => this._sendTimeout;
    }
}