using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventHub.Api.Extensions;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            schema.Type = "string";
            schema.Format = null;
            
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);
            
            schema.Description = "Available options: " + string.Join(", ", enumNames);
            
            // Add each enum value as an option
            foreach (var name in enumNames)
            {
                schema.Enum.Add(new OpenApiString(name));
            }
        }
    }
}

