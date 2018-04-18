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

        public dynamic Send(RedisCommand command) => Send(command.ToString());
        public dynamic Send(string command)
        {
            commands.Add(Encoding.UTF8.GetBytes(command + RedisClient.endLineString));
            return this;
        }

        public dynamic Send(RedisCommand command, params string[] arguments) => Send(command.ToString(), arguments);
        public dynamic Send(string command, params string[] arguments)
        {
            return Send(command, arguments.Select(a => Encoding.UTF8.GetBytes(a)).ToArray());
        }

        public dynamic Send(RedisCommand command, params byte[][] arguments) => Send(command.ToString(), arguments);
        public dynamic Send(string command, byte[][] arguments)
        {
            var sendCommand = RedisClient.BuildBinarySafeCommand(command, arguments);

            commands.Add(sendCommand);
            return this;
        }
    }
}