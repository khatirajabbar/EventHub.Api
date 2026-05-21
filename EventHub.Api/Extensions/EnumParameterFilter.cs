using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventHub.Api.Extensions;

public class EnumParameterFilter : IParameterFilter, IOperationFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo?.PropertyType.IsEnum == true)
        {
            ApplyEnumValues(parameter.Schema, context.PropertyInfo.PropertyType);
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameter in operation.Parameters)
        {
            var propertyType = context.ApiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == parameter.Name)
                ?.Type;

            if (propertyType?.IsEnum == true)
            {
                ApplyEnumValues(parameter.Schema, propertyType);
            }
        }

        if (operation.RequestBody?.Content != null)
        {
            foreach (var content in operation.RequestBody.Content.Values)
            {
                if (content.Schema?.Properties != null)
                {
                    foreach (var property in content.Schema.Properties)
                    {
                        var propertySchema = property.Value;
                        if (propertySchema.Enum?.Count > 0 && propertySchema.Type == "string")
                        {
                            propertySchema.Default = new OpenApiString(propertySchema.Enum.FirstOrDefault()?.ToString() ?? "");
                        }
                    }
                }
            }
        }
    }

    private void ApplyEnumValues(OpenApiSchema schema, Type enumType)
    {
        schema.Enum.Clear();
        schema.Type = "string";
        schema.Format = null;

        var enumNames = Enum.GetNames(enumType);
        schema.Description = "Possible values: " + string.Join(", ", enumNames);

        foreach (var name in enumNames)
        {
            schema.Enum.Add(new OpenApiString(name));
        }
    }
}

