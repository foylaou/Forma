using System.Reflection;
using FluentValidation;
using Forma.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Forma.Application;

/// <summary>
/// Application 層依賴注入配置
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 註冊 Application 層服務
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // AutoMapper
        services.AddAutoMapper(assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFileManagementService, FileManagementService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
        services.AddScoped<IFido2Service, Fido2Service>();

        return services;
    }
}
