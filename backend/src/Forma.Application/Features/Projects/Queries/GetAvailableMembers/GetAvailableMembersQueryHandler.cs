using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Queries.GetAvailableMembers;

/// <summary>
/// 取得可新增的使用者列表查詢處理器
/// </summary>
public class GetAvailableMembersQueryHandler : IRequestHandler<GetAvailableMembersQuery, List<AvailableMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableMemberDto>> Handle(GetAvailableMembersQuery request, CancellationToken cancellationToken)
    {
        // 驗證專案存在並檢查權限
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 權限檢查：必須是專案管理者或系統管理員
        var currentMember = project.Members.FirstOrDefault(m => m.UserId == request.CurrentUserId);
        var isManager = currentMember != null &&
            (currentMember.Role == ProjectRole.Owner || currentMember.Role == ProjectRole.Manager);

        if (!request.IsSystemAdmin && !isManager)
        {
            throw new UnauthorizedAccessException();
        }

        // 取得已在專案中的使用者 ID
        var existingMemberIds = project.Members.Select(m => m.UserId).ToList();

        // 查詢可新增的使用者
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive && !existingMemberIds.Contains(u.Id));

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                (u.Department != null && u.Department.ToLower().Contains(term)) ||
                (u.JobTitle != null && u.JobTitle.ToLower().Contains(term)));
        }

        var limit = Math.Clamp(request.Limit, 1, 100);

        var users = await query
            .OrderBy(u => u.Username)
            .Take(limit)
            .Select(u => new AvailableMemberDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Department = u.Department,
                JobTitle = u.JobTitle
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
