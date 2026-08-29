import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  ProductReviewAdminService,
  ProductReviewDto,
  ProductReviewListRequestDto,
  ReviewStatus,
} from './product-review-admin.service';

@Component({
  selector: 'app-review-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe, SlicePipe],
  templateUrl: './review-list.component.html',
  styleUrls: ['./review-list.component.scss'],
})
export class ReviewListComponent implements OnInit {
  private readonly reviewService = inject(ProductReviewAdminService);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);

  items = signal<ProductReviewDto[]>([]);
  totalCount = signal(0);
  loading = signal(true);
  actionId = signal<string | null>(null);
  statusFilter = signal<number | null>(null);
  page = signal(0);
  pageSize = 20;
  readonly ReviewStatus = ReviewStatus;

  skipCount = computed(() => this.page() * this.pageSize);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  statusLabel = (status: number): string => {
    switch (status) {
      case ReviewStatus.Pending:
        return 'ECommerce::ReviewStatusPending';
      case ReviewStatus.Approved:
        return 'ECommerce::ReviewStatusApproved';
      case ReviewStatus.Rejected:
        return 'ECommerce::ReviewStatusRejected';
      default:
        return 'ECommerce::ReviewStatusPending';
    }
  };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const req: ProductReviewListRequestDto = {
      skipCount: this.skipCount(),
      maxResultCount: this.pageSize,
      sorting: 'CreationTime DESC',
      status: this.statusFilter() ?? undefined,
    };
    this.reviewService.getList(req).subscribe({
      next: (res) => {
        const raw = (res as unknown) as Record<string, unknown>;
        const data = raw?.result ?? raw?.body ?? raw;
        const obj = (data && typeof data === 'object' ? data : {}) as Record<string, unknown>;
        const rawItems = obj.items ?? obj.Items;
        const list = Array.isArray(rawItems)
          ? (rawItems as Record<string, unknown>[]).map((item) => ({
              id: (item.id ?? item.Id) as string,
              productId: (item.productId ?? item.ProductId) as string,
              userId: (item.userId ?? item.UserId) as string,
              authorDisplayName: (item.authorDisplayName ?? item.AuthorDisplayName) as string,
              rating: (item.rating ?? item.Rating) as number,
              reviewText: (item.reviewText ?? item.ReviewText) as string | null,
              status: (item.status ?? item.Status) as number,
              creationTime: (item.creationTime ?? item.CreationTime) as string,
            }))
          : [];
        this.items.set(list);
        this.totalCount.set((obj.totalCount ?? obj.TotalCount) as number ?? 0);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toaster.error('ECommerce::ErrorLoadingProducts', 'Error');
      },
    });
  }

  onStatusFilterChange(): void {
    this.page.set(0);
    this.load();
  }

  onPageChange(p: number): void {
    this.page.set(p);
    this.load();
  }

  approve(item: ProductReviewDto): void {
    this.actionId.set(item.id);
    this.reviewService.approve(item.id).subscribe({
      next: () => {
        this.toaster.success('ECommerce::ReviewApproved', 'Success');
        this.actionId.set(null);
        this.load();
      },
      error: () => this.actionId.set(null),
    });
  }

  reject(item: ProductReviewDto): void {
    if (!confirm(this.localization.instant('ECommerce::ConfirmRejectReview') || 'Reject this review?')) return;
    this.actionId.set(item.id);
    this.reviewService.reject(item.id).subscribe({
      next: () => {
        this.toaster.success('ECommerce::ReviewRejected', 'Success');
        this.actionId.set(null);
        this.load();
      },
      error: () => this.actionId.set(null),
    });
  }

  deleteReview(item: ProductReviewDto): void {
    if (!confirm(this.localization.instant('ECommerce::ConfirmDeleteReview') || 'Delete this review?')) return;
    this.actionId.set(item.id);
    this.reviewService.delete(item.id).subscribe({
      next: () => {
        this.toaster.success('ECommerce::ReviewDeleted', 'Success');
        this.actionId.set(null);
        this.load();
      },
      error: () => this.actionId.set(null),
    });
  }

  formatDate(creationTime: string): string {
    if (!creationTime) return '';
    return new Date(creationTime).toLocaleString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
