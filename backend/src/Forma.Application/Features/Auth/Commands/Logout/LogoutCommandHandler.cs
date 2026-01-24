using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Commands.Logout;

/// <summary>
/// 登出命令處理器
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public LogoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.LogoutAll)
        {
            // 登出所有裝置 - 撤銷該使用者所有有效的 Refresh Token
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "使用者登出所有裝置";
            }
        }
        else if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            // 登出單一裝置 - 撤銷指定的 Refresh Token
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.Token == request.RefreshToken &&
                    rt.UserId == request.UserId,
                    cancellationToken);

            if (token != null && token.IsActive)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "使用者登出";
            }
        }
        else
        {
            // 沒有提供 RefreshToken，撤銷該使用者最近使用的 Token
            var latestToken = await _context.RefreshTokens
                .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
                .OrderByDescending(rt => rt.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestToken != null)
            {
                latestToken.RevokedAt = DateTime.UtcNow;
                latestToken.RevokedReason = "使用者登出";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
