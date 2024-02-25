using System.Reflection;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using VibeConnect.Api.Options;
using VibeConnect.Auth.Module.Services;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

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
    
    public static IServiceCollection AddSwaggerGen(
        this IServiceCollection services,
        IConfiguration configuration,
        string schemeName)
    {
        services.Configure<SwaggerDocsConfig>(c => configuration.GetSection(nameof(SwaggerDocsConfig)).Bind(c));

        services.ConfigureOptions<ConfigureSwaggerOptions>();

        var projName = Assembly.GetExecutingAssembly().GetName().Name;

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.AddSecurityDefinition(schemeName, new()
            {
                Description = $@"Enter '[schemeName]' [space] and then your token in the text input below.<br/>
                      Example: '{schemeName} 12345abcd'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = schemeName
            });

            c.AddSecurityRequirement(new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = schemeName
                        },
                        Scheme = "oauth2",
                        Name = schemeName,
                        In = ParameterLocation.Header,
                    },
                    Array.Empty<string>()
                }
            });

            c.DocumentFilter<AdditionalParametersDocumentFilter>();

            c.ResolveConflictingActions(descriptions => descriptions.FirstOrDefault());
            
        });

        return services;
    }
    
    public static IServiceCollection AddBaseRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
       
        return services;
    }
    
    public static IServiceCollection AddAuthModuleServiceCollection(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
       
        return services;
    }
    
    
}