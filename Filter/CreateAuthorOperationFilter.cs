namespace Library.API.Filter
{
    using Library.API.Helpers;
    using Library.API.Models;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class CreateAuthorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation != null && context != null)
            {
                if (operation.OperationId != "CreateAuthor")
                {
                    return;
                }

                operation.RequestBody.Content.Add(
                    MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathJsonInput,
                    new OpenApiMediaType()
                    {
                        Schema = context.SchemaRepository.GetOrAdd(
                        typeof(AuthorCreateWithDateOfDeathDto),
                        nameof(AuthorCreateWithDateOfDeathDto),
                        () => new OpenApiSchema())
                    });

                operation.RequestBody.Content.Add(
                    MediaTypes.ApplicationVndMihaiAuthorWithDateOfDeathXmlInput,
                    new OpenApiMediaType()
                    {
                        Schema = context.SchemaRepository.GetOrAdd(
                        typeof(AuthorCreateWithDateOfDeathDto),
                        nameof(AuthorCreateWithDateOfDeathDto),
                        () => new OpenApiSchema())
                    });                
            }
        }
    }
}
