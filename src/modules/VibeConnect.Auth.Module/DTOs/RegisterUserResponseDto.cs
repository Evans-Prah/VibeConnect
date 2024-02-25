namespace VibeConnect.Auth.Module.DTOs;

public class RegisterUserResponseDto
{
    public string? Id { get; set; }
    public string? Username { get; set; } 
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
}