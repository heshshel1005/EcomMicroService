using System;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized display name for <see cref="ProductType"/>.
/// </summary>
public class ProductTypeTranslation : Entity<Guid>, IEntityTranslation
{
    public Guid ProductTypeId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual ProductType? ProductType { get; set; }

    protected ProductTypeTranslation()
    {
    }

    public ProductTypeTranslation(Guid id, Guid productTypeId, string language, string name)
        : base(id)
    {
        ProductTypeId = productTypeId;
        Language = language ?? string.Empty;
        Name = name ?? string.Empty;
    }
}
