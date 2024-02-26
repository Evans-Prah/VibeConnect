using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VibeConnect.Api.Options;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = Assembly.GetExecutingAssembly().GetName().Name,
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            info.Description +=
                " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
        }

        return info;
    }
}

public class AdditionalParametersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var i in context.SchemaRepository.Schemas
                     .Where(s => s.Value.AdditionalProperties is null))
        {
            i.Value.AdditionalPropertiesAllowed = true;
        }
    }
}