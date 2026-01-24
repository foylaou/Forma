using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.DTOs;
using Forma.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Commands.Login;

/// <summary>
/// 登入命令處理器
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtSettings _jwtSettings;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IJwtSettings jwtSettings)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 尋找使用者 (支援電子郵件或使用者名稱)
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == request.EmailOrUsername ||
                u.Username == request.EmailOrUsername,
                cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("無效的電子郵件/使用者名稱或密碼");
        }

        // 檢查帳號是否啟用
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("此帳號已被停用");
        }

        // 驗證密碼
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("無效的電子郵件/使用者名稱或密碼");
        }

        // 產生 Tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // 儲存 Refresh Token
        var refreshTokenEntity = new Domain.Entities.RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        // 更新最後登入時間
        user.LastLoginAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                SystemRole = user.SystemRole.ToString(),
                Department = user.Department,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive
            }
        };
    }
}
