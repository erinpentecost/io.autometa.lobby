using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using Io.Autometa.Lobby;
using Newtonsoft.Json;
using Io.Autometa.Lobby.Message;

namespace Io.Autometa.Lobby.Tests
{
    public static class AssertExt
    {
        public static void Valid<T>(ServerResponse<T> r)
            where T : class, IMessage
        {
            Xunit.Assert.True(r.valid.result, r.valid.ToString());
        }
    }
}
