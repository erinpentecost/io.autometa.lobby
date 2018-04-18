using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Io.Autometa.Redis
{
    public class RedisPipeline : IRedisCommandReceiver
    {
        internal readonly List<byte[]> commands = new List<byte[]>();

        public RedisPipeline()
        {
        }

        public dynamic SendCommand(RedisCommand command) => SendCommand(command.ToString());
        public dynamic SendCommand(string command)
        {
            commands.Add(Encoding.UTF8.GetBytes(command + RedisClient.endLineString));
            return this;
        }

        public dynamic SendCommand(RedisCommand command, params string[] arguments) => SendCommand(command.ToString(), arguments);
        public dynamic SendCommand(string command, params string[] arguments)
        {
            return SendCommand(command, arguments.Select(a => Encoding.UTF8.GetBytes(a)).ToArray());
        }

        public dynamic SendCommand(RedisCommand command, params byte[][] arguments) => SendCommand(command.ToString(), arguments);
        public dynamic SendCommand(string command, byte[][] arguments)
        {
            var sendCommand = RedisClient.BuildBinarySafeCommand(command, arguments);

            commands.Add(sendCommand);
            return this;
        }
    }
}