using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace io.autometa.lobby
{
    public class Gateway
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<object> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            /*
            event['pathParameters']['param1']
            event["queryStringParameters"]['queryparam1']
            event['requestContext']['identity']['userAgent']
            event['requestContext']['identity']['sourceIP']
             */



            ILobby lobby = new EchoLobby(input);

            var connectionString = Environment.GetEnvironmentVariable("ElasticacheConnectionString");


            return lobby.CreateLobby(null);
        }
    }
}
