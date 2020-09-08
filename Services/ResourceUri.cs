namespace Library.API.Services
{
    using System;
    using System.Collections.Generic;
    using Library.API.Models;
    using Library.API.Helpers;
    using Microsoft.AspNetCore.Mvc;

    public class ResourceUri : IResourceUri
    {
        private readonly IUrlHelper _urlHelper;

        public ResourceUri(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.GetAuthor,
                        new
                        {
                            id
                        })),
                    LinksMetadata.Self,
                    LinksMetadata.HttpGet));
            }
            else
            {
                links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.GetAuthor,
                        new
                        {
                            id,
                            fields
                        })),
                    LinksMetadata.Self,
                    LinksMetadata.HttpGet));
            }

            links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.DeleteAuthor,
                    new
                    {
                        id
                    })),
                "delete_author",
                "DELETE"));

            links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.CreateBookForAuthor,
                    new
                    {
                        authorId = id
                    })),
                "create_book_for_author",
                "POST"));

            links.Add(new LinkDto(new Uri(_urlHelper.Link(ApiNames.GetBooksForAuthor,
                    new
                    {
                        authorId = id
                    })),
                "get_books_for_author",
                LinksMetadata.HttpGet));

            return links;
        }

        public IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current), LinksMetadata.Self,
                    LinksMetadata.HttpGet)
            };

            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage",
                    LinksMetadata.HttpGet));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previousPage",
                    LinksMetadata.HttpGet));
            }

            return links;
        }

        public Uri CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            if (authorsResourceParameters == null) return null;
            return new Uri(type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link(ApiNames.GetAuthors,
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    searchQuery = authorsResourceParameters.SearchQuery,
                    genre = authorsResourceParameters.Genre,
                    pageNumber = authorsResourceParameters.PageNumber - 1,
                    pageSize = authorsResourceParameters.PageSize
                }),
                ResourceUriType.NextPage => _urlHelper.Link(ApiNames.GetAuthors,
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    searchQuery = authorsResourceParameters.SearchQuery,
                    genre = authorsResourceParameters.Genre,
                    pageNumber = authorsResourceParameters.PageNumber + 1,
                    pageSize = authorsResourceParameters.PageSize
                }),
                _ => _urlHelper.Link(ApiNames.GetAuthors,
                new
                {
                    fields = authorsResourceParameters.Fields,
                    orderBy = authorsResourceParameters.OrderBy,
                    searchQuery = authorsResourceParameters.SearchQuery,
                    genre = authorsResourceParameters.Genre,
                    pageNumber = authorsResourceParameters.PageNumber,
                    pageSize = authorsResourceParameters.PageSize
                }),
            });
        }
    }
}