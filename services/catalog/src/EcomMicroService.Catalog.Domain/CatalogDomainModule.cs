using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EcomMicroService.Catalog;

[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(CatalogDomainSharedModule))]
public class CatalogDomainModule : AbpModule { }
