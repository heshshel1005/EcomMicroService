import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface OrderStatusHistoryDto {
  id: string;
  orderId: string;
  status: string;
  creationTime: string;
}

export interface OrderLineDto {
  id: string;
  productVariantId: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderDto {
  id: string;
  status: string;
  contactEmail: string;
  contactPhone?: string | null;
  contactName?: string | null;
  shippingStreet: string;
  shippingStreet2?: string | null;
  shippingCity?: string | null;
  shippingRegion?: string | null;
  shippingPostalCode?: string | null;
  shippingCountry?: string | null;
  shippingMethodName?: string | null;
  subTotal: number;
  shippingAmount: number;
  taxAmount: number;
  total: number;
  creationTime: string;
  lines: OrderLineDto[];
  statusHistory: OrderStatusHistoryDto[];
}

export interface OrderListDto {
  id: string;
  status: string;
  contactEmail: string;
  contactName?: string | null;
  total: number;
  creationTime: string;
  userId?: string | null;
}

export interface OrderListRequestDto {
  status?: string;
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

export interface UpdateOrderStatusDto {
  status: string;
}

@Injectable({ providedIn: 'root' })
export class OrderAdminService {
  private readonly rest = inject(RestService);

  getList(params: OrderListRequestDto): Observable<PagedResultDto<OrderListDto>> {
    const requestParams: Record<string, string | number | undefined> = {};
    if (params.sorting != null) requestParams.Sorting = params.sorting ?? '';
    if (params.skipCount != null) requestParams.SkipCount = String(params.skipCount);
    if (params.maxResultCount != null) requestParams.MaxResultCount = String(params.maxResultCount ?? 10);
    if (params.status != null && params.status !== '') requestParams.Status = params.status;
    return this.rest.request<void, PagedResultDto<OrderListDto>>({
      method: 'GET',
      url: '/api/ordering/order-admin',
      params: requestParams,
    });
  }

  get(id: string): Observable<OrderDto> {
    return this.rest.request<void, OrderDto>({
      method: 'GET',
      url: `/api/ordering/order-admin/${id}`,
    });
  }

  updateStatus(id: string, status: string): Observable<OrderDto> {
    return this.rest.request<UpdateOrderStatusDto, OrderDto>({
      method: 'PUT',
      url: `/api/ordering/order-admin/${id}/status`,
      body: { status },
    });
  }
}
