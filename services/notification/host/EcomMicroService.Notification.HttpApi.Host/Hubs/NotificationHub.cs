using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.MultiTenancy;

namespace EcomMicroService.Notification;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentTenant _currentTenant;

    public NotificationHub(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetTenantGroupName());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTenantGroupName());
        await base.OnDisconnectedAsync(exception);
    }

    private string GetTenantGroupName() =>
        _currentTenant.Id.HasValue ? $"tenant-{_currentTenant.Id.Value}" : "tenant-host";
}
