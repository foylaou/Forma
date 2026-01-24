using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Queries.GetProjectMembers;

/// <summary>
/// 取得專案成員列表查詢處理器
/// </summary>
public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        // 檢查專案是否存在
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案成員或系統管理員
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == request.ProjectId &&
                          m.UserId == request.CurrentUserId &&
                          m.RemovedAt == null, cancellationToken);

        if (!isMember && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("您沒有權限查看此專案的成員");
        }

        var members = await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.AddedBy)
            .Where(m => m.ProjectId == request.ProjectId && m.RemovedAt == null)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.AddedAt)
            .Select(m => new ProjectMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                Email = m.User.Email,
                Department = m.User.Department,
                JobTitle = m.User.JobTitle,
                Role = m.Role.ToString(),
                AddedAt = m.AddedAt,
                AddedById = m.AddedById,
                AddedByUsername = m.AddedBy.Username
            })
            .ToListAsync(cancellationToken);

        return members;
    }
}
