using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EcomMicroService.Catalog;

internal static class AttributeAllowedValues
{
    public static IReadOnlyList<string> Parse(string? allowedValuesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedValuesJson))
        {
            return Array.Empty<string>();
        }

        List<string>? values;
        try
        {
            values = JsonSerializer.Deserialize<List<string>>(allowedValuesJson);
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }

        return (values ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> ParseOrdered(string? allowedValuesJson)
    {
        return AttributeAllowedValuesParser.ParseOrdered(allowedValuesJson);
    }
}
