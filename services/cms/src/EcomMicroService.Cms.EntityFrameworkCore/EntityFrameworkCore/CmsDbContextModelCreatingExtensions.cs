using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace EcomMicroService.Cms.EntityFrameworkCore;

public static class CmsDbContextModelCreatingExtensions
{
    public static void ConfigureCms(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
