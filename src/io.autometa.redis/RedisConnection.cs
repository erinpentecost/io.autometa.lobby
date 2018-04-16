using System;
using System.IO;
using System.Net.Sockets;

namespace io.autometa.redis
{
    internal class RedisConnection : IDisposable
    {
        public Socket sendSocket {get; private set;}
        public BufferedStream recvStream {get; private set;}

        private RedisConnection()
        {
        }

        public static RedisConnection Start(RedisOptions opt)
        {
            RedisConnection rc = new RedisConnection();
            rc.sendSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            rc.sendSocket.NoDelay = true;
            rc.sendSocket.SendTimeout = opt.SendTimeout;
            rc.sendSocket.Connect (opt.Host, opt.Port);
            if (!rc.sendSocket.Connected){
                throw new IOException("socket is not connected");
            }
            rc.recvStream = new BufferedStream(new NetworkStream (rc.sendSocket), 16*1024);

            return rc;
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
                    this.sendSocket.Dispose();
                    this.recvStream.Dispose();
                }

                this.sendSocket = null;
                this.recvStream = null;

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