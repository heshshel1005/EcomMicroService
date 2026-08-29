using System;
using System.Security.Cryptography;
using System.Text;

namespace EcomMicroService.Catalog;

/// <summary>
/// Deterministic surrogate key for attribute option rows derived from definition id and invariant option value.
/// </summary>
public static class AttributeOptionIdFactory
{
    public static Guid Create(Guid attributeDefinitionId, string optionValue)
    {
        var normalized = $"{attributeDefinitionId:D}:{optionValue.Trim().ToLowerInvariant()}";
        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hash = MD5.HashData(bytes);
        return new Guid(hash);
    }
}
