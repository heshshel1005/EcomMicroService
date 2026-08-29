using System;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized text values for <see cref="Product"/>.
/// </summary>
public class ProductTranslation : Entity<Guid>, IEntityTranslation
{
    public Guid ProductId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual Product? Product { get; set; }

    protected ProductTranslation()
    {
    }

    public ProductTranslation(Guid id, Guid productId, string language, string name, string? description = null)
        : base(id)
    {
        ProductId = productId;
        Language = language ?? string.Empty;
        Name = name ?? string.Empty;
        Description = description;
    }
}
