using Library.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Helpers;
using Library.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly LibraryContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public LibraryRepository(LibraryContext context, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _propertyMappingService = propertyMappingService;
        }

        public void AddAuthor(Author author)
        {
            if(author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }    

            author.Id = Guid.NewGuid();
            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }

            _context.Authors.Add(author);
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId);
            if (author == null || book == null)
            {
                return;
            }

            book.AuthorId = authorId;

            // if there isn't an id filled out (ie: we're not upserting),
            // we should generate one
            if (book.Id == Guid.Empty)
            {
                book.Id = Guid.NewGuid();
            }

            author.Books.Add(book);
        }

        public bool AuthorExists(Guid authorId)
        {
            return _context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            _context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return _context
                .Authors
                .Include(author => author.Books)
                .FirstOrDefault(a => a.Id == authorId);
        }

        public PagedListCollection<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (authorsResourceParameters == null)
            {
                authorsResourceParameters = new AuthorsResourceParameters();
            }

            var collectionBeforePaging = _context.Authors
                .ApplySort(authorsResourceParameters.OrderBy, _propertyMappingService.GetPropertyMappingValue<AuthorDto, Author>());

            if (!string.IsNullOrEmpty(authorsResourceParameters.Genre))
            {
                collectionBeforePaging = collectionBeforePaging.Where(c => c.Genre == authorsResourceParameters.Genre.Trim());
            }

            if (!string.IsNullOrEmpty(authorsResourceParameters.SearchQuery))
            {
                var searchQueryForWhereClause = authorsResourceParameters.SearchQuery.Trim().ToUpperInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(c => EF.Functions.Like(c.Genre, "%" + searchQueryForWhereClause + "%")
                    || EF.Functions.Like(c.FirstName, "%" + searchQueryForWhereClause + "%")
                    || EF.Functions.Like(c.LastName, "%" + searchQueryForWhereClause + "%"));
            }

            return PageCollection(authorsResourceParameters, collectionBeforePaging);
        }

        private static PagedListCollection<Author> PageCollection(
            AuthorsResourceParameters authorsResourceParameters,
            IQueryable<Author> collectionBeforePaging)
        {
            return new PagedListCollection<Author>().Create(
                                collectionBeforePaging,
                                authorsResourceParameters.PageNumber,
                                authorsResourceParameters.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return _context.Books.FirstOrDefault(b => b.AuthorId == authorId && b.Id == bookId);
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return _context.Books.Where(b => b.AuthorId == authorId).OrderBy(b => b.Title).ToList();
        }

        public void UpdateBookForAuthor(Book book)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}