using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Auth.Queries.GetProfile;

/// <summary>
/// 取得個人資料查詢處理器
/// </summary>
public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
{
    private readonly IApplicationDbContext _context;

    public GetProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

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
