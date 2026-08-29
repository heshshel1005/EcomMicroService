import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ProductReviewDto {
  id: string;
  productId: string;
  userId: string;
  authorDisplayName: string;
  rating: number;
  reviewText?: string | null;
  status: number;
  creationTime: string;
}

export interface ProductReviewListRequestDto {
  productId?: string | null;
  status?: number | null;
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

/** Status enum matching backend ProductReviewStatus */
export const ReviewStatus = { Pending: 0, Approved: 1, Rejected: 2 } as const;

@Injectable({ providedIn: 'root' })
export class ProductReviewAdminService {
  private readonly rest = inject(RestService);

  getList(params: ProductReviewListRequestDto): Observable<PagedResultDto<ProductReviewDto>> {
    const requestParams: Record<string, string | number | undefined> = {};
    if (params.productId != null && params.productId !== '') requestParams.ProductId = params.productId;
    if (params.status != null) requestParams.Status = String(params.status);
    if (params.sorting != null) requestParams.Sorting = params.sorting;
    if (params.skipCount != null) requestParams.SkipCount = String(params.skipCount);
    if (params.maxResultCount != null) requestParams.MaxResultCount = String(params.maxResultCount);
    return this.rest.request<void, PagedResultDto<ProductReviewDto>>({
      method: 'GET',
      url: '/api/catalog/product-review-admin',
      params: requestParams,
    });
  }

  approve(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'POST',
      url: `/api/catalog/product-review-admin/${id}/approve`,
    });
  }

  reject(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'POST',
      url: `/api/catalog/product-review-admin/${id}/reject`,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/product-review-admin/${id}`,
    });
  }
}
