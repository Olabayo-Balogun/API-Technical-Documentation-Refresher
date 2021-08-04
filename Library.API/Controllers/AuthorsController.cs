﻿using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Produces("appication/json", "application/xml")]
    //[Route("api/authors")]
    
    //The code below is how we route API versions
    [Route("api/v1.0/authors")]
    //As of the time of the writing of this code, swashbuckle doesn't quickly recognize the versions of two APIs which makes it think that the APIs are the same. This is why magic strings are used above.
    //[Route("api/v{version:apiversion}/authors")]
    //The "ApiExplorerSettings" has the "GroupName" feature which helps to specify where all the actions in a controller belong
    //The "ApiExplorerSettings" can also be declared at action/API level.
    //Note that the "GroupName" must be the same as the SwaggerDoc name in the startup class
    //Note that it may clash with API versioning
    //[ApiExplorerSettings(GroupName = "LibraryOpenAPISpecificationAuthors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorsRepository;
        private readonly IMapper _mapper;

        public AuthorsController(
            IAuthorRepository authorsRepository,
            IMapper mapper)
        {
            _authorsRepository = authorsRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            var authorsFromRepo = await _authorsRepository.GetAuthorsAsync();
            return Ok(_mapper.Map<IEnumerable<Author>>(authorsFromRepo));
        }

        //Generating parameters for is done by typing the "/" three consecutive times and the code snippet below is generated. 
        /// <summary>
        ///  Get an author by his/her id
        /// </summary>
        /// <param name="authorId">The id of the author you want to get</param>
        /// <returns>An ActionResult of type "author"</returns>
        [HttpGet("{authorId}")]
        public async Task<ActionResult<Author>> GetAuthor(
            Guid authorId)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Author>(authorFromRepo));
        }

        [HttpPut("{authorId}")]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            AuthorForUpdate authorForUpdate)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(authorForUpdate, authorFromRepo);

            //// update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo)); 
        }

        /// <summary>
        ///     Partially update an author
        /// </summary>
        /// <param name="authorId">The Id of the author you want to get</param>
        /// <param name="patchDocument">The set of operations to apply to the author</param>
        /// <returns>An ActionResult of type "Author"</returns>
        // The "remarks" code is used to add an example of what the code does
        // Also note that adding "\" to XML initiates a line break
        /// <remarks>
        /// Sample request (this request updates the author's first name) \
        /// PATCH /author/id \
        /// [ \
        ///     { \
        ///         "op": "replace", \
        ///         "path": "/firstname", \
        ///         "value": "new first name" \
        ///     } \
        /// ] \
        /// </remarks>

        [HttpPatch("{authorId}")]
        public async Task<ActionResult<Author>> UpdateAuthor(
            Guid authorId,
            JsonPatchDocument<AuthorForUpdate> patchDocument)
        {
            var authorFromRepo = await _authorsRepository.GetAuthorAsync(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }

            // map to DTO to apply the patch to
            var author = _mapper.Map<AuthorForUpdate>(authorFromRepo);
            patchDocument.ApplyTo(author, ModelState);

            // if there are errors when applying the patch the patch doc 
            // was badly formed  These aren't caught via the ApiController
            // validation, so we must manually check the modelstate and
            // potentially return these errors.
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // map the applied changes on the DTO back into the entity
            _mapper.Map(author, authorFromRepo);

            // update & save
            _authorsRepository.UpdateAuthor(authorFromRepo);
            await _authorsRepository.SaveChangesAsync();

            // return the author
            return Ok(_mapper.Map<Author>(authorFromRepo));
        }
    }
}
