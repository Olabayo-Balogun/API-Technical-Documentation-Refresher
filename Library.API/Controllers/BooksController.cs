using AutoMapper;
using Library.API.Attributes;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
        //The attribute below declares the return type of the controller
        [Produces("application/json", "application/xml")]
        [Route("api/authors/{authorId}/books")]
        //The "ApiExplorerSettings" has the "GroupName" feature which helps to specify where all the actions in a controller belong
        //The "ApiExplorerSettings" can also be declared at action/API level.
        //Note that the "GroupName" must be the same as the SwaggerDoc name in the startup class
        [ApiExplorerSettings(GroupName = "LibraryOpenAPISpecificationBooks")]
        [ApiController]
        //Adding general response types at the controller level saves you from having to repeat it for each API.
        //Only add API response types that apply to all APIs (within the controller) at the controller level
        //If response types are declared globally in the startup class, there's no use for repeating it at the controller level
        /*[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]*/
        public class BooksController : ControllerBase
    { 
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }
       
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        Guid authorId )
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId); 
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }

        /// <summary>
        ///     Get a book by id for a specific author
        /// </summary>
        /// <param name="authorId">The ID of the author</param>
        /// <param name="bookId">The ID of the book</param>
        /// <returns>An ACtionResult of type "Book"</returns>
        /// <response code="200">Returns the requested book</response>
        [HttpGet("{bookId}")]
        //We use the attributes below to aid documentation of all errors that can be returned when trying to consume an API
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //Including the produces atttribute is important in ensuring the API can return it. You can declare it here at the API level or add it to the specified media types at the controller level
        [Produces("application/vendor.marvin.book+json")]
        //The attribute below ensures that only requests for the specified media types below matches to this API
        [RequestHeaderMatchesMediaType(HeaderNames.Accept, "application/json", "application/vendor.marvin.book+json")]
        public async Task<ActionResult<Book>> GetBook(
            Guid authorId,
            Guid bookId)
        {
            if (! await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }

        /// <summary>
        ///     Get a book by id for a specific author
        /// </summary>
        /// <param name="authorId">The ID of the author</param>
        /// <param name="bookId">The ID of the book</param>
        /// <returns>An ACtionResult of type "BookWithConcatenatedAuthorName"</returns>
        /// <response code="200">Returns the requested book</response>
        //We use the attributes below to aid documentation of all errors that can be returned when trying to consume an API
        [HttpGet("{bookId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //Including the produces atttribute is important in ensuring the API can return it. You can declare it here at the API level or add it to the specified media types at the controller level
        [Produces("application/vendor.marvin.bookwithconcatenatedauthorname+json")]
        //The attribute below ensures that only requests for the specified media types below matches to this API
        [RequestHeaderMatchesMediaType(HeaderNames.Accept, "application/vendor.marvin.bookwithconcatenatedauthorname+json")]
        //The attribute below ensures that the customer doesn't see this API as a standalone API but as a subset of the first API
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<BookWithConcatenatedAuthorName>> GetBookWithConcatenatedAuthorName(
            Guid authorId,
            Guid bookId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookWithConcatenatedAuthorName>(bookFromRepo));
        }

        /// <summary>
        /// Create a book for a specific author
        /// </summary>
        /// <param name="authorId">The id of the book author</param>
        /// <param name="bookForCreation">The book to create</param>
        /// <returns>An ActionResult of type book</returns>
        /// <response code="422">Validation Error</response>
        [HttpPost()]
        //The attribute below helps ensure that regardless of the "Produces" specification declared at controller level, this API can accept inputs of the one specified below
        [Consumes("application/json", "application/vendor.marvin.bookforcreation+json")]
        //The attribute below ensures that only requests for the specified media types below matches to this API
        [RequestHeaderMatchesMediaType(HeaderNames.ContentType, "application/json", "application/vendor.marvin.bookforcreation+json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            [FromBody] BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }

        /// <summary>
        /// Create a book for a specific author
        /// </summary>
        /// <param name="authorId">The id of the book author</param>
        /// <param name="bookForCreationWithAmountOfPages">The book to create</param>
        /// <returns>An ActionResult of type Book</returns>
        /// <response code="422">Validation Error</response>
        [HttpPost()]
        //The attribute below helps ensure that regardless of the "Produces" specification declared at controller level, this API can accept inputs of the one specified below
        [Consumes("application/vendor.marvin.bookforcreationwithamountofpages+json")]
        //The attribute below ensures that only requests for the specified media types below matches to this API
        [RequestHeaderMatchesMediaType(HeaderNames.ContentType, "application/json", "application/vendor.marvin.bookforcreationwithamountofpages+json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
        //The attribute below ensures that the customer doesn't see this API as a standalone API but as a subset of the first API
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Book>> CreateBookWithAmountOfPages(
            Guid authorId,
            [FromBody] BookForCreationWithAmountOfPages bookForCreationWithAmountOfPages)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreationWithAmountOfPages);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }
    }
}
