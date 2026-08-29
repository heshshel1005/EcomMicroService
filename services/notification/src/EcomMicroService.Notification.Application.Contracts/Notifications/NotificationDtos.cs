using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using System.Threading.Tasks;

namespace EcomMicroService.Notification;

public class UserNotificationDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? NotificationType { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime NotificationDate { get; set; }
}

public class GetNotificationsInput : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
}

public class NotificationCountDto
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
}

public interface IUserNotificationAppService : IApplicationService
{
    Task<PagedResultDto<UserNotificationDto>> GetListAsync(GetNotificationsInput input);
    Task<NotificationCountDto> GetUnreadCountAsync();
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
}
