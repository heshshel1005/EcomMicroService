using System;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized text values for <see cref="Brand"/>.
/// </summary>
public class BrandTranslation : Entity<Guid>, IEntityTranslation
{
    public Guid BrandId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual Brand? Brand { get; set; }

    protected BrandTranslation()
    {
    }

    public BrandTranslation(Guid id, Guid brandId, string language, string name, string? description = null)
        : base(id)
    {
        BrandId = brandId;
        Language = language ?? string.Empty;
        Name = name ?? string.Empty;
        Description = description;
    }
}
