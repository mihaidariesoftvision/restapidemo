namespace Library.API.Controllers
{
    using Library.API.Helpers;
    using Library.API.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Collections.Generic;

    [Route("api")]
    public class RootController : Controller
    {
        private readonly IUrlHelper _urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = ApiNames.GetRoot)]
        public IActionResult GetRoot([FromHeader(Name = MediaTypes.AcceptHeader)]
            string mediaType)
        {
            if (mediaType != MediaTypes.ApplicationVndMihaiHateoasJsonOutput) return NoContent();
            
            return Ok(new List<LinkDto>
            {
                new LinkDto(new Uri(_urlHelper.Link(
                        ApiNames.GetRoot,
                        new { })),
                    LinksMetadata.Self,
                    LinksMetadata.HttpGet),

                new LinkDto(new Uri(_urlHelper.Link(
                        ApiNames.GetAuthors,
                        new { })),
                    LinksMetadata.Authors,
                    LinksMetadata.HttpGet),

                new LinkDto(new Uri(_urlHelper.Link(
                        ApiNames.CreateAuthor,
                        new { })),
                    LinksMetadata.CreateAuthor,
                    LinksMetadata.HttpPost)
            });
        }
    }
}