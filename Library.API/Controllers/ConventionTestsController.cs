using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    //The line of code declares Microsoft's defaut API conventions which handles the general error response types
    //It is important to note that none of these will take effect as long as there's a global or method level attribute being declared.
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ConventionTestsController : ControllerBase
    {
        // GET: api/<ConventionTestsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ConventionTestsController>/5
        [HttpGet("{id}", Name = "Get")]
        
        //The line of code below shows how to declare default API conventions at method level.
        //The "nameof" part is where you declare the DefaultAPIConventions depending on the action (get, post, put, patch, etc)
        //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ConventionTestsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ConventionTestsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ConventionTestsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
