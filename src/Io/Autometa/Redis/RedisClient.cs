using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Io.Autometa.Redis
{
    public class RedisClient : IDisposable
    {
        private RedisConnection con {get;}
        private ILogger log;

        public RedisClient(RedisOptions opt, ILogger log)
        {
            this.con = new RedisConnection(opt);
            this.log = log;
        }

        public bool Send(string cmd, params object [] args)
        {
            string resp = "*" + (1 + args.Length).ToString () + "\r\n";
            resp += "$" + cmd.Length + "\r\n" + cmd + "\r\n";
            foreach (object arg in args) {
                string argStr = arg.ToString ();
                int argStrLength = Encoding.UTF8.GetByteCount(argStr);
                resp += "$" + argStrLength + "\r\n" + argStr + "\r\n";
            }

            byte [] r = Encoding.UTF8.GetBytes (resp);
            this.log.LogTrace(resp);
            con.Send(r);
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RedisClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
