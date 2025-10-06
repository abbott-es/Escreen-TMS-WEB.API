using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WEB.API.SwaggerFilter
{
    public class RandomGuidSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Guid))
            {
                schema.Example = new OpenApiString(Guid.NewGuid().ToString());
            }
        }
    }
}
