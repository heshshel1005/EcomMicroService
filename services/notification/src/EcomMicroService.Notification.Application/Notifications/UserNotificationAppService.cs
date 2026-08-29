using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Notification;

[Authorize]
public class UserNotificationAppService : ApplicationService, IUserNotificationAppService
{
    private readonly IRepository<UserNotification, Guid> _repository;

    public UserNotificationAppService(IRepository<UserNotification, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<UserNotificationDto>> GetListAsync(GetNotificationsInput input)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var query = await _repository.GetQueryableAsync();
        query = query.Where(n => n.UserId == userId && n.IsActive);
        if (input.IsRead.HasValue)
            query = query.Where(n => n.IsRead == input.IsRead.Value);
        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(n => n.NotificationDate).Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10));
        return new PagedResultDto<UserNotificationDto>(total, items.Select(Map).ToList());
    }

    public async Task<NotificationCountDto> GetUnreadCountAsync()
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var unread = await _repository.CountAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);
        var total = await _repository.CountAsync(n => n.UserId == userId && n.IsActive);
        return new NotificationCountDto { UnreadCount = unread, TotalCount = total };
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var n = await _repository.GetAsync(id);
        if (n.UserId != CurrentUser.Id) throw new Volo.Abp.Authorization.AbpAuthorizationException();
        n.IsRead = true;
        n.ReadTime = DateTime.UtcNow;
        await _repository.UpdateAsync(n);
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var list = await _repository.GetListAsync(n => n.UserId == userId && !n.IsRead && n.IsActive);
        foreach (var n in list)
        {
            n.IsRead = true;
            n.ReadTime = DateTime.UtcNow;
        }
        await _repository.UpdateManyAsync(list);
    }

    private static UserNotificationDto Map(UserNotification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Title = n.Title,
        Message = n.Message,
        NotificationType = n.NotificationType,
        LinkUrl = n.LinkUrl,
        IsRead = n.IsRead,
        NotificationDate = n.NotificationDate
    };
}
