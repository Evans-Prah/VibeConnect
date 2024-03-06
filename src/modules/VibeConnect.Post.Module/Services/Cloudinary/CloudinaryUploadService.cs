using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VibeConnect.Post.Module.Configurations;
using VibeConnect.Post.Module.DTOs.Cloudinary;
using VibeConnect.Post.Module.Utilities;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Cloudinary;

public class CloudinaryUploadService : ICloudinaryUploadService
{
    private readonly CloudinaryConfig _cloudinaryConfig;
    private readonly ICloudinary _cloudinary;
    private const string FolderPath = "VibeConnect";

    public CloudinaryUploadService(IOptions<CloudinaryConfig> cloudinaryConfig)
    {
        _cloudinaryConfig = cloudinaryConfig.Value;
        
        var account = new Account(
            _cloudinaryConfig.CloudName,
            _cloudinaryConfig.ApiKey,
            _cloudinaryConfig.ApiSecret
        );

        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }
    
    public async Task<ApiResponse<CloudinaryUploadResult>> UploadFileAsync(IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<CloudinaryUploadResult>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest
                };
            }

            var fileType = MediaUploadHelper.GetFileType(file.ContentType, file.FileName);
            
            ImageUploadParams? imageUploadParams = null;
            VideoUploadParams? videoUploadParams = null;
            RawUploadParams? rawUploadParams = null;
            
            switch (fileType.ToLowerInvariant())
            {
                case "image":
                    imageUploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = FolderPath,
                    };
                    break;

                case "video":
                    videoUploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = FolderPath,
                    };
                    break;

                default:
                    rawUploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = FolderPath,
                    };
                    break;
            }

            var uploadResult = await _cloudinary.UploadAsync(imageUploadParams ?? videoUploadParams ?? rawUploadParams);

            return new ApiResponse<CloudinaryUploadResult>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Data = new CloudinaryUploadResult
                {
                    SecureUrl = uploadResult.SecureUrl.ToString()
                }
            };

        }
        catch (Exception e)
        {
            return new ApiResponse<CloudinaryUploadResult>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while uploading image, try again later.",
            };
        }
    }

    
}