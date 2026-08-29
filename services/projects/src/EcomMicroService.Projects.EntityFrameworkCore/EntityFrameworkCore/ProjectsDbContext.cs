using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Projects.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.ProjectsDb)]
public class ProjectsDbContext(DbContextOptions<ProjectsDbContext> options)
    : AbpDbContext<ProjectsDbContext>(options),
        IProjectsDbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureProjects();
    }
}
