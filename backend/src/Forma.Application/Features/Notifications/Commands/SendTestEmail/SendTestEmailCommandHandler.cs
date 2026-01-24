using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Commands.SendTestEmail;

public class SendTestEmailCommandHandler : IRequestHandler<SendTestEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public SendTestEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SendTestEmailCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("只有系統管理員可以發送測試郵件");
        }

        var targetEmail = request.TargetEmail;
        if (string.IsNullOrEmpty(targetEmail))
        {
            var user = await _context.Users.FindAsync(new object[] { request.CurrentUserId }, cancellationToken);
            targetEmail = user?.Email;
        }

        if (string.IsNullOrEmpty(targetEmail))
        {
            throw new InvalidOperationException("無法取得目標電子郵件地址");
        }

        // TODO: 實作實際的郵件發送邏輯
        // 目前只回傳成功，表示 API 可正常運作
        // 實際實作時需要注入 IEmailService

        return true;
    }
}
