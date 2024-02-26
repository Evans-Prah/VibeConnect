using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Auth.Module.DTOs;

public class LoginRequestDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? Username { get; set; }

    public required string Password { get; set; }
}