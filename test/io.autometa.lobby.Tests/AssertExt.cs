using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using io.autometa.lobby;
using Newtonsoft.Json;
using io.autometa.lobby.message;

namespace io.autometa.lobby.Tests
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
