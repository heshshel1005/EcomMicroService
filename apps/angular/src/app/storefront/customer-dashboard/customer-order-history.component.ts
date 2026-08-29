import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import { OrderService, OrderDto } from './order.service';

@Component({
  selector: 'app-customer-order-history',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, LocalizationPipe],
  template: `
    <div class="card">
      <div class="card-body">
        <h3 class="card-title">{{ 'ECommerce::CustomerOrderHistory' | abpLocalization }}</h3>
        <p class="text-muted mb-0">{{ 'ECommerce::CustomerOrderHistoryDescription' | abpLocalization }}</p>

        @if (loading()) {
          <p class="mt-3 mb-0 text-muted">{{ 'ECommerce::Loading' | abpLocalization }}</p>
        } @else if (orders().length === 0) {
          <p class="mt-3 mb-0">{{ 'ECommerce::NoOrdersYet' | abpLocalization }}</p>
        } @else {
          <div class="table-responsive mt-3">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>{{ 'ECommerce::OrderDate' | abpLocalization }}</th>
                  <th>{{ 'ECommerce::OrderStatus' | abpLocalization }}</th>
                  <th class="text-end">{{ 'ECommerce::OrderTotal' | abpLocalization }}</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                @for (order of orders(); track order.id) {
                  <tr>
                    <td>{{ order.creationTime | date:'medium' }}</td>
                    <td>{{ statusLabel(order.status) | abpLocalization }}</td>
                    <td class="text-end">{{ order.total | number:'1.2-2' }}</td>
                    <td>
                      <a [routerLink]="['/my-account/orders', order.id]" class="btn btn-outline-primary btn-sm">{{ 'ECommerce::ViewOrder' | abpLocalization }}</a>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    </div>
  `,
})
export class CustomerOrderHistoryComponent implements OnInit, OnDestroy {
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly orderService = inject(OrderService);

  orders = signal<OrderDto[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::CustomerDashboard', route: '/my-account' },
      { label: 'ECommerce::CustomerOrderHistory' },
    ]);
    this.orderService.getMyOrders().subscribe({
      next: (list) => {
        this.orders.set(list ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.orders.set([]);
        this.loading.set(false);
      },
    });
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  statusLabel(status: string): string {
    const key = 'ECommerce::OrderStatus' + status;
    return key;
  }
}