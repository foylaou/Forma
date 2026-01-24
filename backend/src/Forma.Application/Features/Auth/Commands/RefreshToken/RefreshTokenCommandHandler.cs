using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.DTOs;
using Forma.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// 刷新 Token 命令處理器
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IJwtSettings _jwtSettings;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        IJwtSettings jwtSettings)
    {
        _context = context;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 尋找 Refresh Token
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null)
        {
            throw new UnauthorizedAccessException("無效的 Refresh Token");
        }

        // 檢查 Token 是否有效
        if (!existingToken.IsActive)
        {
            // 如果 Token 已被撤銷，可能是被盜用，撤銷該使用者所有 Token
            if (existingToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(existingToken.UserId, "嘗試使用已撤銷的 Token", cancellationToken);
            }

            throw new UnauthorizedAccessException("Refresh Token 已失效");
        }

        var user = existingToken.User;

        // 檢查使用者是否啟用
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("此帳號已被停用");
        }

        // 撤銷舊的 Refresh Token
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedReason = "Token 輪換";

        // 產生新的 Tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // 標記舊 Token 被新 Token 替換
        existingToken.ReplacedByToken = newRefreshToken;

        // 儲存新的 Refresh Token
        var refreshTokenEntity = new Domain.Entities.RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = request.DeviceInfo ?? existingToken.DeviceInfo,
            IpAddress = request.IpAddress ?? existingToken.IpAddress
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
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

    private async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
