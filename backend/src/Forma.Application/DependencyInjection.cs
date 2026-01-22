using System.Reflection;
using FluentValidation;
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

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }
}
