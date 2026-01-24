using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Commands.UpdateProfile;

/// <summary>
/// 更新個人資料命令處理器
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 更新允許修改的欄位
        user.Department = request.Department;
        user.JobTitle = request.JobTitle;
        user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            SystemRole = user.SystemRole.ToString(),
            Department = user.Department,
            JobTitle = user.JobTitle,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
