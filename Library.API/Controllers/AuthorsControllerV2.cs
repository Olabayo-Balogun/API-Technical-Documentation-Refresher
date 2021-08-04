using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Produces("appication/json", "application/xml")]
    //[Route("api/authors")]

    //The code below is how we route API versions
    [Route("api/v2.0/authors")]

    //As of the time of the writing of this code, swashbuckle doesn't quickly recognize the versions of two APIs which makes it think that the APIs are the same. This is why magic strings are used above.
    //[Route("api/v{version:apiversion}/authors")]
    [ApiVersion("2.0")]

    //The "ApiExplorerSettings" has the "GroupName" feature which helps to specify where all the actions in a controller belong
    //The "ApiExplorerSettings" can also be declared at action/API level.
    //Note that the "GroupName" must be the same as the SwaggerDoc name in the startup class
    //Note that it may clash with API versioning
    //[ApiExplorerSettings(GroupName = "LibraryOpenAPISpecificationAuthors")]
    [ApiController]
    public class AuthorsControllerV2 : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsControllerV2(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository;
            _mapper = mapper;
        }

        /// <summary>
        ///     Get the authors (V2)
        /// </summary>
        /// <returns>An ActionResult of type IEnumerable of Author</returns>
        /// <response code="200">Returns the list of authors</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }
    }
}
