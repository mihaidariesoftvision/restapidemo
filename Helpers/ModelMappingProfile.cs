namespace Library.API.Helpers
{
    using AutoMapper;
    using Library.API.Entities;
    using Library.API.Models;

    public class ModelMappingProfile : Profile
    {
        public ModelMappingProfile()
        {
            CreateMap<Author, AuthorDto>()
                    .ForMember(dest => dest.Name,
                        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                    .ForMember(dest => dest.Age,
                        opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

            CreateMap<Book, BookDto>();
            CreateMap<BookCreateDto, Book>();
            CreateMap<BookUpdateDto, Book>();
            CreateMap<Book, BookUpdateDto>();
            CreateMap<AuthorCreateDto, Author>();
            CreateMap<AuthorCreateWithDateOfDeathDto, Author>();
        }
    }
}
