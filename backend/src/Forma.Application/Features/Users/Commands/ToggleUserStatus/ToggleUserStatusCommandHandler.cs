using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Users.Commands.ToggleUserStatus;

/// <summary>
/// 切換使用者狀態命令處理器
/// </summary>
public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        user.IsActive = request.Activate;

        // 如果停用使用者，撤銷所有 Refresh Token
        if (!request.Activate)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "帳號已被停用";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
