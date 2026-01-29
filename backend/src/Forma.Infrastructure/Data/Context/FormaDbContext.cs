using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forma.Infrastructure.Data.Context;

/// <summary>
/// Forma 資料庫上下文
/// </summary>
public class FormaDbContext : DbContext, IApplicationDbContext
{
    public FormaDbContext(DbContextOptions<FormaDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormTemplate> FormTemplates => Set<FormTemplate>();
    public DbSet<FormVersion> FormVersions => Set<FormVersion>();
    public DbSet<FormPermission> FormPermissions => Set<FormPermission>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Export> Exports => Set<Export>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    public DbSet<ActionLog> ActionLogs => Set<ActionLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<FidoCredential> FidoCredentials => Set<FidoCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 套用所有實體配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FormaDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 自動設定審計欄位
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
