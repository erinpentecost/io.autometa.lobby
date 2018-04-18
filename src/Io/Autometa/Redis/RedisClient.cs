using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Io.Autometa.Redis
{
    public class RedisClient : IDisposable
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

        internal object FetchResponse(Func<byte[], object> binaryDecoder)
        {
            var type = (RespType)con.stream.ReadByte();
            switch (type)
            {
                case RespType.SimpleStrings:
                    {
                        var result = con.stream.ReadFirstLine();
                        return result;
                    }
                case RespType.Erorrs:
                    {
                        var result = con.stream.ReadFirstLine();
                        return result;
                    }
                case RespType.Integers:
                    {
                        var line = con.stream.ReadFirstLine();
                        return long.Parse(line);
                    }
                case RespType.BulkStrings:
                    {
                        var line = con.stream.ReadFirstLine();
                        var length = int.Parse(line);
                        if (length == -1)
                        {
                            return null;
                        }
                        var buffer = new byte[length];
                        con.stream.Read(buffer, 0, length);

                        con.stream.ReadFirstLine(); // read terminate

                        if (binaryDecoder == null)
                        {
                            return buffer;
                        }
                        else
                        {
                            return binaryDecoder(buffer);
                        }
                    }
                case RespType.Arrays:
                    {
                        var line = con.stream.ReadFirstLine();
                        var length = int.Parse(line);

                        if (length == 0)
                        {
                            return new object[0];
                        }
                        if (length == -1)
                        {
                            return null;
                        }

                        var objects = new object[length];

                        for (int i = 0; i < length; i++)
                        {
                            objects[i] = FetchResponse(binaryDecoder);
                        }

                        return objects;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object SendCommand(string command)
        {
            return SendCommand(command, (Func<byte[], object>)null);
        }

        public object SendCommand(string command, Func<byte[], object> binaryDecoder)
        {
            // Request
            con.stream.WriteLine(command);

            // Response
            return FetchResponse(binaryDecoder);
        }

        public object SendCommand(string command, params byte[][] arguments)
        {
            return SendCommand(command, arguments, null);
        }

        public object SendCommand(string command, byte[][] arguments, Func<byte[], object> binaryDecoder)
        {
            var sendCommand = BuildBinarySafeCommand(command, arguments);

            // Request
            con.stream.Write(sendCommand, 0, sendCommand.Length);

            // Response
            return FetchResponse(binaryDecoder);
        }

        public object[] SendCommand(RedisPipeline command)
        {
            var encoded = command.commands.SelectMany(x => x.Item1).ToArray();
            // Request
            this.con.stream.Write(encoded, 0, encoded.Length);

            // Response
            var result = new object[command.commands.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this.FetchResponse(command.commands[i].Item2);
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
