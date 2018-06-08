namespace Io.Autometa.Redis
{
        internal interface IRedisCommandReceiver
        {
                /// <summary>
                /// Parameterless command.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <returns>output for the command, as a dynamic object.</returns>
                dynamic Send(RedisCommand command);

                /// <summary>
                /// Parameterless command.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <returns>Output for the command, as a dynamic object.</returns>
                dynamic Send(string command);

                /// <summary>
                /// Command that takes in an array of strings as arguments.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <param name="arguments">Array of strings.</param>
                /// <returns>Output for the command, as a dynamic object.</returns>
                dynamic Send(RedisCommand command, params string[] arguments);

                /// <summary>
                /// Command that takes in an array of strings as arguments.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <param name="arguments">Array of strings.</param>
                /// <returns>Output for the command, as a dynamic object.</returns>
                dynamic Send(string command, params string[] arguments);

                /// <summary>
                /// Command that takes in a two-dimensional byte array as an arguments.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <param name="arguments">Two-dimensional byte array.</param>
                /// <returns>Output for the command, as a dynamic object.</returns>
                dynamic Send(RedisCommand command, params byte[][] arguments);

                /// <summary>
                /// Command that takes in a two-dimensional byte array as an arguments.
                /// </summary>
                /// <param name="command">Command to execute.</param>
                /// <param name="arguments">Two-dimensional byte array.</param>
                /// <returns>Output for the command, as a dynamic object.</returns>
                dynamic Send(string command, byte[][] arguments);
        }
}