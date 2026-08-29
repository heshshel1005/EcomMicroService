import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BrandService, BrandDto } from '../brands/brand.service';
import { BrandModelService, BrandModelDto } from './brand-model.service';

@Component({
  selector: 'app-brand-model-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe],
  templateUrl: './brand-model-list.component.html',
  styleUrls: ['./brand-model-list.component.scss'],
})
export class BrandModelListComponent implements OnInit {
  private readonly brandService = inject(BrandService);
  private readonly brandModelService = inject(BrandModelService);
  private readonly toaster = inject(ToasterService);

  brands = signal<BrandDto[]>([]);
  items = signal<BrandModelDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);
  showActiveOnly = signal<boolean>(true);
  brandId = signal<string | null>(null);

  ngOnInit(): void {
    this.brandService.getList(true).subscribe({
      next: (list) => this.brands.set(list),
      error: () => {},
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const brandId = this.brandId();
    const isActive = this.showActiveOnly() ? true : null;
    this.brandModelService.getList(brandId, isActive).subscribe({
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

  onBrandChange(value: string): void {
    this.brandId.set(value || null);
    this.load();
  }

  toggleActiveOnly(value: boolean): void {
    this.showActiveOnly.set(value);
    this.load();
  }

  getBrandName(brandId: string): string {
    const brand = this.brands().find((b) => b.id === brandId);
    return brand?.name ?? '';
  }

  deleteModel(item: BrandModelDto): void {
    if (!confirm(this.getConfirmDeleteText())) {
      return;
    }
    this.deletingId.set(item.id);
    this.brandModelService.delete(item.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::ModelDeleted', 'Success');
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
        return 'هل أنت متأكد من حذف هذا الموديل؟';
      }
      return 'Are you sure you want to delete this model?';
    } catch {
      return 'Are you sure you want to delete this model?';
    }
  }
}

