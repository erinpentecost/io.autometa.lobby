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

namespace Io.Autometa.Lobby
{
    public class LobbyException : Exception
    {
        public LobbyException(string msg) : base(msg)
        {
        }

        public LobbyException(string msg, params string[] objs) : base(string.Format(msg, objs))
        {
        }
    }
}
