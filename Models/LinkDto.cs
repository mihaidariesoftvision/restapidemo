namespace Library.API.Models
{
    using System;

    public class LinkDto
    {
        // ReSharper disable once UnusedMember.Global
        public LinkDto()
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Uri HRef { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Rel { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Method { get; }

        public LinkDto(Uri href, string rel, string method)
        {
            HRef = href;
            Rel = rel;
            Method = method;
        }
    }
}