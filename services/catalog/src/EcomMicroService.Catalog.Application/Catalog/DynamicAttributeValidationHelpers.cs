using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EcomMicroService.Catalog;

internal static class DynamicAttributeValueNormalizer
{
    public static string? Normalize(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.String => element.GetString()?.Trim(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.ToString(),
                _ => element.GetRawText()
            };
        }

        var text = value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}

internal static class ProductTypeAttributeRuleConditionEvaluator
{
    /// <summary>
    /// When true, <see cref="AttributeDefinition.IsRequired"/> (and any future requirement logic tied to the rule)
    /// applies for this rule. When the rule defines no condition, this is always true.
    /// </summary>
    public static bool IsRuleConditionSatisfied(
        ProductTypeAttributeRule rule,
        IReadOnlyDictionary<string, object?> normalizedAttributes)
    {
        if (string.IsNullOrWhiteSpace(rule.ConditionalAttributeKey) || !rule.ConditionalOperator.HasValue)
        {
            return true;
        }

        var key = rule.ConditionalAttributeKey.Trim();
        normalizedAttributes.TryGetValue(key, out var raw);
        var actual = DynamicAttributeValueNormalizer.Normalize(raw) ?? string.Empty;
        var expected = rule.ConditionalExpectedValue?.Trim() ?? string.Empty;

        return rule.ConditionalOperator.Value switch
        {
            ProductTypeRuleConditionOperator.Equals => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            ProductTypeRuleConditionOperator.NotEquals => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            ProductTypeRuleConditionOperator.Contains => actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0,
            ProductTypeRuleConditionOperator.NotContains => actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) < 0,
            _ => true
        };
    }
}
