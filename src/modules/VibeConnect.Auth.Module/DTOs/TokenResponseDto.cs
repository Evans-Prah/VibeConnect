namespace VibeConnect.Auth.Module.DTOs;

public class TokenResponseDto
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}