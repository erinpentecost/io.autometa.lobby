using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Io.Autometa.Redis
{
    public class RedisPipeline
    {
        internal readonly List<Tuple<byte[], Func<byte[], object>>> commands = new List<Tuple<byte[], Func<byte[], object>>>();

        private RedisPipeline()
        {
        }

        public RedisPipeline QueueCommand(string command)
        {
            commands.Add(Tuple.Create(Encoding.UTF8.GetBytes(command + RedisClient.endLineString), (Func<byte[], object>)null));
            return this;
        }

        public RedisPipeline QueueCommand(string command, Func<byte[], object> binaryDecoder)
        {
            commands.Add(Tuple.Create(Encoding.UTF8.GetBytes(command + RedisClient.endLineString), binaryDecoder));
            return this;
        }

        public RedisPipeline QueueCommand(string command, params byte[][] arguments)
        {
            return QueueCommand(command, arguments, null);
        }

        public RedisPipeline QueueCommand(string command, byte[][] arguments, Func<byte[], object> binaryDecoder)
        {
            var sendCommand = RedisClient.BuildBinarySafeCommand(command, arguments);

            commands.Add(Tuple.Create(sendCommand, binaryDecoder));
            return this;
        }
    }
}