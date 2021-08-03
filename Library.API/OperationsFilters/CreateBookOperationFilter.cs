using Library.API.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.OperationsFilters
{
    //We need to inherit the IOperationFilter to be able to tell the two APIs apart and to run the first property shown in the code below
    public class CreateBookOperationFilter: IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId != "CreateBook")
            {
                return;
            }

            //This code below factors in the second API with the same name and attributes which in this case is "bookforcreationwithamountofpages"
            operation.RequestBody.Content.Add(
                "application/vendor.marvin.bookforcreationwithamountofpages+json",
                new OpenApiMediaType()
                {
                    Schema = context.SchemaRegistry.GetOrRegister(
                    typeof(BookForCreationWithAmountOfPages))

                });
        }

        
    }
}
