using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Io.Autometa.Redis
{
    /// <summary>
    /// This class is not thread-safe, and the methods it exposes are
    /// not re-entrant.
    /// </summary>
    public class RedisClient : IDisposable, IRedisCommandReceiver
    {
        internal RedisConnection con { get; }
        private ILogger log;

        internal const string endLineString = "\r\n";
        internal static byte[] endLineBytes = Encoding.UTF8.GetBytes(endLineString);

        public RedisClient(RedisOptions opt)
        {
            this.con = new RedisConnection(opt);
            this.log = opt.Log;
        }

        /// this makes a bulk array
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

        /// <summary>
        /// Borrowed heavily from https://github.com/neuecc/RespClient/blob/master/RespClient/Cmdlet/Cmdlets.cs
        /// https://redis.io/topics/protocol
        /// </summary>
        /// <returns>string, resperror, int, bulk string, array</returns>
        internal dynamic FetchResponse()
        {
            var type = (RespType)con.stream.ReadByte();
            switch (type)
            {
                case RespType.SimpleStrings:
                    {
                        return con.stream.ReadFirstLine();
                    }
                case RespType.Errors:
                    {
                        return new RespError(con.stream.ReadFirstLine());
                    }
                case RespType.Integers:
                    {
                        var line = con.stream.ReadFirstLine();
                        return Convert.ParseInt64(line);
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

        public dynamic Send(RedisCommand command) => Send(command.ToString());
        public dynamic Send(string command)
        {
            this.log?.LogTrace("Send {0}", command);

            // Request
            con.stream.WriteLine(command);

            // Response
            return FetchResponse();
        }

        public dynamic Send(RedisCommand command, params string[] arguments) => Send(command.ToString(), arguments);
        public dynamic Send(string command, params string[] arguments)
        {
            this.log?.LogTrace("Send {0} {1}", command, string.Join(" ", arguments));

            return Send(command, arguments.Select(a => Encoding.UTF8.GetBytes(a)).ToArray());
        }

        public dynamic Send(RedisCommand command, params byte[][] arguments) => Send(command.ToString(), arguments);
        public dynamic Send(string command, byte[][] arguments)
        {
            var sendCommand = BuildBinarySafeCommand(command, arguments);

            this.log?.LogTrace("Send {0} {1}", command, string.Join(" ", Encoding.UTF8.GetString(sendCommand)));

            // Request
            con.stream.Write(sendCommand, 0, sendCommand.Length);

            // Response
            return FetchResponse();
        }

        /// <summary>
        /// Send pipelined commands to Redis.
        /// </summary>
        /// <param name="command">Pipeline object.</param>
        /// <returns>a parallel array of responses to the commands in the pipeline</returns>
        public dynamic[] Send(RedisPipeline command)
        {
            var encoded = command.commands.SelectMany(x => x).ToArray();

            this.log?.LogTrace("Send Pipeline {1}", command.ToString());

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

        /// <summary>
        /// You will probably want to flatten this output.
        /// It's just situated like this so the enumerable can be quit without breaking everything else.
        /// https://redis.io/commands/scan
        /// </summary>
        /// <param name="cmd">SCAN-type command to use</param>
        /// <param name="match">pattern to match</param>
        /// <returns></returns>
        public IEnumerable<List<string>> Scan(RedisCommand cmd = RedisCommand.SCAN, string match = null)
        {
            // this class is not thread safe; the socket is re-used for all commands.
            // this introduces issues when sateful connections must be made, and
            // scan is one of those calls.
            // to account for this, I return elements in batches in the same exact
            // batch sizes that Redis returns them.
            long cursor = 0;
            dynamic[] resp;
            do
            {
                if (match != null)
                {
                    resp = this.Send(cmd, cursor.ToString(), "MATCH", match);
                }
                else
                {
                    resp = this.Send(cmd, cursor.ToString());
                }

                // the first element in resp is the number of elements we've read
                // so far. 0 is a magic value that indicates that no further
                // calls to scan will return additional elements.
                cursor = Redis.Convert.ParseInt64(resp[0]);

                // the second element in resp is a two-dimensional byte array,
                // which holds 0 or more elements from our scan.
                List<string> searchRes = new List<string>();
                foreach (byte[] b in resp[1])
                {
                    searchRes.Add(Encoding.UTF8.GetString(b));
                }

                yield return searchRes;
            }
            while (cursor != 0);
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
                    this.con.Dispose();
                }

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
