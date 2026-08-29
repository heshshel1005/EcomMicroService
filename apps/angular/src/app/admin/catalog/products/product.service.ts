import { Injectable, inject } from '@angular/core';
import { RestService, SessionStateService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface ProductListDto {
  id: string;
  productNumber: string;
  name: string;
  categoryId?: string | null;
  categoryName?: string | null;
  brandId?: string | null;
  brandName?: string | null;
  modelId?: string | null;
  modelName?: string | null;
  productTypeId?: string | null;
  productTypeName?: string | null;
  requiredAttributeCount?: number;
  filledRequiredAttributeCount?: number;
  isAttributeComplete?: boolean;
  isPublished: boolean;
  priceFrom?: number | null;
}

export interface ProductVariantAttributeDto {
  productAttributeId: string;
  productAttributeName: string;
  value: string;
}

export interface ProductVariantDto {
  id: string;
  productId: string;
  sku: string;
  price?: number | null;
  quantity: number;
  reserved: number;
  availableQuantity: number;
  dynamicAttributes?: Record<string, unknown> | null;
  attributes: ProductVariantAttributeDto[];
}

export interface ProductDto {
  id: string;
  productNumber: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  brandId: string;
  brandName?: string | null;
  modelId?: string | null;
  modelName?: string | null;
  productTypeId?: string | null;
  dynamicAttributes?: Record<string, unknown> | null;
  isPublished: boolean;
  variants: ProductVariantDto[];
  translations?: ProductTranslationDto[];
}

export interface ProductTypeDto {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface ProductVariantAttributeInputDto {
  productAttributeId: string;
  value: string;
}

export interface CreateProductVariantDto {
  sku: string;
  price?: number | null;
  quantity: number;
  dynamicAttributes?: Record<string, unknown>;
  attributes: ProductVariantAttributeInputDto[];
}

export interface CreateProductDto {
  productNumber: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  brandId: string;
  modelId?: string | null;
  productTypeId?: string | null;
  dynamicAttributes?: Record<string, unknown>;
  isPublished: boolean;
  translations?: ProductTranslationDto[];
  variants: CreateProductVariantDto[];
}

export interface UpdateProductVariantDto {
  id?: string | null;
  sku: string;
  price?: number | null;
  quantity: number;
  dynamicAttributes?: Record<string, unknown>;
  attributes: ProductVariantAttributeInputDto[];
}

export interface UpdateProductDto {
  productNumber: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  brandId: string;
  modelId?: string | null;
  productTypeId?: string | null;
  dynamicAttributes?: Record<string, unknown>;
  isPublished: boolean;
  translations?: ProductTranslationDto[];
  variants: UpdateProductVariantDto[];
}

export interface ProductTranslationDto {
  language: string;
  name: string;
  description?: string | null;
}

export interface ProductListRequestDto {
  filter?: string;
  categoryId?: string | null;
  brandId?: string | null;
  modelId?: string | null;
  isPublished?: boolean | null;
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

export interface ProductAttributeDto {
  id: string;
  name: string;
}

export interface ProductMediaDto {
  id: string;
  productId: string;
  mediaType: number;
  sortOrder: number;
  isPrimary: boolean;
  altText?: string | null;
}

export interface ProductTypeAttributeRequirementItemDto {
  attributeDefinitionId: string;
  key: string;
  dataType: number;
  displayName?: string | null;
  displayNameLanguage?: string | null;
  fallbackDisplayName?: string | null;
  fallbackDisplayNameLanguage?: string | null;
  description?: string | null;
  descriptionLanguage?: string | null;
  fallbackDescription?: string | null;
  fallbackDescriptionLanguage?: string | null;
  allowedValuesJson?: string | null;
  localizedOptions?: ProductTypeAttributeOptionDto[];
  regexPattern?: string | null;
  minValue?: number | null;
  maxValue?: number | null;
  displayOrder: number;
  isRequired: boolean;
  isRecommended: boolean;
  conditionalAttributeKey?: string | null;
  conditionalOperator?: number | null;
  conditionalExpectedValue?: string | null;
}

export interface ProductTypeAttributeOptionDto {
  value: string;
  displayName?: string | null;
  displayNameLanguage?: string | null;
  fallbackDisplayName?: string | null;
  fallbackDisplayNameLanguage?: string | null;
}

export interface ProductTypeAttributeRequirementsDto {
  productTypeId: string;
  requiredAttributes: ProductTypeAttributeRequirementItemDto[];
  recommendedAttributes: ProductTypeAttributeRequirementItemDto[];
  conditionalAttributes: ProductTypeAttributeRequirementItemDto[];
}

export interface UpdateProductMediaDto {
  isPrimary: boolean;
  sortOrder: number;
  altText?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly rest = inject(RestService);
  private readonly sessionState = inject(SessionStateService);

  getList(params: ProductListRequestDto): Observable<PagedResultDto<ProductListDto>> {
    const requestParams: Record<string, string | number | undefined> = {};
    if (params.sorting != null) requestParams.Sorting = params.sorting;
    if (params.skipCount != null) requestParams.SkipCount = String(params.skipCount);
    if (params.maxResultCount != null) requestParams.MaxResultCount = String(params.maxResultCount);
    if (params.filter != null && params.filter !== '') requestParams.Filter = params.filter;
    if (params.categoryId != null && params.categoryId !== '') requestParams.CategoryId = params.categoryId;
    if (params.isPublished != null) requestParams.IsPublished = String(params.isPublished);
    return this.rest.request<void, PagedResultDto<ProductListDto>>({
      method: 'GET',
      url: '/api/catalog/product/list',
      params: requestParams,
    });
  }

  get(id: string): Observable<ProductDto> {
    return this.rest.request<void, ProductDto>({
      method: 'GET',
      url: `/api/catalog/product/${id}`,
    });
  }

  getAttributes(): Observable<ProductAttributeDto[]> {
    return this.rest.request<void, ProductAttributeDto[]>({
      method: 'GET',
      url: '/api/catalog/product/attributes',
    }).pipe(
      catchError(() => of([])),
    );
  }

  getProductTypes(): Observable<ProductTypeDto[]> {
    return this.rest.request<void, ProductTypeDto[]>({
      method: 'GET',
      url: '/api/catalog/product-type',
    }).pipe(
      catchError(() => of([])),
    );
  }

  getAttributeRequirementsByProductType(productTypeId: string): Observable<ProductTypeAttributeRequirementsDto | null> {
    return this.rest.request<void, ProductTypeAttributeRequirementsDto>({
      method: 'GET',
      url: '/api/catalog/product/attribute-requirements-by-product-type',
      params: { productTypeId },
    }).pipe(
      catchError(() => of(null)),
    );
  }

  create(input: CreateProductDto): Observable<ProductDto> {
    return this.rest.request<CreateProductDto, ProductDto>({
      method: 'POST',
      url: '/api/catalog/product',
      body: input,
    });
  }

  update(id: string, input: UpdateProductDto): Observable<ProductDto> {
    return this.rest.request<UpdateProductDto, ProductDto>({
      method: 'PUT',
      url: `/api/catalog/product/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/product/${id}`,
    });
  }

  getMediaByProduct(productId: string): Observable<ProductMediaDto[]> {
    return this.rest.request<void, ProductMediaDto[]>({
      method: 'GET',
      url: `/api/catalog/product-media/by-product/${productId}`,
    });
  }

  uploadMedia(productId: string, file: File, mediaType: number, sortOrder: number, isPrimary: boolean, altText?: string): Observable<ProductMediaDto> {
    const form = new FormData();
    form.append('ProductId', productId);
    form.append('File', file);
    form.append('MediaType', String(mediaType));
    form.append('SortOrder', String(sortOrder));
    form.append('IsPrimary', String(isPrimary));
    if (altText != null) form.append('AltText', altText);
    return this.rest.request<FormData, ProductMediaDto>({
      method: 'POST',
      url: '/api/catalog/product-media/upload',
      body: form,
    });
  }

  updateMedia(id: string, input: UpdateProductMediaDto): Observable<ProductMediaDto> {
    return this.rest.request<UpdateProductMediaDto, ProductMediaDto>({
      method: 'PUT',
      url: `/api/catalog/product-media/${id}`,
      body: input,
    });
  }

  deleteMedia(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/product-media/${id}`,
    });
  }

  getMediaFileUrl(id: string): string {
    const base = environment?.apis?.default?.url ?? '';
    const tenantId = this.sessionState.getTenant()?.id;
    const tenantQuery = tenantId ? `?__tenant=${encodeURIComponent(tenantId)}` : '';
    if (base) {
      return base.replace(/\/$/, '') + '/api/catalog/product-media/' + id + '/file' + tenantQuery;
    }
    return '/api/catalog/product-media/' + id + '/file' + tenantQuery;
  }
}
