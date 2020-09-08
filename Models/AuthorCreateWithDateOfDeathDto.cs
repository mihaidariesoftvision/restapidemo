namespace Library.API.Models
{
    using System;

    public class AuthorCreateWithDateOfDeathDto //: AuthorCreateDto
    {
        // TODO: if any API isn't working anymore, remove the inheritance
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Genre { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public DateTimeOffset? DateOfDeath { get; set; }
    }
}