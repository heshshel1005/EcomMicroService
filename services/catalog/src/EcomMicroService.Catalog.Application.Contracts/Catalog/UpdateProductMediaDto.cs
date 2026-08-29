namespace EcomMicroService.Catalog;

/// <summary>
/// Input for updating product media (primary, order, alt text).
/// </summary>
public class UpdateProductMediaDto
{
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public string? AltText { get; set; }
}
