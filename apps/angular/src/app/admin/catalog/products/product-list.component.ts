import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { ProductService, ProductListDto, ProductListRequestDto } from './product.service';
import { CategoryService, CategoryDto } from '../categories/category.service';
import { BrandService, BrandDto } from '../brands/brand.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [DecimalPipe, RouterLink, LocalizationPipe],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly brandService = inject(BrandService);
  private readonly toaster = inject(ToasterService);

  items = signal<ProductListDto[]>([]);
  totalCount = signal(0);
  loading = signal(true);
  deletingId = signal<string | null>(null);
  categories = signal<CategoryDto[]>([]);
  brands = signal<BrandDto[]>([]);
  filter = signal('');
  categoryId = signal<string | null>(null);
  brandId = signal<string | null>(null);
  page = signal(0);
  pageSize = 10;
  maxResultCount = 10;
  skipCount = computed(() => this.page() * this.pageSize);

  ngOnInit(): void {
    this.categoryService.getList().subscribe({
      next: (list) => this.categories.set(list),
      error: () => {},
    });
    this.brandService.getList(true).subscribe({
      next: (list) => this.brands.set(list),
      error: () => {},
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const req: ProductListRequestDto = {
      skipCount: this.skipCount(),
      maxResultCount: this.maxResultCount,
      sorting: 'Name',
      filter: this.filter() || undefined,
      categoryId: this.categoryId() || undefined,
      brandId: this.brandId() || undefined,
    };
    this.productService.getList(req).subscribe({
      next: (res) => {
        // Unwrap if wrapped (e.g. res.result or res.body) and support camelCase or PascalCase
        const raw = res != null && typeof res === 'object' ? (res as unknown) as Record<string, unknown> : {};
        const data = raw.result ?? raw.body ?? raw;
        const obj = data != null && typeof data === 'object' ? (data as unknown) as Record<string, unknown> : {};
        const rawItems = obj.items ?? obj.Items;
        const totalCount = obj.totalCount ?? obj.TotalCount;
        const list = Array.isArray(rawItems) ? rawItems.map((item: Record<string, unknown>) => ({
          id: item.id ?? item.Id,
          productNumber: item.productNumber ?? item.ProductNumber,
          name: item.name ?? item.Name,
          categoryId: item.categoryId ?? item.CategoryId,
          categoryName: item.categoryName ?? item.CategoryName,
          brandId: item.brandId ?? item.BrandId,
          brandName: item.brandName ?? item.BrandName,
          modelId: item.modelId ?? item.ModelId,
          modelName: item.modelName ?? item.ModelName,
          productTypeId: item.productTypeId ?? item.ProductTypeId,
          productTypeName: item.productTypeName ?? item.ProductTypeName,
          requiredAttributeCount: item.requiredAttributeCount ?? item.RequiredAttributeCount,
          filledRequiredAttributeCount: item.filledRequiredAttributeCount ?? item.FilledRequiredAttributeCount,
          isAttributeComplete: item.isAttributeComplete ?? item.IsAttributeComplete,
          isPublished: item.isPublished ?? item.IsPublished,
          priceFrom: item.priceFrom ?? item.PriceFrom,
        })) : [];
        this.items.set(list as ProductListDto[]);
        this.totalCount.set(typeof totalCount === 'number' ? totalCount : 0);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toaster.error('ECommerce::ErrorLoadingProducts', 'Error');
      },
    });
  }

  onFilterChange(): void {
    this.page.set(0);
    this.load();
  }

  onPageChange(page: number): void {
    this.page.set(page);
    this.load();
  }

  deleteProduct(item: ProductListDto): void {
    if (!confirm(this.getLocalizedConfirmDelete())) return;
    this.deletingId.set(item.id);
    this.productService.delete(item.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::ProductDeleted', 'Success');
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
      },
    });
  }

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  getCompletenessLabel(item: ProductListDto): string {
    if (!item.productTypeId) {
      return '—';
    }

    const required = item.requiredAttributeCount ?? 0;
    const filled = item.filledRequiredAttributeCount ?? 0;
    if (required === 0) {
      return 'Complete';
    }

    return `${filled}/${required}`;
  }

  getCompletenessClass(item: ProductListDto): string {
    if (!item.productTypeId) {
      return 'bg-secondary-subtle text-secondary';
    }

    return item.isAttributeComplete ? 'bg-success-subtle text-success' : 'bg-warning-subtle text-warning-emphasis';
  }

  private getLocalizedConfirmDelete(): string {
    try {
      const lang = (document.documentElement.lang || 'en').startsWith('ar') ? 'ar' : 'en';
      if (lang === 'ar') return 'هل أنت متأكد من حذف هذا المنتج؟';
      return 'Are you sure you want to delete this product?';
    } catch {
      return 'Are you sure you want to delete this product?';
    }
  }
}
