using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API
{
    //A convention has to be static as it applies to many classes
    public static class CustomConventions
    {
        //The default response types below will apply to any API or Controller it is called to.
        [ProducesDefaultResponseType]
        [ProducesDefaultResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType(StatusCodes.Status400BadRequest)]
        //The attribute below helps set up the name match behaviour.
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
        //The name of the method has to match the name of the API that one is passing custom conventions to.
        //In the case of this example, "Insert" is the name of the API action.
        public static void Insert(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]
            object model)
        {
             
        }
    }
}
