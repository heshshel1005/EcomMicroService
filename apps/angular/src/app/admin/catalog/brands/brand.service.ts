import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface BrandDto {
  id: string;
  name: string;
  slug?: string | null;
  description?: string | null;
  isActive: boolean;
  translations?: BrandTranslationDto[];
}

export interface CreateBrandDto {
  name: string;
  slug?: string | null;
  description?: string | null;
  isActive: boolean;
  translations?: BrandTranslationDto[];
}

export type UpdateBrandDto = CreateBrandDto;

export interface BrandTranslationDto {
  language: string;
  name: string;
  description?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly rest = inject(RestService);

  getList(isActive?: boolean | null): Observable<BrandDto[]> {
    const params: Record<string, string> = {};
    if (isActive != null) {
      params.IsActive = String(isActive);
    }
    return this.rest.request<void, BrandDto[]>({
      method: 'GET',
      url: '/api/catalog/brand',
      params,
    });
  }

  get(id: string): Observable<BrandDto> {
    return this.rest.request<void, BrandDto>({
      method: 'GET',
      url: `/api/catalog/brand/${id}`,
    });
  }

  create(input: CreateBrandDto): Observable<BrandDto> {
    return this.rest.request<CreateBrandDto, BrandDto>({
      method: 'POST',
      url: '/api/catalog/brand',
      body: input,
    });
  }

  update(id: string, input: UpdateBrandDto): Observable<BrandDto> {
    return this.rest.request<UpdateBrandDto, BrandDto>({
      method: 'PUT',
      url: `/api/catalog/brand/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/brand/${id}`,
    });
  }
}

