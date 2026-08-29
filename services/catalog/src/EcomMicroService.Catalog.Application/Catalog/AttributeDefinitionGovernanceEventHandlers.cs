using System.Threading.Tasks;
using EcomMicroService.Catalog.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace EcomMicroService.Catalog;

public class AttributeDefinitionSubmittedForReviewLocalEventHandler
    : ILocalEventHandler<AttributeDefinitionSubmittedForReviewEvent>, ITransientDependency
{
    private readonly ILogger<AttributeDefinitionSubmittedForReviewLocalEventHandler> _logger;

    public AttributeDefinitionSubmittedForReviewLocalEventHandler(
        ILogger<AttributeDefinitionSubmittedForReviewLocalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(AttributeDefinitionSubmittedForReviewEvent eventData)
    {
        _logger.LogInformation(
            "Attribute definition submitted for review: {AttributeDefinitionId}, tenant {TenantId}",
            eventData.AttributeDefinitionId,
            eventData.TenantId);
        return Task.CompletedTask;
    }
}

public class AttributeDefinitionReviewRejectedLocalEventHandler
    : ILocalEventHandler<AttributeDefinitionReviewRejectedEvent>, ITransientDependency
{
    private readonly ILogger<AttributeDefinitionReviewRejectedLocalEventHandler> _logger;

    public AttributeDefinitionReviewRejectedLocalEventHandler(
        ILogger<AttributeDefinitionReviewRejectedLocalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(AttributeDefinitionReviewRejectedEvent eventData)
    {
        _logger.LogInformation(
            "Attribute definition review rejected (returned to draft): {AttributeDefinitionId}, tenant {TenantId}",
            eventData.AttributeDefinitionId,
            eventData.TenantId);
        return Task.CompletedTask;
    }
}

public class AttributeDefinitionPublishedLocalEventHandler
    : ILocalEventHandler<AttributeDefinitionPublishedEvent>, ITransientDependency
{
    private readonly ILogger<AttributeDefinitionPublishedLocalEventHandler> _logger;

    public AttributeDefinitionPublishedLocalEventHandler(ILogger<AttributeDefinitionPublishedLocalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(AttributeDefinitionPublishedEvent eventData)
    {
        _logger.LogInformation(
            "Attribute definition published: {AttributeDefinitionId}, tenant {TenantId}, version {PublishedVersion}",
            eventData.AttributeDefinitionId,
            eventData.TenantId,
            eventData.PublishedVersion);
        return Task.CompletedTask;
    }
}

public class AttributeDefinitionArchivedLocalEventHandler
    : ILocalEventHandler<AttributeDefinitionArchivedEvent>, ITransientDependency
{
    private readonly ILogger<AttributeDefinitionArchivedLocalEventHandler> _logger;

    public AttributeDefinitionArchivedLocalEventHandler(ILogger<AttributeDefinitionArchivedLocalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(AttributeDefinitionArchivedEvent eventData)
    {
        _logger.LogInformation(
            "Attribute definition archived: {AttributeDefinitionId}, tenant {TenantId}",
            eventData.AttributeDefinitionId,
            eventData.TenantId);
        return Task.CompletedTask;
    }
}

public class AttributeDefinitionDemotedToDraftLocalEventHandler
    : ILocalEventHandler<AttributeDefinitionDemotedToDraftEvent>, ITransientDependency
{
    private readonly ILogger<AttributeDefinitionDemotedToDraftLocalEventHandler> _logger;

    public AttributeDefinitionDemotedToDraftLocalEventHandler(
        ILogger<AttributeDefinitionDemotedToDraftLocalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(AttributeDefinitionDemotedToDraftEvent eventData)
    {
        _logger.LogInformation(
            "Attribute definition demoted to draft: {AttributeDefinitionId}, tenant {TenantId}",
            eventData.AttributeDefinitionId,
            eventData.TenantId);
        return Task.CompletedTask;
    }
}
