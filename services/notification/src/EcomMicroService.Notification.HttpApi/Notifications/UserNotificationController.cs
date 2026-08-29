using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace EcomMicroService.Notification;

[RemoteService(Name = "Notification")]
[Area("notification")]
[Route("api/notification/user-notification")]
[Authorize]
public class UserNotificationController : NotificationController
{
    private readonly IUserNotificationAppService _app;
    public UserNotificationController(IUserNotificationAppService app) => _app = app;

    [HttpGet]
    public Task<PagedResultDto<UserNotificationDto>> GetListAsync([FromQuery] GetNotificationsInput input) => _app.GetListAsync(input);

    [HttpGet("unread-count")]
    public Task<NotificationCountDto> GetUnreadCountAsync() => _app.GetUnreadCountAsync();

    [HttpPost("{id}/read")]
    public Task MarkAsReadAsync(Guid id) => _app.MarkAsReadAsync(id);

    [HttpPost("read-all")]
    public Task MarkAllAsReadAsync() => _app.MarkAllAsReadAsync();
}
