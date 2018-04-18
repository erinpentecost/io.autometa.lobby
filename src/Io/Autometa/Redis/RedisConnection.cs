using System;
using System.IO;
using System.Net.Sockets;

namespace Io.Autometa.Redis
{
    internal class RedisConnection : IDisposable
    {
        private RedisOptions opt {get;}
        private Socket _sendSocket;
        private Socket sendSocket
        {
            get
            {
                if (this._sendSocket == null || !this._sendSocket.Connected)
                {
                    this.Connect();
                }
                return this._sendSocket;
            }
        }

        private BufferedStream _recvStream;
        private BufferedStream recvStream
        {
            get
            {
                if (this._recvStream == null || !this._sendSocket.Connected)
                {
                    this.Connect();
                }
                return this._recvStream;
            }
        }

        public RedisConnection(RedisOptions opt)
        {
            this.opt = opt;
        }

        public int Send(byte[] message)
        {
            return this.sendSocket.Send(message);
        }

        public byte[] Receive()
        {
            throw new NotImplementedException();
        }

        private void Connect()
        {
            this._sendSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            this._sendSocket.NoDelay = true;
            this._sendSocket.SendTimeout = this.opt.SendTimeout;
            this._sendSocket.Connect(this.opt.Host, this.opt.Port);
            if (!this._sendSocket.Connected){
                throw new IOException("socket is not connected");
            }
            this._recvStream = new BufferedStream(new NetworkStream (this._sendSocket), 16*1024);
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
                    try { this._sendSocket.Dispose(); } catch { }
                    try { this._recvStream.Dispose(); } catch { }
                }

                this._sendSocket = null;
                this._recvStream = null;

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