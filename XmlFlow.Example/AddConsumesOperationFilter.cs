using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

public class AddConsumesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attribute = context.MethodInfo.GetCustomAttributes<ConsumesAttribute>().FirstOrDefault();
        if (attribute is not null)
        {
            operation.RequestBody = new()
                                    {
                                            Content = attribute.ContentTypes
                                                               .ToDictionary(
                                                                             ct => ct,
                                                                             ct => new OpenApiMediaType
                                                                                 {
                                                                                         Schema = new() { Type = "string" },
                                                                                 }),
                                    };
        }
    }
}