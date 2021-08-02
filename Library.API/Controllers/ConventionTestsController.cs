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
    //[ApiConventionType(typeof(DefaultApiConventions))]

    //The code snippet below shows how we declare CustomConventions at the API level.
    //Note that the specificity rule applies all the same.
    //Also note that declaring custom conventions at the controller level won't match with API methods that don't match the method name of the custom conventions.
    [ApiConventionType(typeof(CustomConventions))]
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
        //We're changing the "Post" request to "Insert" in order to learn how to create a custom API documentation convention
        [HttpPost]
        //public void Post([FromBody] string value){}

        //Just as we did in line 31 for calling default convention, we call the custom convention in the same way.
        //Note that in calling the custom convention, it must always end with the name of the method
        //Rather than writing "DefaultApiConventions", we write the name of the convention class, in this case, it is "CustomConventions".
        //This custom API convention also works on any API it is declared on regardless of if the method name matches the API convention name or not. This rule doesn't apply at controller level though.
        //[ApiConventionMethod(typeof(CustomConventions), nameof(CustomConventions.Insert))]
        public void Insert([FromBody] string value) 
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
