namespace Library.API.Controllers
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Library.API.Entities;
    using Library.API.Helpers;
    using AutoMapper;
    using Library.API.Models;
    using Library.API.Services;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperService;
        private readonly IResourceUri _resourceUri;
        private readonly IMapper _mapper;

        public AuthorsController(
            ILibraryRepository libraryRepository,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService,
            IResourceUri resourceUri,
            IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _resourceUri = resourceUri;
            _mapper = mapper;
        }

        [HttpGet(Name = ApiNames.GetAuthors)]
        [HttpHead]
        public IActionResult GetAuthors(
            [FromQuery]AuthorsResourceParameters authorsResourceParameters,
            [FromHeader(Name = MediaTypes.AcceptHeader)]
            string mediaType)
        {
            //try
            //{
            //throw new Exception("Boom");

            if(authorsResourceParameters == null)
            {
                authorsResourceParameters = new AuthorsResourceParameters();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var authorsFromRepo = _libraryRepository.GetAuthors(authorsResourceParameters);

            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            if (mediaType == MediaTypes.ApplicationVndMihaiHateoasJsonOutput)
            {
                // consumer wants the HATEOES Metadata included in the response, besides Navigation info (links)
                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsResourceParameters.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                var links = _resourceUri.CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext,
                    authorsFromRepo.HasPrevious);

                var shapedAuthors = authors.ShapeData(authorsResourceParameters.Fields);

                var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
                {
                    var authorAsDictionary = author as IDictionary<string, object>;
                    var authorLinks = _resourceUri.CreateLinksForAuthor((Guid) authorAsDictionary["Id"],
                        authorsResourceParameters.Fields);
                    authorAsDictionary.Add("links", authorLinks);

                    return authorAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorsWithLinks,
                    links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                // consumer doesn't want HATEOS Metadata included in the response

                var previousPageLink = authorsFromRepo.HasPrevious
                    ? _resourceUri.CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
                    : null;

                var nextPageLink = authorsFromRepo.HasNext
                    ? _resourceUri.CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
                    : null;

                var paginationMetadata = new
                {
                    totalCount = authorsFromRepo.TotalCount,
                    pageSize = authorsResourceParameters.PageSize,
                    currentPage = authorsFromRepo.CurrentPage,
                    totalPages = authorsFromRepo.TotalPages,

                    // these are not actual metadata, it's part of Navigation info (HATEOAS)
                    previousPageLink,
                    nextPageLink
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                return Ok(authors.ShapeData(authorsResourceParameters.Fields));
            }

            //}
            //catch (Exception)
            //{
            //    return StatusCode(500, "Something went wrong");
            //}
        }

        [HttpGet("{id}", Name = ApiNames.GetAuthor)]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var links = _resourceUri.CreateLinksForAuthor(id, fields);

            var author = _mapper.Map<AuthorDto>(authorFromRepo);
            var linkedResourceToReturn = author.ShapeData(fields) as IDictionary<string, object>;

            // TODO: remove when serializing as XML
            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn );
        }

        [HttpPost(Name = ApiNames.CreateAuthorWithDateOfDeath)]
        //[Consumes
        //    (
        //        MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonInput,
        //        new[]
        //        {
        //            MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonInput,
        //            MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathXmlInput
        //        }
        //    )
        //]
        [RequestHeaderMatchesMediaType(MediaTypes.ContentTypeHeader,
            new[]
            {
                MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonInput,
                MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathXmlInput
            })
        ]
        //[RequestHeaderMatchesMediaType(MediaTypes.AcceptHeader,
        //    new[]
        //    {
        //        MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonOutput,
        //        MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathXmlOutput
        //    })
        //]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult CreateAuthorWithDateOfDeath([FromBody] AuthorCreateWithDateOfDeathDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            return CreateAuthor(authorEntity);
        }

        [HttpPost(Name = ApiNames.CreateAuthor)]
        [RequestHeaderMatchesMediaType(MediaTypes.ContentTypeHeader, new[]
        {
            MediaTypes.ApplicationXmlContentType,
            MediaTypes.ApplicationJsonContentType,
            MediaTypes.ApplicationVndMihaiAuthorFullJsonInput
        })]
        [Consumes(
            MediaTypes.ApplicationVndMihaiAuthorFullJsonInput,
            new[]
            {
                MediaTypes.ApplicationXmlContentType,
                MediaTypes.ApplicationJsonContentType,
                MediaTypes.ApplicationVndMihaiAuthorFullJsonInput
            })
        ]
        public IActionResult CreateAuthor([FromBody] AuthorCreateDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = _mapper.Map<Author>(author);

            return CreateAuthor(authorEntity);
        }

        private IActionResult CreateAuthor(Author authorEntity)
        {
            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("We couldn't save!");
                //return StatusCode(500, "Something went wrong while saving");
            }

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = _resourceUri.CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute(ApiNames.GetAuthor,
                new
                {
                    id = linkedResourceToReturn["Id"]
                },
                linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            return _libraryRepository.AuthorExists(id) ? new StatusCodeResult(StatusCodes.Status409Conflict) : NotFound();
        }

        [HttpDelete("{id}", Name = ApiNames.DeleteAuthor)]
        public IActionResult DeleteAuthor(Guid id)
        {
            var author = _libraryRepository.GetAuthor(id);
            if (author == null)
            {
                return NotFound("Author not found!");
            }

            _libraryRepository.DeleteAuthor(author);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Author and Books deletion failure!");
            }

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,OPTIONS,POST");

            return Ok();
        }
    }
}