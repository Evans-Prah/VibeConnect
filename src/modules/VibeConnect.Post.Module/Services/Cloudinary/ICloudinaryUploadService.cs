using Microsoft.AspNetCore.Http;
using VibeConnect.Post.Module.DTOs.Cloudinary;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Cloudinary;

public interface ICloudinaryUploadService
{
    Task<ApiResponse<CloudinaryUploadResult>> UploadFileAsync(IFormFile? file);
}