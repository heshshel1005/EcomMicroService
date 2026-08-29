using EcomMicroService.Projects.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EcomMicroService.Projects;

public abstract class ProjectsController : AbpControllerBase
{
    protected ProjectsController()
    {
        LocalizationResource = typeof(ProjectsResource);
    }
}
