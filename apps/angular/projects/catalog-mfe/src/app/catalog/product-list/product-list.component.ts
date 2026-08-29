import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CatalogService, Product, Category, Brand, Model } from '../../services/catalog.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  brands: Brand[] = [];
  models: Model[] = [];
  
  totalCount = 0;
  loading = false;
  successMessage: string | null = null;
  toastTimeout: any;

  // Filters
  searchTerm = '';
  selectedCategory: string | null = null;
  selectedBrand: string | null = null;
  selectedModel: string | null = null;

  constructor(
    private catalogService: CatalogService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFilters();
    this.loadProducts();
  }

  loadFilters(): void {
    this.catalogService.getCategories().subscribe(cats => this.categories = cats);
    this.catalogService.getBrands().subscribe(brs => this.brands = brs);
  }

  onBrandChange(): void {
    this.selectedModel = null;
    this.models = [];
    if (this.selectedBrand) {
      this.catalogService.getModels(this.selectedBrand).subscribe(mds => this.models = mds);
    }
    this.applyFilters();
  }

  applyFilters(): void {
    this.loadProducts();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = null;
    this.selectedBrand = null;
    this.selectedModel = null;
    this.models = [];
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.catalogService.getProducts({
      filter: this.searchTerm || undefined,
      categoryId: this.selectedCategory || undefined,
      brandId: this.selectedBrand || undefined,
      modelId: this.selectedModel || undefined,
      maxResultCount: 20
    }).subscribe({
      next: (res) => {
        this.products = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  addToCart(product: Product, event: MouseEvent): void {
    event.stopPropagation(); // prevent card click navigation
    this.catalogService.addToCart(product, 1).subscribe(() => {
      this.showToast(`${product.name} added to cart!`);
    });
  }

  showToast(message: string): void {
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
    this.successMessage = message;
    this.toastTimeout = setTimeout(() => {
      this.successMessage = null;
    }, 3000);
  }

  viewProductDetails(productId: string): void {
    this.router.navigate(['/catalog/product', productId]);
  }
}
