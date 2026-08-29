using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace EcomMicroService.Catalog.Localization;

public static class MultilingualDomainGuard
{
    public static void ValidateRequiredDefaultAndNoDuplicates<TTranslation>(
        IEnumerable<TTranslation> translations,
        string defaultLanguage,
        string duplicateLanguageErrorCode,
        string defaultTranslationRequiredErrorCode)
        where TTranslation : IEntityTranslation
    {
        if (translations == null)
        {
            throw new BusinessException(defaultTranslationRequiredErrorCode);
        }

        var normalizedDefaultLanguage = NormalizeLanguage(defaultLanguage);
        var normalizedLanguages = new List<string>();

        foreach (var translation in translations)
        {
            var normalizedLanguage = NormalizeLanguage(translation?.Language);
            if (string.IsNullOrWhiteSpace(normalizedLanguage))
            {
                throw new BusinessException(CatalogDomainErrorCodes.TranslationLanguageRequired);
            }

            normalizedLanguages.Add(normalizedLanguage);
        }

        var duplicateLanguage = normalizedLanguages
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1)?
            .Key;

        if (!string.IsNullOrWhiteSpace(duplicateLanguage))
        {
            throw new BusinessException(duplicateLanguageErrorCode)
                .WithData("Language", duplicateLanguage);
        }

        var hasDefaultLanguage = normalizedLanguages.Any(x =>
            string.Equals(x, normalizedDefaultLanguage, StringComparison.OrdinalIgnoreCase));

        if (!hasDefaultLanguage)
        {
            throw new BusinessException(defaultTranslationRequiredErrorCode)
                .WithData("DefaultLanguage", normalizedDefaultLanguage);
        }
    }

    public static string NormalizeLanguage(string? language)
    {
        return language?.Trim() ?? string.Empty;
    }
}
