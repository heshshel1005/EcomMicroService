namespace EcomMicroService.Catalog;

/// <summary>
/// Shared shape for translation DTOs.
/// </summary>
public interface ITranslationDto
{
    string Language { get; set; }
}

/// <summary>
/// Shared shape for translation DTOs containing a localized name.
/// </summary>
public interface INameTranslationDto : ITranslationDto
{
    string Name { get; set; }
}

/// <summary>
/// Shared shape for translation DTOs containing localized name and description.
/// </summary>
public interface INameDescriptionTranslationDto : INameTranslationDto
{
    string? Description { get; set; }
}
