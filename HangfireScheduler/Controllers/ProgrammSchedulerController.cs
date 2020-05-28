using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HangfireScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgrammSchedulerController : ControllerBase
    {
        // GET: api/<ProgrammSchedulerController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ProgrammSchedulerController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ProgrammSchedulerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ProgrammSchedulerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProgrammSchedulerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
