namespace Library.API.Formatters
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CustomJsonOutputFormatter : SystemTextJsonOutputFormatter
    {
        public CustomJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
        {
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return base.CanWriteResult(context);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return base.GetSupportedContentTypes(contentType, objectType);
        }

        public override Encoding SelectCharacterEncoding(OutputFormatterWriteContext context)
        {
            return base.SelectCharacterEncoding(context);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            return base.WriteAsync(context);
        }

        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            base.WriteResponseHeaders(context);
        }

        protected override bool CanWriteType(Type type)
        {
            return base.CanWriteType(type);
        }
    }
}
