using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using io.autometa.lobby.message;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace io.autometa.lobby
{
    public class Gateway
    {
        private static int maxBody = 4000;
        public static string lobbyMethodKey = "lobbyMethod";
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public object FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            /*
            event['pathParameters']['param1']
            event["queryStringParameters"]['queryparam1']
            event['requestContext']['identity']['userAgent']
            event['requestContext']['identity']['sourceIP']
             */
             //https://mynkc1sp17.execute-api.us-west-2.amazonaws.com/lobby/lobby



            try
            {
                string sourceIP = input?.RequestContext?.Identity?.SourceIp;

                var ivc = new ValidationCheck()
                    .Compose(string.Equals(input.HttpMethod, "post", StringComparison.InvariantCultureIgnoreCase), "only POST method is allowed")
                    .Compose(input.Headers != null && input.Headers.ContainsKey("Content-Type"), "Content-Type header is missing")
                    .Compose(() => string.Equals(input.Headers["Content-Type"], @"application/json", StringComparison.InvariantCultureIgnoreCase), "Content-Type header should be application/json")
                    .Compose(input.Body.Length < maxBody, "body length is too long ("+input.Body.Length+"/"+maxBody.ToString()+")")
                    .Compose(input.PathParameters != null, "path parameters are null")
                    .Compose(() => input.PathParameters.ContainsKey(lobbyMethodKey), "expecting path key ("+lobbyMethodKey+")")
                    .Compose(() => !string.IsNullOrWhiteSpace(input.PathParameters[lobbyMethodKey]), "path key is empty ("+lobbyMethodKey+")")
                    .Compose(() => !string.IsNullOrWhiteSpace(sourceIP), "source ip is empty");
                if (!ivc.result)
                {
                    return ivc;
                }

                ILobby lobby = null;

                if (input.QueryStringParameters.ContainsKey("echo") &&
                    string.Equals(input.QueryStringParameters["echo"], "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    lobby = new EchoLobby(input);
                }
                else
                {
                    lobby = new RedisLobby(
                        Environment.GetEnvironmentVariable("ElasticacheConnectionString"),
                        sourceIP);
                }

                return MapToLobby(
                    lobby,
                    input.PathParameters[lobbyMethodKey],
                    input.Body);
            }
            catch (Exception ex)
            {
                var vc = new ValidationCheck();
                vc.result = false;
                // don't leak a stack trace
                vc.reason.Add(ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine(JsonConvert.SerializeObject(ex));
                return vc;
            }
        }

        public object MapToLobby(ILobby lobby, string param, string body)
        {
            var method = typeof(ILobby).GetMethod(param.Trim());
            var vc = new ValidationCheck()
                .Compose(method != null, "invalid method ("+param+")");
            if (!vc.result)
            {
                return vc;
            }

            var expectedParamType = method.GetParameters()[0].ParameterType;
            var castedBody = JsonConvert.DeserializeObject(body, expectedParamType);

            return method.Invoke(lobby, new object[]{castedBody});
        }
    }
}
