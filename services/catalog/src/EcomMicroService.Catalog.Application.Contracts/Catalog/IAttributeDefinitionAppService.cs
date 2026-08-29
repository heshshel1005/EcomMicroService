using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EcomMicroService.Catalog;

public interface IAttributeDefinitionAppService : IApplicationService
{
    Task<List<AttributeDefinitionDto>> GetListAsync();
    Task<AttributeDefinitionDto> GetAsync(Guid id);
    Task<AttributeDefinitionDto> CreateAsync(CreateAttributeDefinitionDto input);
    Task<AttributeDefinitionDto> UpdateAsync(Guid id, UpdateAttributeDefinitionDto input);
    Task DeleteAsync(Guid id);

    Task<AttributeDefinitionDto> SubmitForReviewAsync(Guid id);
    Task<AttributeDefinitionDto> RejectReviewAsync(Guid id);
    Task<AttributeDefinitionDto> PublishAsync(Guid id);
    Task<AttributeDefinitionDto> ArchiveAsync(Guid id);
    Task<AttributeDefinitionDto> DemoteToDraftAsync(Guid id);

    Task<List<AttributeOptionTranslationDto>> GetOptionTranslationsAsync(Guid attributeDefinitionId);

    Task<List<AttributeOptionTranslationDto>> SaveOptionTranslationsAsync(
        Guid attributeDefinitionId,
        SaveAttributeOptionTranslationsDto input);
}
