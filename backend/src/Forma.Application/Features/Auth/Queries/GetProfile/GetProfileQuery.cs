using Forma.Application.Features.Auth.DTOs;
using MediatR;

namespace Forma.Application.Features.Auth.Queries.GetProfile;

/// <summary>
/// 取得個人資料查詢
/// </summary>
public class GetProfileQuery : IRequest<ProfileDto>
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public Guid UserId { get; set; }
}
