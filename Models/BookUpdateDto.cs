namespace Library.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public class BookUpdateDto : BookManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a Description")]
        public override string Description
        {
            get => base.Description;
            set => base.Description = value;
        }
    }
}
