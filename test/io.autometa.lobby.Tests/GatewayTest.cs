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
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Gateway();
            var context = new TestLambdaContext();
            APIGatewayProxyRequest testRequest = new APIGatewayProxyRequest();
            testRequest.QueryStringParameters = new Dictionary<string, string>();
            testRequest.QueryStringParameters.Add("whatever", "dude");
            testRequest.PathParameters = new Dictionary<string, string>();
            testRequest.PathParameters.Add("yeah", "whatever");
            var funcResponse = function.FunctionHandler(testRequest, context);
            //Console.WriteLine(JsonConvert.SerializeObject(funcResponse, Formatting.Indented));
        }
    }
}
