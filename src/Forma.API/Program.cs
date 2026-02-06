using Forma.API.Extensions;
using Forma.API.Middleware;
using Forma.Application;
using Forma.Application.Features.SystemSettings.DTOs;
using Forma.Application.Services;
using Forma.Infrastructure;
using Forma.Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// JWT Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// CORS - 使用動態來源
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var configOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var origins = configOrigins ?? ["http://localhost:3000"];

        policy.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 種子資料初始化
await DataSeeder.SeedAsync(app.Services);

// 動態設定 Forwarded Headers (根據 CORS 設定)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var settingService = scope.ServiceProvider.GetRequiredService<ISystemSettingService>();
        var corsSettings = await settingService.GetSettingAsync<CorsSettingsDto>("Cors");

        if (corsSettings.TrustProxyHeaders)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }
    catch
    {
        // 使用預設值
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Forma API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var urls = builder.Configuration["ASPNETCORE_URLS"]
         ?? builder.Configuration["applicationUrl"]
         ?? "http://localhost:5053";
app.Run(urls);
