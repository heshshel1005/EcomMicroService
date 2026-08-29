using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EcomMicroService.Catalog;

/// <summary>
/// Parses <see cref="AttributeDefinition.AllowedValuesJson"/> preserving JSON array order (trimmed, first casing wins for case-insensitive duplicates).
/// </summary>
public static class AttributeAllowedValuesParser
{
    public static IReadOnlyList<string> ParseOrdered(string? allowedValuesJson)
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

        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in values ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                continue;
            }

            var t = v.Trim();
            if (seen.Add(t))
            {
                result.Add(t);
            }
        }

        return result;
    }
}
