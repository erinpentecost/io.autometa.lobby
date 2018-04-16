using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using io.autometa.lobby;
using Newtonsoft.Json;

namespace io.autometa.lobby.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void Test()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Gateway();
            var context = new TestLambdaContext();
            APIGatewayProxyRequest testRequest = new APIGatewayProxyRequest();
            testRequest.QueryStringParameters = new Dictionary<string, string>();
            testRequest.QueryStringParameters.Add("echo", "true");
            testRequest.PathParameters = new Dictionary<string, string>();
            testRequest.PathParameters.Add(Gateway.lobbyMethodKey, "Search");


            message.GameClient gc  = new message.GameClient();
            gc.nickName = "cool nickname";
            gc.ip = "localhost";
            gc.port = 7777;
            gc.game = new message.Game();
            gc.game.id = nameof(FunctionTest);
            gc.game.api = 2;
            testRequest.Body = Newtonsoft.Json.JsonConvert.SerializeObject(gc);


            var funcResponse = function.FunctionHandler(testRequest, context);
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(funcResponse));
        }
    }
}
