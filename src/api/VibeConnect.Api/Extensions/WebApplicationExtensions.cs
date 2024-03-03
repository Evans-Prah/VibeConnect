using System.Net;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using VibeConnect.Api.Configurations;
using VibeConnect.Storage;

namespace VibeConnect.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var services = scope.ServiceProvider;
        try
        {
            var storageContext = services.GetRequiredService<ApplicationDbContext>();
            var pendingMigrations = await storageContext.Database.GetPendingMigrationsAsync();
            var count = pendingMigrations.Count();
            if (count > 0)
            {
                logger.LogInformation("Found {count} pending migrations to apply.", count);
                await storageContext.Database.MigrateAsync();
                logger.LogInformation("Finished applying pending migrations");
            }
            else
            {
                logger.LogInformation("No pending migrations found!)");
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while performing migration.");
            throw;
        }
    }
    
    public static void UseExceptionHandler(
        this WebApplication app,
        bool returnStackTrace = false)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        app.UseExceptionHandler(appError =>
        {
            appError.Run(context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "Unhadled Exception Occured");
                }

                return Task.CompletedTask;
            });
        });
    }

    public static void UseSwaggerDoc(this WebApplication app)
    {
        var apiDocsConfig = app.Services.GetRequiredService<IOptions<SwaggerDocsConfig>>().Value;

        var apiVersionDescription = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        if (apiDocsConfig.ShowSwaggerUi)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                var projName = Assembly.GetExecutingAssembly().GetName().Name;
                foreach (var description in apiVersionDescription.ApiVersionDescriptions.Reverse())
                {
                    c.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"{projName} - {description.GroupName}");
                }

                var submitMethods = Array.Empty<SubmitMethod>();

                if (apiDocsConfig.EnableSwaggerTryIt)
                {
                    submitMethods = new SubmitMethod[]
                    {
                        SubmitMethod.Post,
                        SubmitMethod.Get,
                        SubmitMethod.Put,
                        SubmitMethod.Patch,
                        SubmitMethod.Delete,
                    };
                }

                c.SupportedSubmitMethods(submitMethods);
            });
        }

        if (apiDocsConfig.ShowRedocUi)
        {
            foreach (var description in apiVersionDescription.ApiVersionDescriptions.Reverse())
            {
                app.UseReDoc(options =>
                {
                    options.DocumentTitle = Assembly.GetExecutingAssembly().GetName().Name;
                    options.RoutePrefix = $"api-docs-{description.GroupName}";
                    options.SpecUrl = $"/swagger/{description.GroupName}/swagger.json";
                });
            }
        }
    }

}