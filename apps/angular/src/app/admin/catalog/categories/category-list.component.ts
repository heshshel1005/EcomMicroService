import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
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

  tree: CategoryTreeDto[] = [];
  loading = true;
  deletingId: string | null = null;

  ngOnInit(): void {
    this.loadTree();
  }

  loadTree(): void {
    this.loading = true;
    this.categoryService.getTree().subscribe({
      next: (data) => {
        this.tree = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toaster.error('ECommerce::ErrorLoadingCategories', 'Error');
      },
    });
  }

  deleteCategory(node: CategoryTreeDto): void {
    if (node.children?.length) {
      this.toaster.error('ECommerce::CategoryHasChildren', 'Error');
      return;
    }
    if (!confirm(this.getLocalizedConfirmDelete())) return;
    this.deletingId = node.id;
    this.categoryService.delete(node.id).subscribe({
      next: () => {
        this.deletingId = null;
        this.toaster.success('ECommerce::CategoryDeleted', 'Success');
        this.loadTree();
      },
      error: (err) => {
        this.deletingId = null;
        this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
      },
    });
  }

  private getLocalizedConfirmDelete(): string {
    try {
      const key = 'ECommerce::ConfirmDeleteCategory';
      const lang = (document.documentElement.lang || 'en').startsWith('ar') ? 'ar' : 'en';
      if (lang === 'ar') return 'هل أنت متأكد من حذف هذه الفئة؟';
      return 'Are you sure you want to delete this category?';
    } catch {
      return 'Are you sure you want to delete this category?';
    }
  }
}
