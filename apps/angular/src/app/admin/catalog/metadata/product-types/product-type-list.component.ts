import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { ProductTypeDto, ProductTypeService } from './product-type.service';

@Component({
  selector: 'app-product-type-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe],
  templateUrl: './product-type-list.component.html',
})
export class ProductTypeListComponent implements OnInit {
  private readonly service = inject(ProductTypeService);
  private readonly toaster = inject(ToasterService);

  items = signal<ProductTypeDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);
  showActiveOnly = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const filter = this.showActiveOnly() ? true : null;
    this.service.getList(filter).subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  toggleActiveOnly(value: boolean): void {
    this.showActiveOnly.set(value);
    this.load();
  }

  deleteItem(item: ProductTypeDto): void {
    if (!confirm('Delete this product type?')) {
      return;
    }

    this.deletingId.set(item.id);
    this.service.delete(item.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::Success', 'Success');
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }
}
