using Forma.Application.Common.Authorization;
using Forma.Domain.Enums;

namespace Forma.API.Extensions;

/// <summary>
/// 授權配置擴充方法
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// 新增授權策略
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            // RequireUser: 任何已認證的使用者
            .AddPolicy(Policies.RequireUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(
                    SystemRole.User.ToString(),
                    SystemRole.Auditor.ToString(),
                    SystemRole.SystemAdmin.ToString());
            })
            // RequireAuditor: 審核員或系統管理員
            .AddPolicy(Policies.RequireAuditor, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(
                    SystemRole.Auditor.ToString(),
                    SystemRole.SystemAdmin.ToString());
            })
            // RequireSystemAdmin: 僅系統管理員
            .AddPolicy(Policies.RequireSystemAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(SystemRole.SystemAdmin.ToString());
            });

        return services;
    }
}
