import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BrandService, BrandDto } from './brand.service';

@Component({
  selector: 'app-brand-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe],
  templateUrl: './brand-list.component.html',
  styleUrls: ['./brand-list.component.scss'],
})
export class BrandListComponent implements OnInit {
  private readonly brandService = inject(BrandService);
  private readonly toaster = inject(ToasterService);

  items = signal<BrandDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);
  showActiveOnly = signal<boolean>(true);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const isActive = this.showActiveOnly() ? true : null;
    this.brandService.getList(isActive).subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toaster.error(
          err?.error?.error?.message || 'ECommerce::Error',
          'Error',
        );
      },
    });
  }

  toggleActiveOnly(value: boolean): void {
    this.showActiveOnly.set(value);
    this.load();
  }

  deleteBrand(item: BrandDto): void {
    if (!confirm(this.getConfirmDeleteText())) {
      return;
    }
    this.deletingId.set(item.id);
    this.brandService.delete(item.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::BrandDeleted', 'Success');
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toaster.error(
          err?.error?.error?.message || 'ECommerce::Error',
          'Error',
        );
      },
    });
  }

  private getConfirmDeleteText(): string {
    try {
      const lang = (document.documentElement.lang || 'en').startsWith('ar')
        ? 'ar'
        : 'en';
      if (lang === 'ar') {
        return 'هل أنت متأكد من حذف هذه العلامة التجارية؟';
      }
      return 'Are you sure you want to delete this brand?';
    } catch {
      return 'Are you sure you want to delete this brand?';
    }
  }
}

