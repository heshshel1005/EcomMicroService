using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace EcomMicroService.Catalog;

[DependsOn(typeof(CatalogDomainSharedModule))]
[DependsOn(typeof(AbpDddApplicationContractsModule))]
[DependsOn(typeof(AbpAuthorizationModule))]
public class CatalogApplicationContractsModule : AbpModule { }
