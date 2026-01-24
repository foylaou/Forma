using System.Security.Claims;
using Forma.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Forma.Infrastructure.Services;

/// <summary>
/// 當前使用者服務實作
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("uid");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    /// <inheritdoc />
    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <inheritdoc />
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public IEnumerable<string> Roles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);
            return roles?.Select(r => r.Value) ?? Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public string? SystemRole => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    /// <inheritdoc />
    public bool IsSystemAdmin => SystemRole == "SystemAdmin";
}
