import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CatalogService, Product } from '../../services/catalog.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss'
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  quantity = 1;
  successMessage: string | null = null;
  toastTimeout: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private catalogService: CatalogService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
    } else {
      this.router.navigate(['/catalog']);
    }
  }

  loadProduct(id: string): void {
    this.loading = true;
    this.catalogService.getProduct(id).subscribe({
      next: (prod) => {
        this.product = prod;
        this.loading = false;
        if (!this.product) {
          this.router.navigate(['/catalog']);
        }
      },
      error: () => {
        this.loading = false;
        this.router.navigate(['/catalog']);
      }
    });
  }

  incrementQuantity(): void {
    if (this.product && this.quantity < this.product.stockQuantity) {
      this.quantity++;
    }
  }

  decrementQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  addToCart(): void {
    if (!this.product) return;
    this.catalogService.addToCart(this.product, this.quantity).subscribe(() => {
      this.showToast(`${this.quantity}x ${this.product?.name} added to cart!`);
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
}
