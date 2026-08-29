import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface BrandModelDto {
  id: string;
  brandId: string;
  name: string;
  code?: string | null;
  isActive: boolean;
  translations?: BrandModelTranslationDto[];
}

export interface CreateBrandModelDto {
  brandId: string;
  name: string;
  code?: string | null;
  isActive: boolean;
  translations?: BrandModelTranslationDto[];
}

export type UpdateBrandModelDto = CreateBrandModelDto;

export interface BrandModelTranslationDto {
  language: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class BrandModelService {
  private readonly rest = inject(RestService);

  getList(brandId?: string | null, isActive?: boolean | null): Observable<BrandModelDto[]> {
    const params: Record<string, string> = {};
    if (brandId != null && brandId !== '') {
      params.BrandId = brandId;
    }
    if (isActive != null) {
      params.IsActive = String(isActive);
    }
    return this.rest.request<void, BrandModelDto[]>({
      method: 'GET',
      url: '/api/catalog/brand-model',
      params,
    });
  }

  getListByBrandId(brandId: string): Observable<BrandModelDto[]> {
    // Reuse the main list endpoint with brand filter and active models only
    return this.getList(brandId, true);
  }

  get(id: string): Observable<BrandModelDto> {
    return this.rest.request<void, BrandModelDto>({
      method: 'GET',
      url: `/api/catalog/brand-model/${id}`,
    });
  }

  create(input: CreateBrandModelDto): Observable<BrandModelDto> {
    return this.rest.request<CreateBrandModelDto, BrandModelDto>({
      method: 'POST',
      url: '/api/catalog/brand-model',
      body: input,
    });
  }

  update(id: string, input: UpdateBrandModelDto): Observable<BrandModelDto> {
    return this.rest.request<UpdateBrandModelDto, BrandModelDto>({
      method: 'PUT',
      url: `/api/catalog/brand-model/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/brand-model/${id}`,
    });
  }
}

