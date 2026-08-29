using EcomMicroService.Projects.Localization;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Projects;

public abstract class ProjectsAppService : ApplicationService
{
    protected ProjectsAppService()
    {
        LocalizationResource = typeof(ProjectsResource);
        ObjectMapperContext = typeof(ProjectsApplicationModule);
    }
}
