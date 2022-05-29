using System.Security.Claims;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _contextAccessor;
    public CurrentUserAccessor(
        IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
    public string GetUsername()
    {
        var usernameClaim = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
        return usernameClaim ?? string.Empty;
    }
}