namespace VibeConnect.Shared;

public interface ILoggerAdapter<T>
{
    void LogError(Exception ex, string message, params object?[] args);
    void LogError(string message, params object?[] args);
    void LogDebug(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogInformation(string message, params object?[] args);
}