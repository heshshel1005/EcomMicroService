using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Cms.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.CmsDb)]
public interface ICmsDbContext : IEfCoreDbContext
{
}
