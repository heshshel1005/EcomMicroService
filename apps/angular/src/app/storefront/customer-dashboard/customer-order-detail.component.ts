import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import { OrderService, OrderDto } from './order.service';

@Component({
  selector: 'app-customer-order-detail',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, LocalizationPipe],
  templateUrl: './customer-order-detail.component.html',
})
export class CustomerOrderDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly breadcrumbService = inject(BreadcrumbService);

  order = signal<OrderDto | null>(null);
  loading = signal(true);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }
    this.breadcrumbService.setItems([
      { label: 'ECommerce::CustomerDashboard', route: '/my-account' },
      { label: 'ECommerce::CustomerOrderHistory', route: '/my-account/orders' },
      { label: 'ECommerce::OrderDetail' },
    ]);
    this.orderService.get(id).subscribe({
      next: (o) => {
        this.order.set(o ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.order.set(null);
        this.loading.set(false);
      },
    });
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  statusLabel(status: string): string {
    return status ? `ECommerce::OrderStatus${status}` : '';
  }
}
