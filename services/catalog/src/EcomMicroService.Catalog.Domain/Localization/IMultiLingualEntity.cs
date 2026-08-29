using System.Collections.Generic;

namespace EcomMicroService.Catalog.Localization;

public interface IMultiLingualEntity<TTranslation>
    where TTranslation : IEntityTranslation
{
    ICollection<TTranslation> Translations { get; set; }
}

public interface IEntityTranslation
{
    string Language { get; set; }
}
