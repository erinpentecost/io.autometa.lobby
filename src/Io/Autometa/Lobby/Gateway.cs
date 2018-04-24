using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Io.Autometa.LobbyContract;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Io.Autometa.Lobby
{
    public class Gateway
    {
        private static int maxBody = 4000;
        public static string lobbyMethodKey = "proxy";
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

            object mappedResponse = null;

            try
            {
                string sourceIP = input?.RequestContext?.Identity?.SourceIp;

                /// TODO null ref exception in below line
                var ivc = new ValidationCheck()
                    .Assert(input != null, "input is null")
                    .Assert(context != null, "context is null")
                    .Assert(() => !string.IsNullOrWhiteSpace(input.HttpMethod), "http method not supplied")
                    .Assert(() => string.Equals(input.HttpMethod, "post", StringComparison.InvariantCultureIgnoreCase), "only POST method is allowed")
                    .Assert(() => input.Headers != null && input.Headers.ContainsKey("Content-Type"), "Content-Type header is missing")
                    .Assert(() => string.Equals(input.Headers["Content-Type"], @"application/json", StringComparison.InvariantCultureIgnoreCase), "Content-Type header should be application/json")
                    .Assert(() => input.Body != null, "body is null")
                    .Assert(() => input.Body.Length < maxBody, "body length is too long ("+input.Body.Length+"/"+maxBody.ToString()+")")
                    .Assert(() => input.PathParameters != null, "path parameters are null")
                    .Assert(() => input.PathParameters.ContainsKey(lobbyMethodKey), "expecting path key ("+lobbyMethodKey+")")
                    .Assert(() => !string.IsNullOrWhiteSpace(input.PathParameters[lobbyMethodKey]), "path key is empty ("+lobbyMethodKey+")")
                    .Assert(() => !string.IsNullOrWhiteSpace(sourceIP), "source ip is empty");
                if (!ivc.result)
                {
                    return WrapResponse(ivc);
                }

                ILobby lobby = new RedisLobby(
                        Environment.GetEnvironmentVariable("ElasticacheConnectionString"),
                        sourceIP);

                mappedResponse =  MapToLobby(
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
                mappedResponse =  vc;
            }

            return WrapResponse(mappedResponse);
        }

        private static APIGatewayProxyResponse WrapResponse(
            object toSerialize)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(toSerialize),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Access-Control-Allow-Origin", "*" } }
            };
        }

        public object MapToLobby(ILobby lobby, string param, string body)
        {
            var method = typeof(ILobby).GetMethod(param.Trim());
            var vc = new ValidationCheck()
                .Assert(method != null, "invalid method ("+param+")");
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
