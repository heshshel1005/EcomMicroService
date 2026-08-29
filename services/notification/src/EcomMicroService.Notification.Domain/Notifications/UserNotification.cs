using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Notification;

public class UserNotification : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? NotificationType { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime NotificationDate { get; set; }
    public string? Data { get; set; }

    protected UserNotification() { }

    public UserNotification(
        Guid id,
        Guid? tenantId,
        Guid userId,
        string title,
        string? message = null,
        string? notificationType = null,
        string? linkUrl = null,
        string? data = null)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Title = title ?? string.Empty;
        Message = message;
        NotificationType = notificationType ?? "Info";
        LinkUrl = linkUrl;
        Data = data;
        IsRead = false;
        IsActive = true;
        NotificationDate = DateTime.UtcNow;
    }
}

