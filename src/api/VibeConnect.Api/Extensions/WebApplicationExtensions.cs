using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
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

}