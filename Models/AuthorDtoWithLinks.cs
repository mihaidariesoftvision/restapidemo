//namespace Library.API.Models
//{
//    using System.Collections.Generic;

//    public class AuthorDtoWithLinks : AuthorDto
//    {
//        public AuthorDtoWithLinks(AuthorDto authorDto, IEnumerable<LinkDto> links) 
//            : base(authorDto.Id, authorDto.Name, authorDto.Age, authorDto.Genre)
//        {
//            Links = links;
//        }

//        public IEnumerable<LinkDto> Links { get; }
//    }
//}