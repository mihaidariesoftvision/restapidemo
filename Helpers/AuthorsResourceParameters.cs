namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string Genre { get; set; }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; } = "Name";

        public string Fields { get; set; }
    }
}