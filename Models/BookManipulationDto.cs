namespace Library.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public abstract class BookManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "No more than 100 chars")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "No more than 500 chars")]
        public virtual string Description { get; set; }
    }
}
