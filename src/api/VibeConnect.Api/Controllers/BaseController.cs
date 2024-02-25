using Microsoft.AspNetCore.Mvc;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    public IActionResult ToActionResult<T>(ApiResponse<T> apiResponse)
    {
        return StatusCode(apiResponse.ResponseCode, apiResponse);
    }
}