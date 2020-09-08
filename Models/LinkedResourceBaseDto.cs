namespace Library.API.Models
{
    using System.Collections.Generic;

    public abstract class LinkedResourceBaseDto
    {
        public IList<LinkDto> Links { get; } = new List<LinkDto>();
    }
}
