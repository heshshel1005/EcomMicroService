using Volo.Abp.Modularity;

namespace EcomMicroService.Catalog;

[DependsOn(typeof(CatalogApplicationModule))]
[DependsOn(typeof(CatalogDomainTestModule))]
public class CatalogApplicationTestModule : AbpModule { }
