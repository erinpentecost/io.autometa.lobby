namespace Io.Autometa.Redis
{
        internal interface IRedisCommandReceiver
        {

                dynamic Send(RedisCommand command);
                dynamic Send(string command);

                dynamic Send(RedisCommand command, params string[] arguments);
                dynamic Send(string command, params string[] arguments);

                dynamic Send(RedisCommand command, params byte[][] arguments);
                dynamic Send(string command, byte[][] arguments);
        }
}