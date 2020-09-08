namespace Library.API.Models
{
    using System;
    using System.Collections.Generic;

    public class AuthorCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Genre { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }

        public IEnumerable<BookCreateDto> Books { get; set; } = new List<BookCreateDto>();
    }
}