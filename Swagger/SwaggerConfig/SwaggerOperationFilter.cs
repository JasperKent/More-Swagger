using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swagger.SwaggerConfig
{
    internal class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Tags = new[] { new OpenApiTag { Name = context.ApiDescription.HttpMethod } };
        }
    }
}
