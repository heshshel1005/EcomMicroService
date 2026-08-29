using System;
using EcomMicroService.Catalog.Localization;
using Volo.Abp.Domain.Entities;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized text values for <see cref="BrandModel"/>.
/// </summary>
public class BrandModelTranslation : Entity<Guid>, IEntityTranslation
{
    public Guid BrandModelId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual BrandModel? BrandModel { get; set; }

    protected BrandModelTranslation()
    {
    }

    public BrandModelTranslation(Guid id, Guid brandModelId, string language, string name)
        : base(id)
    {
        BrandModelId = brandModelId;
        Language = language ?? string.Empty;
        Name = name ?? string.Empty;
    }
}
