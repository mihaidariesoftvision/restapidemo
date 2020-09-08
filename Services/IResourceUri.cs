namespace Library.API.Services
{
    using System.Collections.Generic;
    using Library.API.Models;
    using System;
    using Library.API.Helpers;

    public interface IResourceUri
    {
        Uri CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type);
        IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious);
        IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields);
    }
}