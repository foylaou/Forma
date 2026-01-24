using Forma.Application.Common.Interfaces;
using Forma.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// 修改密碼命令處理器
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 驗證目前密碼
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("目前密碼不正確");
        }

        // 更新密碼
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        // 撤銷所有 Refresh Token (強制重新登入)
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "密碼已變更";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
