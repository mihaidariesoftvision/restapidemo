namespace Library.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Library.API.Entities;
    using Library.API.Services;
    using Library.API.Helpers;
    using Library.API.Models;

    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<Author> authorCollection)
        {
            if (authorCollection == null)
            {
                return BadRequest();
            }

            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);

            var authors = authorEntities.ToList();
            foreach (var authorEntity in authors)
            {
                _libraryRepository.AddAuthor(authorEntity);
            }

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating author collection failed!");
            }

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authors);

            return CreatedAtRoute(
                "GetAuthorCollection",
                new
                {
                    ids = string.Join(",", authors.Select(s => s.Id))
                },
                authorCollectionToReturn);
        }

        [HttpGet("{ids}", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorIds = ids.ToList();
            var authorEntities = _libraryRepository.GetAuthors(authorIds);

            if (authorEntities.Count() != authorIds.Count)
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }
    }
}