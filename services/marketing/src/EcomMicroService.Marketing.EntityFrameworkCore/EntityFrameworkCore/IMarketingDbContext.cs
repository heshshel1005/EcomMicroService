using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Marketing.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.MarketingDb)]
public interface IMarketingDbContext : IEfCoreDbContext
{
}
