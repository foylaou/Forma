using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Users.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// 管理員更新使用者命令處理器
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserProfileDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 檢查使用者名稱是否已被其他人使用
        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == request.Username && u.Id != request.UserId, cancellationToken);

            if (usernameExists)
            {
                throw new InvalidOperationException("此使用者名稱已被使用");
            }

            user.Username = request.Username;
        }

        // 檢查電子郵件是否已被其他人使用
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException("此電子郵件已被使用");
            }

            user.Email = request.Email;
        }

        // 更新系統角色
        if (!string.IsNullOrEmpty(request.SystemRole) &&
            Enum.TryParse<SystemRole>(request.SystemRole, true, out var role))
        {
            user.SystemRole = role;
        }

        // 更新其他欄位
        if (request.Department != null)
            user.Department = request.Department;

        if (request.JobTitle != null)
            user.JobTitle = request.JobTitle;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            SystemRole = user.SystemRole.ToString(),
            Department = user.Department,
            JobTitle = user.JobTitle,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
