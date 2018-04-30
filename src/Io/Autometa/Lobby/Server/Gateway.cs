using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Io.Autometa.Lobby.Contract;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Io.Autometa.Lobby.Server
{
    public class Gateway
    {
        private static int maxBody = 4000;
        public static string lobbyMethodKey = "proxy";

        private static string redirect = "http://autometa.io";

        /// <summary>
        /// Entry into application via AWS
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public object FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            try
            {
                string sourceIP = input?.RequestContext?.Identity?.SourceIp;

                var ivc = new ValidationCheck()
                    .Assert(input != null, "input is null")
                    .Assert(context != null, "context is null")
                    .Assert(() => input.Headers != null, "input.headers is null")
                    .Assert(() => !string.IsNullOrWhiteSpace(input.HttpMethod), "http method not supplied")
                    .Assert(() => string.Equals(input.HttpMethod, "post", StringComparison.InvariantCultureIgnoreCase), "only POST method is allowed")
                    .Assert(() => input.Headers.ContainsKey("Content-Type"), "Content-Type header is missing")
                    .Assert(() => string.Equals(input.Headers["Content-Type"], @"application/json", StringComparison.InvariantCultureIgnoreCase), "Content-Type header should be application/json");
                ivc.Assert(() => !string.IsNullOrWhiteSpace(input.Body), "body is null")
                    .Assert(() => input.Body.Length <= maxBody, "body length is too long (" + input.Body.Length + "/" + maxBody.ToString() + ")")
                    .Assert(() => input.PathParameters != null, "path parameters are null")
                    .Assert(() => input.PathParameters.ContainsKey(lobbyMethodKey), "expecting path key (" + lobbyMethodKey + ")")
                    .Assert(() => !string.IsNullOrWhiteSpace(input.PathParameters[lobbyMethodKey]), "path key is empty (" + lobbyMethodKey + ")")
                    .Assert(() => !string.IsNullOrWhiteSpace(sourceIP), "source ip is empty");
                if (!ivc.result)
                {
                    return WrapResponse(ivc, HttpStatusCode.Redirect);
                }

                // Handle http proxy servers. This opens up the lobby service to forgeries.
                // A way around this would be to give the host a token that they must send back
                // to the server for all host actions, but since the lobby is public and
                // exploitable anyway, I'm not really worried about people messing with it.
                // https://en.wikipedia.org/wiki/X-Forwarded-For
                if (input.Headers.ContainsKey("X-Forwarded-For"))
                {
                    sourceIP = input.Headers["X-Forwarded-For"]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .First()
                        .Trim();
                }

                ILobby lobby = new RedisLobby(
                        Environment.GetEnvironmentVariable("ElasticacheConnectionString"),
                        sourceIP);

                var lobbyResp = MapToLobby(
                    lobby,
                    input.PathParameters[lobbyMethodKey],
                    input.Body);
                return WrapResponse(lobbyResp);
            }
            catch (Exception ex)
            {
                var vc = new ValidationCheck();
                vc.result = false;
                // don't leak a stack trace
                vc.reason.Add(ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine(JsonConvert.SerializeObject(ex));
                return WrapResponse(vc, HttpStatusCode.Redirect);
            }

            return WrapResponse("oops", HttpStatusCode.Redirect);
        }

        // Ensure all responses are in the correct format
        private static APIGatewayProxyResponse WrapResponse(
            object toSerialize,
            HttpStatusCode code = HttpStatusCode.OK)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(toSerialize),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Location", redirect}
                    }
            };
        }

        /// <summary>
        /// Super basic path router based on reflection
        /// </summary>
        /// <param name="lobby">lobby instance to use</param>
        /// <param name="param">method to call on ILobby</param>
        /// <param name="body">json POST content</param>
        /// <returns></returns>
        public static object MapToLobby(ILobby lobby, string param, string body)
        {
            // Find the method in ILobby that corresponds to param
            var method = typeof(ILobby).GetMethods()
                .First(m => string.Equals(m.Name, param.Trim(), StringComparison.InvariantCultureIgnoreCase));
            var vc = new ValidationCheck()
                .Assert(method != null, "invalid method (" + param + ")");
            if (!vc.result)
            {
                return vc;
            }

            // Cast the raw POST content into the type expected by the method.
            var expectedParamType = method.GetParameters()[0].ParameterType;
            var castedBody = JsonConvert.DeserializeObject(body, expectedParamType);

            // Call the method and return its output
            return method.Invoke(lobby, new object[] { castedBody });
        }
    }
}
