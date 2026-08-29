import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { OrderAdminService, OrderDto } from './order-admin.service';

const STATUS_OPTIONS = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] as const;

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, LocalizationPipe],
  templateUrl: './order-detail.component.html',
})
export class OrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderAdminService = inject(OrderAdminService);
  private readonly toaster = inject(ToasterService);

  order = signal<OrderDto | null>(null);
  loading = signal(true);
  savingStatus = signal(false);
  newStatus = signal('');

  statusOptions = STATUS_OPTIONS;
  statusLabel = (s: string) => (s ? `ECommerce::OrderStatus${s}` : '');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }
    this.orderAdminService.get(id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.newStatus.set(o.status);
        this.loading.set(false);
      },
      error: () => {
        this.order.set(null);
        this.loading.set(false);
      },
    });
  }

  updateStatus(): void {
    const o = this.order();
    const status = this.newStatus();
    if (!o || !status || status === o.status) return;
    this.savingStatus.set(true);
    this.orderAdminService.updateStatus(o.id, status).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.savingStatus.set(false);
        this.toaster.success('ECommerce::UpdateStatus', 'ECommerce::Orders');
      },
      error: () => {
        this.savingStatus.set(false);
        this.toaster.error('ECommerce::ErrorLoadingProducts', 'Error');
      },
    });
  }
}
