using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EcomMicroService.Catalog.Localization;

namespace EcomMicroService.Catalog;

internal static class CatalogTranslationResolver
{
    private const string FallbackDefaultLanguage = "en";
    private static readonly StringComparer LanguageComparer = StringComparer.OrdinalIgnoreCase;

    public static TTranslation? Resolve<TTranslation>(
        IEnumerable<TTranslation>? translations,
        string? currentCultureName,
        string? defaultLanguage = null)
        where TTranslation : class, IEntityTranslation
    {
        var translationList = translations?
            .Where(x => !string.IsNullOrWhiteSpace(x.Language))
            .ToList();

        if (translationList == null || translationList.Count == 0)
        {
            return null;
        }

        foreach (var language in GetFallbackLanguageOrder(currentCultureName, defaultLanguage))
        {
            var match = translationList.FirstOrDefault(x => LanguageComparer.Equals(NormalizeLanguage(x.Language), language));
            if (match != null)
            {
                return match;
            }
        }

        return translationList.First();
    }

    internal static IReadOnlyList<string> GetFallbackLanguageOrder(string? currentCultureName, string? defaultLanguage = null)
    {
        var order = new List<string>();
        var seen = new HashSet<string>(LanguageComparer);

        void Add(string? value)
        {
            var normalized = NormalizeLanguage(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            if (seen.Add(normalized))
            {
                order.Add(normalized);
            }
        }

        var exactCulture = NormalizeLanguage(currentCultureName);
        Add(exactCulture);
        Add(GetNeutralLanguage(exactCulture));

        var normalizedDefaultLanguage = NormalizeLanguage(defaultLanguage);
        Add(string.IsNullOrWhiteSpace(normalizedDefaultLanguage)
            ? FallbackDefaultLanguage
            : normalizedDefaultLanguage);

        return order;
    }

    private static string NormalizeLanguage(string? language)
    {
        return language?.Trim().Replace('_', '-') ?? string.Empty;
    }

    private static string GetNeutralLanguage(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return string.Empty;
        }

        try
        {
            return CultureInfo.GetCultureInfo(cultureName).TwoLetterISOLanguageName;
        }
        catch (CultureNotFoundException)
        {
            var separatorIndex = cultureName.IndexOf('-');
            if (separatorIndex > 0)
            {
                return cultureName[..separatorIndex];
            }

            return cultureName;
        }
    }

    internal static AttributeDefinitionTranslation? ResolveCurrentCultureDefinitionTranslation(
        IReadOnlyCollection<AttributeDefinitionTranslation>? translations,
        string? currentCultureName)
    {
        return ResolveCurrentCultureRow(translations, currentCultureName, x => x.Language);
    }

    internal static AttributeDefinitionTranslation? ResolveFallbackDefinitionTranslation(
        IReadOnlyCollection<AttributeDefinitionTranslation>? translations,
        string? currentCultureName,
        string? defaultLanguage)
    {
        return ResolveFallbackRow(
            translations,
            currentCultureName,
            defaultLanguage,
            x => x.Language,
            x => x.DisplayName);
    }

    internal static AttributeOptionTranslation? ResolveCurrentCultureOptionTranslation(
        IReadOnlyCollection<AttributeOptionTranslation>? translations,
        string? currentCultureName)
    {
        return ResolveCurrentCultureRow(translations, currentCultureName, x => x.Language);
    }

    internal static AttributeOptionTranslation? ResolveFallbackOptionTranslation(
        IReadOnlyCollection<AttributeOptionTranslation>? translations,
        string? currentCultureName,
        string? defaultLanguage)
    {
        return ResolveFallbackRow(
            translations,
            currentCultureName,
            defaultLanguage,
            x => x.Language,
            x => x.DisplayName);
    }

    private static T? ResolveCurrentCultureRow<T>(
        IReadOnlyCollection<T>? translations,
        string? currentCultureName,
        Func<T, string> getLanguage)
        where T : class
    {
        if (translations == null || translations.Count == 0)
        {
            return null;
        }

        var currentOrder = GetFallbackLanguageOrder(currentCultureName, null);
        foreach (var language in currentOrder.Take(2))
        {
            var match = translations
                .FirstOrDefault(x => string.Equals(NormalizeLanguage(getLanguage(x)), language, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static T? ResolveFallbackRow<T>(
        IReadOnlyCollection<T>? translations,
        string? currentCultureName,
        string? defaultLanguage,
        Func<T, string> getLanguage,
        Func<T, string> getDisplayNameForSort)
        where T : class
    {
        if (translations == null || translations.Count == 0)
        {
            return null;
        }

        if (ResolveCurrentCultureRow(translations, currentCultureName, getLanguage) != null)
        {
            return null;
        }

        foreach (var language in GetFallbackLanguageOrder(currentCultureName, defaultLanguage).Skip(2))
        {
            var match = translations
                .Where(x => !string.IsNullOrWhiteSpace(getLanguage(x)))
                .OrderBy(x => NormalizeLanguage(getLanguage(x)))
                .ThenBy(x => getDisplayNameForSort(x))
                .FirstOrDefault(x => string.Equals(NormalizeLanguage(getLanguage(x)), language, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return translations
            .Where(x => !string.IsNullOrWhiteSpace(getLanguage(x)))
            .OrderBy(x => NormalizeLanguage(getLanguage(x)))
            .ThenBy(x => getDisplayNameForSort(x))
            .FirstOrDefault();
    }
}
