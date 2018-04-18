using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Io.Autometa.Redis
{
    public class RedisClient : IDisposable, IRedisCommandReceiver
    {
        internal RedisConnection con {get;}
        private ILogger log;

        internal const string endLineString = "\r\n";
        internal static byte[] endLineBytes = Encoding.UTF8.GetBytes(endLineString);

        public RedisClient(RedisOptions opt, ILogger log = null)
        {
            this.con = new RedisConnection(opt);
            this.log = log;
        }

        public bool Send(string cmd, params string[] args)
        {
            MemoryStream buff = new MemoryStream();
            buff.WriteLine("*" + (1 + args.Length).ToString());
            buff.WriteLine("$" + cmd.Length + "\r\n" + cmd);

            foreach (object arg in args) {
                string argStr = arg.ToString ();
                int argStrLength = Encoding.UTF8.GetByteCount(argStr);
                buff.WriteLine("$" + argStrLength + "\r\n" + argStr);
            }

            this.log?.LogTrace("Sending "+cmd+" to redis.");
            buff.CopyTo(con.stream);

            return true;
        }

        internal static byte[] BuildBinarySafeCommand(string command, byte[][] arguments)
        {
            var firstLine = Encoding.UTF8.GetBytes((char)RespType.Arrays + (arguments.Length + 1).ToString() + endLineString);
            var secondLine = Encoding.UTF8.GetBytes((char)RespType.BulkStrings + Encoding.UTF8.GetBytes(command).Length.ToString() + endLineString + command + endLineString);
            var thirdLine = arguments.Select(x =>
            {
                var head = Encoding.UTF8.GetBytes((char)RespType.BulkStrings + x.Length.ToString() + endLineString);
                return head.Concat(x).Concat(Encoding.UTF8.GetBytes(endLineString)).ToArray();
            })
            .ToArray();

            return new[] { firstLine, secondLine }.Concat(thirdLine).SelectMany(xs => xs).ToArray();
        }

        internal dynamic FetchResponse()
        {
            var type = (RespType)con.stream.ReadByte();
            switch (type)
            {
                case RespType.SimpleStrings:
                    {
                        var result = con.stream.ReadFirstLine();
                        return result;
                    }
                case RespType.Errors:
                    {
                        var result = con.stream.ReadFirstLine();
                        return result;
                    }
                case RespType.Integers:
                    {
                        var line = con.stream.ReadFirstLine();
                        return Decoders.ParseInt64(line);
                    }
                case RespType.BulkStrings:
                    {
                        var line = con.stream.ReadFirstLine();
                        var length = int.Parse(line);
                        if (length == -1)
                        {
                            return string.Empty;
                        }
                        var buffer = new byte[length];
                        con.stream.Read(buffer, 0, length);

                        con.stream.ReadFirstLine(); // read terminate

                        return buffer;
                    }
                case RespType.Arrays:
                    {
                        var line = con.stream.ReadFirstLine();
                        var length = int.Parse(line);

                        if (length == 0)
                        {
                            return new dynamic[0];
                        }
                        if (length == -1)
                        {
                            return null;
                        }

                        var objects = new dynamic[length];

                        for (int i = 0; i < length; i++)
                        {
                            objects[i] = FetchResponse();
                        }

                        return objects;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public dynamic SendCommand(RedisCommand command) => SendCommand(command.ToString());
        public dynamic SendCommand(string command)
        {
            // Request
            con.stream.WriteLine(command);

            // Response
            return FetchResponse();
        }

        public dynamic SendCommand(RedisCommand command, params string[] arguments) => SendCommand(command.ToString(), arguments);
        public dynamic SendCommand(string command, params string[] arguments)
        {
            return SendCommand(command, arguments.Select(a => Encoding.UTF8.GetBytes(a)).ToArray());
        }
        
        public dynamic SendCommand(RedisCommand command, params byte[][] arguments) => SendCommand(command.ToString(), arguments);
        public dynamic SendCommand(string command, byte[][] arguments)
        {
            var sendCommand = BuildBinarySafeCommand(command, arguments);

            // Request
            con.stream.Write(sendCommand, 0, sendCommand.Length);

            // Response
            return FetchResponse();
        }

        public dynamic[] SendCommand(RedisPipeline command)
        {
            var encoded = command.commands.SelectMany(x => x).ToArray();
            // Request
            this.con.stream.Write(encoded, 0, encoded.Length);

            // Response
            var result = new object[command.commands.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this.FetchResponse();
            }

            return result;
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
