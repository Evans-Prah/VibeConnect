using Microsoft.Extensions.Logging;

namespace VibeConnect.Shared;

public class LoggerAdapter<T>(ILogger<T> logger) : ILoggerAdapter<T>
{
    public void LogError(Exception ex, string message, params object?[] args)
    {
        logger.LogError(ex, message, args);
    }

    public void LogError(string message, params object?[] args)
    {
        logger.LogError(message, args);
    }

    public void LogDebug(string message, params object?[] args)
    {
        logger.LogDebug(message, args);
    }

    public void LogWarning(string message, params object?[] args)
    {
        logger.LogWarning(message, args);
    }

    public void LogInformation(string message, params object?[] args)
    {
        logger.LogInformation(message, args);
    }
}