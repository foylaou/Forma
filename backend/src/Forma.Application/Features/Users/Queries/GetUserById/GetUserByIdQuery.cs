using Forma.Application.Features.Users.DTOs;
using MediatR;

namespace Forma.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// 取得指定使用者查詢
/// </summary>
public class GetUserByIdQuery : IRequest<UserProfileDto>
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public Guid UserId { get; set; }
}
