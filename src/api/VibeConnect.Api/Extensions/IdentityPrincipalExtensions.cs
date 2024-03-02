using System.Security.Claims;
using VibeConnect.Api.Models;

namespace VibeConnect.Api.Extensions;

public static class IdentityPrincipalExtensions
{
    private static string? GetUsername(this ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst(ClaimTypes.Name);
        return claim?.Value;
    }

    public static AuthData? GetCurrentUserAccount(this ClaimsPrincipal claimsPrincipal)
    {
        var claimsIdentity = claimsPrincipal.Identities.FirstOrDefault(x => x.AuthenticationType == "VibeConnect");
        var auth = claimsIdentity?.FindFirst(ClaimTypes.Authentication);

        if (auth == null) return new AuthData { Username = claimsPrincipal.GetUsername() };

        var user = new AuthData { Username = claimsPrincipal.GetUsername() };
        return user;
    }
}

