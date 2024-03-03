using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Profile.Module.DTOs.Request;

public class UpdateLocationRequestDto
{
    [Required]
    public required string City { get; set; }
    [Required]
    public required string Country { get; set; }
}