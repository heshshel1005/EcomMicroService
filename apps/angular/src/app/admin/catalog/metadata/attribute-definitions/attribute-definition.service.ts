import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export enum AttributeDefinitionDataType {
  Text = 0,
  Number = 1,
  Boolean = 2,
  Date = 3,
  Enum = 4,
  Json = 5,
}

export enum AttributeDefinitionGovernanceStatus {
  Draft = 0,
  PendingReview = 1,
  Published = 2,
  Archived = 3,
}

export interface AttributeDefinitionTranslationDto {
  language: string;
  name: string;
  description?: string | null;
}

export interface AttributeDefinitionDto {
  id: string;
  key: string;
  dataType: AttributeDefinitionDataType;
  allowedValuesJson?: string | null;
  regexPattern?: string | null;
  minValue?: number | null;
  maxValue?: number | null;
  isRequired: boolean;
  isRecommended: boolean;
  governanceStatus: AttributeDefinitionGovernanceStatus;
  publishedVersion: number;
  displayName?: string | null;
  displayNameLanguage?: string | null;
  fallbackDisplayName?: string | null;
  fallbackDisplayNameLanguage?: string | null;
  description?: string | null;
  translations?: AttributeDefinitionTranslationDto[];
}

export interface CreateAttributeDefinitionDto {
  key: string;
  dataType: AttributeDefinitionDataType;
  allowedValuesJson?: string | null;
  regexPattern?: string | null;
  minValue?: number | null;
  maxValue?: number | null;
  isRequired: boolean;
  isRecommended: boolean;
  translations: AttributeDefinitionTranslationDto[];
}

export type UpdateAttributeDefinitionDto = CreateAttributeDefinitionDto;

export interface AttributeOptionTranslationItemDto {
  language: string;
  displayName: string;
}

export interface AttributeOptionTranslationDto {
  value: string;
  optionId: string;
  displayName?: string | null;
  displayNameLanguage?: string | null;
  fallbackDisplayName?: string | null;
  fallbackDisplayNameLanguage?: string | null;
  translations: AttributeOptionTranslationItemDto[];
}

export interface AttributeOptionTranslationsInputDto {
  value: string;
  translations: AttributeOptionTranslationItemDto[];
}

export interface SaveAttributeOptionTranslationsDto {
  options: AttributeOptionTranslationsInputDto[];
}

@Injectable({ providedIn: 'root' })
export class AttributeDefinitionService {
  private readonly rest = inject(RestService);

  getList(): Observable<AttributeDefinitionDto[]> {
    return this.rest.request<void, AttributeDefinitionDto[]>({
      method: 'GET',
      url: '/api/catalog/attribute-definition',
    });
  }

  get(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'GET',
      url: `/api/catalog/attribute-definition/${id}`,
    });
  }

  create(input: CreateAttributeDefinitionDto): Observable<AttributeDefinitionDto> {
    return this.rest.request<CreateAttributeDefinitionDto, AttributeDefinitionDto>({
      method: 'POST',
      url: '/api/catalog/attribute-definition',
      body: input,
    });
  }

  update(id: string, input: UpdateAttributeDefinitionDto): Observable<AttributeDefinitionDto> {
    return this.rest.request<UpdateAttributeDefinitionDto, AttributeDefinitionDto>({
      method: 'PUT',
      url: `/api/catalog/attribute-definition/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/attribute-definition/${id}`,
    });
  }

  submitForReview(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/submit-for-review/${id}`,
    });
  }

  rejectReview(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/reject-review/${id}`,
    });
  }

  publish(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/publish/${id}`,
    });
  }

  archive(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/archive/${id}`,
    });
  }

  demoteToDraft(id: string): Observable<AttributeDefinitionDto> {
    return this.rest.request<void, AttributeDefinitionDto>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/demote-to-draft/${id}`,
    });
  }

  getOptionTranslations(attributeDefinitionId: string): Observable<AttributeOptionTranslationDto[]> {
    return this.rest.request<void, AttributeOptionTranslationDto[]>({
      method: 'GET',
      url: `/api/catalog/attribute-definition/option-translations/${attributeDefinitionId}`,
    });
  }

  saveOptionTranslations(
    attributeDefinitionId: string,
    input: SaveAttributeOptionTranslationsDto
  ): Observable<AttributeOptionTranslationDto[]> {
    return this.rest.request<SaveAttributeOptionTranslationsDto, AttributeOptionTranslationDto[]>({
      method: 'POST',
      url: `/api/catalog/attribute-definition/save-option-translations/${attributeDefinitionId}`,
      body: input,
    });
  }
}
