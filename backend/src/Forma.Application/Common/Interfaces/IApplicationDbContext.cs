using Forma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Common.Interfaces;

/// <summary>
/// 應用程式資料庫上下文介面
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// 使用者
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Refresh Token
    /// </summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// 組織
    /// </summary>
    DbSet<Organization> Organizations { get; }

    /// <summary>
    /// 專案
    /// </summary>
    DbSet<Project> Projects { get; }

    /// <summary>
    /// 專案成員
    /// </summary>
    DbSet<ProjectMember> ProjectMembers { get; }

    /// <summary>
    /// 表單
    /// </summary>
    DbSet<Form> Forms { get; }

    /// <summary>
    /// 表單範本
    /// </summary>
    DbSet<FormTemplate> FormTemplates { get; }

    /// <summary>
    /// 表單版本
    /// </summary>
    DbSet<FormVersion> FormVersions { get; }

    /// <summary>
    /// 表單提交
    /// </summary>
    DbSet<FormSubmission> FormSubmissions { get; }

    /// <summary>
    /// 表單權限
    /// </summary>
    DbSet<FormPermission> FormPermissions { get; }

    /// <summary>
    /// 報告
    /// </summary>
    DbSet<Report> Reports { get; }

    /// <summary>
    /// 匯出任務
    /// </summary>
    DbSet<Export> Exports { get; }

    /// <summary>
    /// 通知
    /// </summary>
    DbSet<Notification> Notifications { get; }

    /// <summary>
    /// 通知偏好設定
    /// </summary>
    DbSet<NotificationPreference> NotificationPreferences { get; }

    /// <summary>
    /// 儲存變更
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
