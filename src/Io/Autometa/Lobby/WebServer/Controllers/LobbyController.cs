using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Io.Autometa.Lobby.Contract;
using Io.Autometa.Lobby.Server;

namespace Io.Autometa.Lobby.WebServer.Controllers
{
    [Route("lobby")]
    public class LobbyController : Controller
    {
        [HttpPost("create")]
        [AcceptVerbs("POST")]
        ServerResponse<GameLobby> Create(CreateGameLobby newLobby)
        {
            ILobby lobby = new RedisLobby(
                    Environment.GetEnvironmentVariable("ElasticacheConnectionString"),
                    "DUMMY");

            return lobby.Create(newLobby);
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
