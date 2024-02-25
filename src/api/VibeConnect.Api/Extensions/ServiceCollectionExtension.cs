using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Extensions;

public static class ServiceCollectionExtension
{
     public static IServiceCollection AddApiVersioning(this IServiceCollection services, int version)
    {
        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(version, 0);
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.ReportApiVersions = true;
            opt.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var validationErrors = context.ModelState
                        .Where(modelError => modelError.Value?.Errors?.Count > 0)
                        .Select(modelError => new ErrorResponse(
                            Field: modelError.Key,
                            ErrorMessage: modelError.Value?.Errors?.FirstOrDefault()?.ErrorMessage ?? "Invalid Request"));

                    return new BadRequestObjectResult(new ApiResponse<object>(
                        message: "Validation Errors",
                        responseCode: 400,
                        errors: validationErrors));
                };
            });

        return services;
    }    
}