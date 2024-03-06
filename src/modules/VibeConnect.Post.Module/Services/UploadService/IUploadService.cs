using Microsoft.AspNetCore.Http;
using VibeConnect.Post.Module.DTOs.Upload;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.UploadService;

public interface IUploadService
{
    Task<ApiResponse<List<UploadResponseDto>>> UploadFileAsync(string? username, List<IFormFile>? files);
}