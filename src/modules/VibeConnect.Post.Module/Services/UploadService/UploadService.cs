using System.Net;
using Microsoft.AspNetCore.Http;
using VibeConnect.Post.Module.DTOs.Upload;
using VibeConnect.Post.Module.Services.Cloudinary;
using VibeConnect.Post.Module.Utilities;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.UploadService;

public class UploadService(ICloudinaryUploadService cloudinaryUploadService, ILoggerAdapter<UploadService> logger) : IUploadService
{
    private const int MaximumFileCount = 5;

    public async Task<ApiResponse<List<UploadResponseDto>>> UploadFileAsync(string? username, List<IFormFile>? files)
    {
        try
        {
            logger.LogInformation(
                "Upload files for user {user} -> Service: {service} -> Method: {method}.",
                username,
                nameof(UploadService),
                nameof(UploadFileAsync)
            );

            if (files == null || files.Count == 0)
            {
                return new ApiResponse<List<UploadResponseDto>>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "No files were provided for upload."
                };
            }

            if (files.Count > MaximumFileCount)
            {
                return new ApiResponse<List<UploadResponseDto>>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = $"You can upload a maximum of {MaximumFileCount} files."
                };
            }

            const long maxFileSize = 100 * 1024 * 1024;

            var fileUrls = new List<string>();
            var uploadResponseDtoList = new List<UploadResponseDto>();

            foreach (var file in files)
            {
                switch (file.Length)
                {
                    case 0:
                        return new ApiResponse<List<UploadResponseDto>>
                        {
                            ResponseCode = (int)HttpStatusCode.BadRequest,
                            Message = "One or more files are empty."
                        };
                    case > maxFileSize:
                        return new ApiResponse<List<UploadResponseDto>>
                        {
                            ResponseCode = (int)HttpStatusCode.BadRequest,
                            Message = $"File '{file.FileName}' exceeds the maximum allowed size."
                        };
                }

                var fileUrl = await cloudinaryUploadService.UploadFileAsync(file);

                if (fileUrl.ResponseCode != (int)HttpStatusCode.OK)
                {
                    return new ApiResponse<List<UploadResponseDto>>
                    {
                        ResponseCode = fileUrl.ResponseCode,
                        Message = fileUrl.Message
                    };
                }

                if (fileUrl.Data?.SecureUrl != null)
                {
                    fileUrls.Add(fileUrl.Data.SecureUrl);
                }
            }

            uploadResponseDtoList.AddRange(fileUrls.Select(url => new UploadResponseDto
            {
                SecureUrl = url,
                FileType = MediaUploadHelper.GetFileType(files[0].ContentType, files[0].FileName)
            }));

            return new ApiResponse<List<UploadResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Upload successful",
                Data = uploadResponseDtoList
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Upload files for user {user} -> Service: {service} -> Method: {method}.", username,
                nameof(UploadService), nameof(UploadFileAsync));
            
            return new ApiResponse<List<UploadResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while uploading image, try again later."
            };
        }
    }
    

}