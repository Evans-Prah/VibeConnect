using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using VibeConnect.Api.Configurations;
using VibeConnect.Auth.Module.Options;
using VibeConnect.Auth.Module.Services;
using VibeConnect.Post.Module.Services.Cloudinary;
using VibeConnect.Post.Module.Services.Comment;
using VibeConnect.Post.Module.Services.Post;
using VibeConnect.Post.Module.Services.PostLikes;
using VibeConnect.Post.Module.Services.UploadService;
using VibeConnect.Profile.Module.Services;
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

    private static readonly char[] Separator = { ' ' };

    public static IServiceCollection AddBearerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Action<JwtConfig> bearerTokenConfigAction = bearerTokenConfig =>
            configuration.GetSection(nameof(JwtConfig)).Bind(bearerTokenConfig);
        var bearerConfig = new JwtConfig();
        bearerTokenConfigAction.Invoke(bearerConfig);
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async (ctx) =>
                    {
                        if (ctx.SecurityToken.ValidTo < DateTime.UtcNow)
                        {
                            ctx.Fail("Token has expired");
                            return;
                        }
                        
                        var bearerAuth = ctx.HttpContext.Request.Headers.Authorization[0]?.Split(Separator)[1]!;
                        var username = ctx.Principal?.FindFirst(c => c.Type == ClaimTypes.Name)?.Value!;
                        
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.Authentication, bearerAuth)
                        };
                        
                        var appIdentity = new ClaimsIdentity(claims, "VibeConnect");

                        ctx.Principal?.AddIdentity(appIdentity);

                    }
                };
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = bearerConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerConfig.SigningKey)),
                    ValidAudience = bearerConfig.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
    
    public static IServiceCollection AddBaseRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
        services.AddScoped<IBaseRepository<Storage.Entities.Post>, BaseRepository<Storage.Entities.Post>>();
        services.AddScoped<IBaseRepository<PostLike>, BaseRepository<PostLike>>();
        services.AddScoped<IBaseRepository<Comment>, BaseRepository<Comment>>();
       
        return services;
    }
    
    public static IServiceCollection AddAuthModuleServiceCollection(this IServiceCollection services)
    {
        services.AddTransient<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
       
        return services;
    }
    
    public static IServiceCollection AddProfileModuleServiceCollection(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
       
        return services;
    }
    
    public static IServiceCollection AddPostModuleServiceCollection(this IServiceCollection services)
    {
        services.AddScoped<ICloudinaryUploadService, CloudinaryUploadService>();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostLikeService, PostLikeService>();
        services.AddScoped<ICommentService, CommentService>();
       
        return services;
    }
}