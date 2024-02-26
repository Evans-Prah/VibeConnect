using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Auth.Module.DTOs;

public class TokenRequestDto
{
    [Required]
    public required string AccessToken { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
}