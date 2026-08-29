import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

export interface AnalyticsFilter {
  dateFrom?: string | null;
  dateTo?: string | null;
}

export interface SalesSummary {
  totalOrders: number;
  totalRevenue: number;
  periodStart?: string | null;
  periodEnd?: string | null;
}

export interface SalesByDay {
  date: string;
  orderCount: number;
  revenue: number;
}

export interface TopProduct {
  productId: string;
  productName: string;
  quantity: number;
  revenue: number;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsAdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/ordering/analytics';

  getSummary(filter: AnalyticsFilter) {
    const params = this.buildParams(filter);
    return this.http.get<SalesSummary>(`${this.baseUrl}/summary`, { params });
  }

  getSalesByDay(filter: AnalyticsFilter) {
    const params = this.buildParams(filter);
    return this.http.get<SalesByDay[]>(`${this.baseUrl}/by-day`, { params });
  }

  getTopProducts(filter: AnalyticsFilter) {
    const params = this.buildParams(filter);
    return this.http.get<TopProduct[]>(`${this.baseUrl}/top-products`, { params });
  }

  exportCsv(filter: AnalyticsFilter) {
    const params = this.buildParams(filter);
    return this.http.get(`${this.baseUrl}/export`, {
      params,
      responseType: 'blob',
    });
  }

  private buildParams(filter: AnalyticsFilter) {
    let params = new HttpParams();
    if (filter.dateFrom) {
      params = params.set('dateFrom', filter.dateFrom);
    }
    if (filter.dateTo) {
      params = params.set('dateTo', filter.dateTo);
    }
    return params;
  }
}

