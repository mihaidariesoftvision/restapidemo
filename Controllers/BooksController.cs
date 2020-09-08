namespace Library.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Library.API.Entities;
    using Library.API.Helpers;
    using Library.API.Models;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Library.API.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.JsonPatch.Exceptions;

    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly ILogger<BooksController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IMapper _mapper;

        public BooksController(
            ILibraryRepository libraryRepository,
            ILogger<BooksController> logger,
            IUrlHelper urlHelper,
            IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _logger = logger;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = ApiNames.GetBooksForAuthor)]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            var booksForAuthorFromRepo = _libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = _mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo).ToList();

            foreach (var bookDto in booksForAuthor)
            {
                CreateLinksForBook(bookDto);
            }

            //return Ok(booksForAuthor);

            return Ok(CreateLinksForBooks(booksForAuthor));
        }

        [HttpGet("{id}", Name = ApiNames.GetBookForAuthor)]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound("Book not found!");
            }

            var bookForAuthor = _mapper.Map<BookDto>(bookForAuthorFromRepo);
            var bookWithLinks = CreateLinksForBook(bookForAuthor);
            
            return Ok(bookWithLinks);
        }

        [HttpPost(Name = ApiNames.CreateBookForAuthor)]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookCreateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookCreateDto), "Title and Description must be different");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            var bookEntity = _mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Book creation failure!");
            }

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(ApiNames.GetBookForAuthor,
                new
                {
                    authorId,
                    id = bookEntity.Id
                },
                CreateLinksForBook(bookToReturn));
        }

        [HttpDelete("{id}", Name = ApiNames.DeleteBookForAuthor)]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
            {
                return NotFound("Book not found");
            }

            _libraryRepository.DeleteBook(book);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Book deletion failure!");
            }

            _logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");

            return NoContent();
        }

        [HttpPut("{id}", Name = ApiNames.UpdateBookForAuthor)]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto), "Title and Description must be different");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                var bookToAdd = _mapper.Map<Book>(book);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception("Upserting book failed!");
                }

                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute(
                    ApiNames.GetBookForAuthor,
                    new { authorId, id = bookToReturn.Id },
                    CreateLinksForBook(bookToReturn));
            }

            _mapper.Map(book, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Update failed!");
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = ApiNames.PartiallyUpdateBookForAuthor)]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookUpdateDto> book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound("Author not found!");
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return Upsert(authorId, id, book);
            }

            var bookToPatch = _mapper.Map<BookUpdateDto>(bookForAuthorFromRepo);

            // book.ApplyTo(bookToPatch, ModelState);

            try
            {
                book.ApplyTo(bookToPatch);
            }
            catch(JsonPatchException jpex)
            {
                ModelState.AddModelError(nameof(BookUpdateDto), jpex.Message);
            }

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto), "Title and Description must be different");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Failed to patch!");
            }

            return NoContent();
        }

        private IActionResult Upsert(Guid authorId, Guid id, JsonPatchDocument<BookUpdateDto> book)
        {
            var bookDto = new BookUpdateDto();
            book.ApplyTo(bookDto, ModelState);

            if (bookDto.Description == bookDto.Title)
            {
                ModelState.AddModelError(nameof(BookUpdateDto), "Title and Description must be different");
            }

            TryValidateModel(bookDto);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var bookToAdd = _mapper.Map<Book>(bookDto);
            bookToAdd.Id = id;

            _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Failed to add book");
            }

            var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

            return CreatedAtRoute(ApiNames.GetBookForAuthor, new { authorId, id = bookToReturn.Id }, bookToReturn);
        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            var bookId = new { id = book.Id };

            var urlParams = new
            {
                authorId = book.AuthorId,
                id = bookId
            };

            book.Links.Add(
                new LinkDto(new Uri(_urlHelper.Link(ApiNames.GetBookForAuthor, urlParams)),
                LinksMetadata.Self,
                LinksMetadata.HttpGet));

            book.Links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.DeleteBookForAuthor, urlParams)),
                "delete_book",
                "DELETE"));

            book.Links.Add(
                new LinkDto(
                    new Uri(_urlHelper.Link(ApiNames.UpdateBookForAuthor, urlParams)),
                    "update_book",
                    "UPDATE"));

            book.Links.Add(
                new LinkDto(
                    new Uri(_urlHelper.Link(
                        ApiNames.PartiallyUpdateBookForAuthor,
                        new {
                            authorId = book.AuthorId,
                            id = bookId }
                        )),
                    "partially_update_book",
                    "PATCH"));

            return book;
        }

        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(
            IEnumerable<BookDto> booksForAuthor)
        {
            var booksWrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);

            booksWrapper.Links.Add(
                new LinkDto
                (
                    new Uri(_urlHelper.Link(ApiNames.GetBooksForAuthor, new { })),
                    LinksMetadata.Self,
                    LinksMetadata.HttpGet)
                );

            return booksWrapper;
        }
    }
}