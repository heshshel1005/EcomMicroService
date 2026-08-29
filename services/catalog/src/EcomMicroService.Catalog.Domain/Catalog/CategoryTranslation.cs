using System;
using Volo.Abp.Domain.Entities;
using EcomMicroService.Catalog.Localization;

namespace EcomMicroService.Catalog;

/// <summary>
/// Localized text values for <see cref="Category"/>.
/// </summary>
public class CategoryTranslation : Entity<Guid>, IEntityTranslation
{
    public Guid CategoryId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual Category? Category { get; set; }

    protected CategoryTranslation()
    {
    }

    public CategoryTranslation(Guid id, Guid categoryId, string language, string name)
        : base(id)
    {
        CategoryId = categoryId;
        Language = language ?? string.Empty;
        Name = name ?? string.Empty;
    }
}
