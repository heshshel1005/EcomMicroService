import { Component, inject, OnInit, computed, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { OrderAdminService, OrderListDto, OrderListRequestDto, PagedResultDto } from './order-admin.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, LocalizationPipe],
  templateUrl: './order-list.component.html',
})
export class OrderListComponent implements OnInit {
  private readonly orderAdminService = inject(OrderAdminService);

  items = signal<OrderListDto[]>([]);
  totalCount = signal(0);
  loading = signal(true);
  statusFilter = signal<string>('');
  page = signal(0);
  pageSize = 10;
  skipCount = computed(() => this.page() * this.pageSize);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const req: OrderListRequestDto = {
      skipCount: this.skipCount(),
      maxResultCount: this.pageSize,
      sorting: 'CreationTime DESC',
      status: this.statusFilter() || undefined,
    };
    this.orderAdminService.getList(req).subscribe({
      next: (res) => {
        const data = (res as unknown) as PagedResultDto<OrderListDto>;
        const raw = data?.items ?? (data as unknown as Record<string, unknown>)?.items;
        const list = Array.isArray(raw) ? raw : [];
        const total = data?.totalCount ?? (data as unknown as Record<string, number>)?.totalCount ?? 0;
        this.items.set(list);
        this.totalCount.set(total);
        this.loading.set(false);
      },
      error: () => {
        this.items.set([]);
        this.totalCount.set(0);
        this.loading.set(false);
      },
    });
  }

  onFilterChange(): void {
    this.page.set(0);
    this.load();
  }

  onPageChange(pageIndex: number): void {
    this.page.set(pageIndex);
    this.load();
  }

  statusLabel(status: string): string {
    return status ? `ECommerce::OrderStatus${status}` : '';
  }
}
