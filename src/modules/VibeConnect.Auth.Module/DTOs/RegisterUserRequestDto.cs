using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Auth.Module.DTOs;

public class RegisterUserRequestDto
{
    [Required]
    public required string  Username { get; set; } 
    [EmailAddress]
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    [Required]
    public required string FullName { get; set; }
    
    [Required]
    public required DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    [Required]
    public required string Password { get; set; }
}