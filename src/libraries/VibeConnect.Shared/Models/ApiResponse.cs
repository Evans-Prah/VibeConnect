namespace VibeConnect.Shared.Models;

public class ApiResponse<T>
{
    public ApiResponse(string message, int responseCode, IEnumerable<ErrorResponse>? errors = default)
    {
        Message = message;
        ResponseCode = responseCode;
        Errors = errors;
    }

    public ApiResponse()
    {
    }

    

    public string? Message { get; set; }
    public int ResponseCode { get; set; }
    public T? Data { get; set; }
    
    public IEnumerable<ErrorResponse>? Errors { get; }
}

public sealed record ErrorResponse(string Field, string ErrorMessage);
