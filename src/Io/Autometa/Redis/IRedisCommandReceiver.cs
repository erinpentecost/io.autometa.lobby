namespace Io.Autometa.Redis
{
        internal interface IRedisCommandReceiver
        {

                dynamic SendCommand(RedisCommand command);
                dynamic SendCommand(string command);

                dynamic SendCommand(RedisCommand command, params string[] arguments);
                dynamic SendCommand(string command, params string[] arguments);

                dynamic SendCommand(RedisCommand command, params byte[][] arguments);
                dynamic SendCommand(string command, byte[][] arguments);
        }
}