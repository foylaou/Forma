using Forma.Application.Features.Users.DTOs;
using MediatR;

namespace Forma.Application.Features.Users.Queries.GetCurrentUser;

/// <summary>
/// 取得當前使用者查詢
/// </summary>
public class GetCurrentUserQuery : IRequest<UserProfileDto>
{
    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid UserId { get; set; }
}
