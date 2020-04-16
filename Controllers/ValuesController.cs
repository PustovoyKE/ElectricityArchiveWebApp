using System.Collections.Generic;
using ElectricityArchiveWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElectricityArchiveWebApp.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] {"value1", "value2"};
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public ActionResult<Person> Post([FromBody] Person person)
        {
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}