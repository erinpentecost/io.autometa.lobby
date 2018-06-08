using System;
using System.IO;
using System.Net.Sockets;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// This class holds the actual network connection to Redis.
    /// </summary>
    internal class RedisConnection : IDisposable
    {
        private RedisOptions opt {get;}
        private Socket _cSocket;

        private BufferedStream _cStream;
        public BufferedStream stream
        {
            get
            {
                if (this._cStream == null || !this._cSocket.Connected)
                {
                    this.Connect();
                }
                return this._cStream;
            }
        }

        public RedisConnection(RedisOptions opt)
        {
            this.opt = opt;
        }

        private void Connect()
        {
            this._cSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            this._cSocket.NoDelay = true;
            this._cSocket.SendTimeout = this.opt.SendTimeout;
            this._cSocket.Connect(this.opt.Host, this.opt.Port);
            if (!this._cSocket.Connected){
                throw new IOException("socket is not connected");
            }
            this._cStream = new BufferedStream(new NetworkStream (this._cSocket), 16*1024);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    try { this._cSocket.Dispose(); } catch { }
                    try { this._cStream.Dispose(); } catch { }
                }

                this._cSocket = null;
                this._cStream = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}