using System.Collections.Generic;

namespace EcomMicroService.Catalog;

/// <summary>
/// Structured validation failure payload for UI/API consumers.
/// </summary>
public class ValidationErrorResponseDto
{
    /// <summary>
    /// Stable machine-readable code for the validation failure group.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional localized summary message for the whole validation response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Per-field validation issues.
    /// </summary>
    public List<FieldValidationErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// One field-level validation issue.
/// </summary>
public class FieldValidationErrorDto
{
    /// <summary>
    /// Input field key/path used by UI to map errors to controls.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Stable machine-readable validation code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Localized human-readable message for UI display.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional attempted/invalid value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Optional additional metadata (min/max/pattern/allowedValues/etc.).
    /// </summary>
    public Dictionary<string, string?> Metadata { get; set; } = new();
}
