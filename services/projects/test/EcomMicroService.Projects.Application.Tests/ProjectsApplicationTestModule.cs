using Volo.Abp.Modularity;

namespace EcomMicroService.Projects;

[DependsOn(typeof(ProjectsApplicationModule))]
[DependsOn(typeof(ProjectsDomainTestModule))]
public class ProjectsApplicationTestModule : AbpModule { }
