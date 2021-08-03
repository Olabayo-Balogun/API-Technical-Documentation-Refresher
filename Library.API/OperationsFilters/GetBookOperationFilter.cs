using Library.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.OperationsFilters
{
    //We need to inherit the IOperationFilter to be able to tell the two APIs apart and to run the first property shown in the code below
    public class GetBookOperationFilter: IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId != "GetBook")
            {
                return;
            }

            //This code below factors in the second API with the same name and attributes which in this case is "bookwithconcatenatedauthorname"
            operation.Responses[StatusCodes.Status200OK.ToString()].Content.Add(
                "application/vendor.marvin.bookwithconcatenatedauthorname+json", new OpenApiMediaType()
                { 
                    Schema = context.SchemaRegistry.GetOrRegister(typeof(BookWithConcatenatedAuthorName))
                });
        }

    }
}
