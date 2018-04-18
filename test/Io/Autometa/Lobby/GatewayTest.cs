using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Io.Autometa.Lobby;
using Newtonsoft.Json;

namespace Io.Autometa.Lobby.Tests
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
            testRequest.HttpMethod = "POST";
            testRequest.Headers = new Dictionary<string,string>();
            testRequest.Headers["Content-Type"] = @"application/json";


            testRequest.RequestContext = testRequest.RequestContext ?? new APIGatewayProxyRequest.ProxyRequestContext();
            testRequest.RequestContext.Identity = testRequest.RequestContext.Identity ?? new APIGatewayProxyRequest.RequestIdentity();
            testRequest.RequestContext.Identity.SourceIp = "localhost";


            Message.GameClient gc  = new Message.GameClient();
            gc.nickName = "cool nickname";
            gc.ip = "localhost";
            gc.port = 7777;
            gc.game = new Message.Game();
            gc.game.id = nameof(FunctionTest);
            gc.game.api = 2;
            testRequest.Body = Newtonsoft.Json.JsonConvert.SerializeObject(gc);


            var funcResponse = function.FunctionHandler(testRequest, context);
            
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(funcResponse));
        }
    }
}
