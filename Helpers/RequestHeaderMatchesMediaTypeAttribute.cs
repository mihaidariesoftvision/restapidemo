namespace Library.API.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly string[] _mediaTypes;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;
            _mediaTypes = mediaTypes;
        }

        public bool Accept(ActionConstraintContext context)
        {
            if(context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            var mediaTypeMatched = _mediaTypes
                .Select(mediaType => string.Equals(requestHeaders[_requestHeaderToMatch].ToString(), mediaType, StringComparison.OrdinalIgnoreCase))
                .Any(mediaTypeMatches => mediaTypeMatches);

            return mediaTypeMatched;
        }

        public int Order => 0;
    }
}