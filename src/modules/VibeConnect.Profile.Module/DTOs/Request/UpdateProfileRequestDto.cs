using System.ComponentModel.DataAnnotations;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Profile.Module.DTOs.Request;

public class UpdateProfileRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
   
    public List<LanguagePreference>? LanguagePreferences { get; set; }
    public List<ExternalLinkDto>? ExternalLinks { get; set; }
}

public class ExternalLinkDto
{
    public string? Name { get; set; }
    [Url]
    public string? Url { get; set; }
}