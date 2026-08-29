import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

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

export interface OrderStatusHistoryDto {
  id: string;
  orderId: string;
  status: string;
  creationTime: string;
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
  statusHistory?: OrderStatusHistoryDto[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly rest = inject(RestService);

  getMyOrders(): Observable<OrderDto[]> {
    return this.rest.request<void, unknown>({
      method: 'GET',
      url: '/api/ordering/orders/my-orders',
    }).pipe(
      map((res) => this.normalizeOrderList(res))
    );
  }

  get(id: string): Observable<OrderDto | null> {
    return this.rest.request<void, unknown>({
      method: 'GET',
      url: `/api/ordering/orders/${id}`,
    }).pipe(
      map((res) => this.normalizeOrder(res))
    );
  }

  /** Unwrap response (array or { result/body }) and normalize to OrderDto[] with camelCase. */
  private normalizeOrderList(res: unknown): OrderDto[] {
    const raw = res != null && typeof res === 'object' ? res as Record<string, unknown> : {};
    const arr = raw.result ?? raw.body ?? raw;
    if (!Array.isArray(arr)) return [];
    return arr.map((item) => this.normalizeOrderItem(item));
  }

  private normalizeOrderItem(o: unknown): OrderDto {
    const item = (o != null && typeof o === 'object' ? o : {}) as Record<string, unknown>;
    return {
      id: String(item.id ?? item.Id ?? ''),
      status: String(item.status ?? item.Status ?? ''),
      contactEmail: String(item.contactEmail ?? item.ContactEmail ?? ''),
      contactPhone: (item.contactPhone ?? item.ContactPhone) as string | null,
      contactName: (item.contactName ?? item.ContactName) as string | null,
      shippingStreet: String(item.shippingStreet ?? item.ShippingStreet ?? ''),
      shippingStreet2: (item.shippingStreet2 ?? item.ShippingStreet2) as string | null,
      shippingCity: (item.shippingCity ?? item.ShippingCity) as string | null,
      shippingRegion: (item.shippingRegion ?? item.ShippingRegion) as string | null,
      shippingPostalCode: (item.shippingPostalCode ?? item.ShippingPostalCode) as string | null,
      shippingCountry: (item.shippingCountry ?? item.ShippingCountry) as string | null,
      shippingMethodName: (item.shippingMethodName ?? item.ShippingMethodName) as string | null,
      subTotal: Number(item.subTotal ?? item.SubTotal ?? 0),
      shippingAmount: Number(item.shippingAmount ?? item.ShippingAmount ?? 0),
      taxAmount: Number(item.taxAmount ?? item.TaxAmount ?? 0),
      total: Number(item.total ?? item.Total ?? 0),
      creationTime: String(item.creationTime ?? item.CreationTime ?? ''),
      lines: [],
      statusHistory: [],
    };
  }

  private normalizeOrder(res: unknown): OrderDto | null {
    if (res == null) return null;
    const raw = typeof res === 'object' ? (res as Record<string, unknown>) : {};
    const data = raw.result ?? raw.body ?? raw;
    const item = data != null && typeof data === 'object' ? (data as Record<string, unknown>) : null;
    if (!item) return null;
    const dto = this.normalizeOrderItem(item);
    const linesRaw = (item.lines ?? item.Lines ?? []) as unknown[];
    dto.lines = linesRaw.map((l) => {
      const line = (l != null && typeof l === 'object' ? l : {}) as Record<string, unknown>;
      return {
        id: String(line.id ?? line.Id ?? ''),
        productVariantId: String(line.productVariantId ?? line.ProductVariantId ?? ''),
        productId: String(line.productId ?? line.ProductId ?? ''),
        productName: String(line.productName ?? line.ProductName ?? ''),
        sku: String(line.sku ?? line.Sku ?? ''),
        unitPrice: Number(line.unitPrice ?? line.UnitPrice ?? 0),
        quantity: Number(line.quantity ?? line.Quantity ?? 0),
        lineTotal: Number(line.lineTotal ?? line.LineTotal ?? 0),
      };
    });
    const historyRaw = (item.statusHistory ?? item.StatusHistory ?? []) as unknown[];
    dto.statusHistory = historyRaw.map((h) => {
      const hist = (h != null && typeof h === 'object' ? h : {}) as Record<string, unknown>;
      return {
        id: String(hist.id ?? hist.Id ?? ''),
        orderId: String(hist.orderId ?? hist.OrderId ?? ''),
        status: String(hist.status ?? hist.Status ?? ''),
        creationTime: String(hist.creationTime ?? hist.CreationTime ?? ''),
      };
    });
    return dto;
  }
}
