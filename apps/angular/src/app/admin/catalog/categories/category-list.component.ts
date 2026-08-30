import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize, timeout } from 'rxjs/operators';
import { CategoryService, CategoryTreeDto } from './category.service';
import { CategoryTreeComponent } from './category-tree.component';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe, CategoryTreeComponent],
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss'],
})
export class CategoryListComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly toaster = inject(ToasterService);

  tree = signal<CategoryTreeDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadTree();
  }

  loadTree(): void {
    this.loading.set(true);
    this.categoryService
      .getTree()
      .pipe(
        timeout(20000),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => this.tree.set(Array.isArray(data) ? data : []),
        error: (err) => {
          this.tree.set([]);
          this.toaster.error(
            err?.error?.error?.message || 'ECommerce::ErrorLoadingCategories',
            'Error',
          );
        },
      });
  }

  deleteCategory(node: CategoryTreeDto): void {
    if (node.children?.length) {
      this.toaster.error('ECommerce::CategoryHasChildren', 'Error');
      return;
    }
    if (!confirm(this.getLocalizedConfirmDelete())) return;
    this.deletingId.set(node.id);
    this.categoryService.delete(node.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::CategoryDeleted', 'Success');
        this.loadTree();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
      },
    });
  }

  private getLocalizedConfirmDelete(): string {
    try {
      const lang = (document.documentElement.lang || 'en').startsWith('ar') ? 'ar' : 'en';
      if (lang === 'ar') return 'هل أنت متأكد من حذف هذه الفئة؟';
      return 'Are you sure you want to delete this category?';
    } catch {
      return 'Are you sure you want to delete this category?';
    }
  }
}
