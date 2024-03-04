using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Post.Module.Models.Upload;
using VibeConnect.Post.Module.Services.UploadService;
using VibeConnect.Profile.Module.DTOs.Request;
using VibeConnect.Profile.Module.DTOs.Response;
using VibeConnect.Profile.Module.Services;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.PostModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class UploadController(IUploadService uploadService) : BaseController
{
    
    /// <summary>
    /// User Upload media
    /// </summary>
    /// <param name="files">upload user media payload</param>
    /// <returns>Uploaded media result</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<UploadResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<List<UploadResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<List<UploadResponseDto>>))]
    [SwaggerOperation(nameof(UploadMedia), OperationId = nameof(UploadMedia))]
    public async Task<IActionResult> UploadMedia([FromForm] List<IFormFile> files)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await uploadService.UploadFileAsync(currentUser?.Username, files);
        return ToActionResult(response);
    }
    
}