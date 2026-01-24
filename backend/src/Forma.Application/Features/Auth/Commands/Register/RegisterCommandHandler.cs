using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.DTOs;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using Forma.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Commands.Register;

/// <summary>
/// 註冊命令處理器
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtSettings _jwtSettings;

    public RegisterCommandHandler(
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

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 檢查使用者名稱是否已存在
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException("此使用者名稱已被使用");
        }

        // 檢查電子郵件是否已存在
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("此電子郵件已被使用");
        }

        // 建立使用者
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            SystemRole = SystemRole.User,
            Department = request.Department,
            JobTitle = request.JobTitle,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

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
