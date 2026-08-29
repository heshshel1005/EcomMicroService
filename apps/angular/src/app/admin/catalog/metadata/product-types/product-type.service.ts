import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ProductTypeTranslationDto {
  language: string;
  name: string;
}

export interface ProductTypeDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  translations: ProductTypeTranslationDto[];
}

export interface CreateProductTypeDto {
  code: string;
  name: string;
  isActive: boolean;
  translations: ProductTypeTranslationDto[];
}

export type UpdateProductTypeDto = CreateProductTypeDto;

@Injectable({ providedIn: 'root' })
export class ProductTypeService {
  private readonly rest = inject(RestService);

  getList(isActive?: boolean | null): Observable<ProductTypeDto[]> {
    const params: Record<string, string> = {};
    if (isActive != null) {
      params.IsActive = String(isActive);
    }

    return this.rest.request<void, ProductTypeDto[]>({
      method: 'GET',
      url: '/api/catalog/product-type',
      params,
    });
  }

  get(id: string): Observable<ProductTypeDto> {
    return this.rest.request<void, ProductTypeDto>({
      method: 'GET',
      url: `/api/catalog/product-type/${id}`,
    });
  }

  create(input: CreateProductTypeDto): Observable<ProductTypeDto> {
    return this.rest.request<CreateProductTypeDto, ProductTypeDto>({
      method: 'POST',
      url: '/api/catalog/product-type',
      body: input,
    });
  }

  update(id: string, input: UpdateProductTypeDto): Observable<ProductTypeDto> {
    return this.rest.request<UpdateProductTypeDto, ProductTypeDto>({
      method: 'PUT',
      url: `/api/catalog/product-type/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/product-type/${id}`,
    });
  }
}
