import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import { WishlistService, WishlistItemDto } from '../wishlist.service';
import { CartService } from '../cart.service';

@Component({
  selector: 'app-customer-wishlist',
  standalone: true,
  imports: [DecimalPipe, RouterLink, LocalizationPipe],
  template: `
    <div class="card">
      <div class="card-body">
        <h3 class="card-title">{{ 'ECommerce::CustomerWishlist' | abpLocalization }}</h3>
        <p class="text-muted mb-3">{{ 'ECommerce::CustomerWishlistDescription' | abpLocalization }}</p>

        @if (loading()) {
          <p class="text-muted">{{ 'ECommerce::Loading' | abpLocalization }}</p>
        } @else if (!wishlist()?.items?.length) {
          <p class="text-muted mb-0">{{ 'ECommerce::WishlistEmpty' | abpLocalization }}</p>
          <a routerLink="/catalog" class="btn btn-outline-primary mt-2">{{ 'ECommerce::Catalog' | abpLocalization }}</a>
        } @else {
          <ul class="list-group list-group-flush">
            @for (item of wishlist()!.items; track item.id) {
              <li class="list-group-item d-flex justify-content-between align-items-center flex-wrap gap-2">
                <div>
                  <a [routerLink]="['/catalog/product', item.productId]">{{ item.productName }}</a>
                  <span class="text-muted ms-2">({{ item.sku }})</span>
                  @if (item.price !== null) {
                    <span class="ms-2">{{ item.price | number:'1.2-2' }} {{ 'ECommerce::Price' | abpLocalization }}</span>
                  }
                  @if ((item.availableQuantity ?? 0) <= 0) {
                    <span class="badge bg-secondary ms-2">{{ 'ECommerce::OutOfStock' | abpLocalization }}</span>
                  }
                </div>
                <div class="d-flex gap-2">
                  <button
                    type="button"
                    class="btn btn-sm btn-primary"
                    [disabled]="(item.availableQuantity ?? 0) <= 0 || addingToCartId() === item.id"
                    (click)="addToCart(item)"
                  >
                    @if (addingToCartId() === item.id) {
                      <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                    } @else {
                      {{ 'ECommerce::AddToCartFromWishlist' | abpLocalization }}
                    }
                  </button>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-danger"
                    (click)="remove(item)"
                  >
                    {{ 'ECommerce::RemoveFromWishlist' | abpLocalization }}
                  </button>
                </div>
              </li>
            }
          </ul>
        }
      </div>
    </div>
  `,
})
export class CustomerWishlistComponent implements OnInit, OnDestroy {
  private readonly wishlistService = inject(WishlistService);
  private readonly cartService = inject(CartService);
  private readonly toaster = inject(ToasterService);
  private readonly breadcrumbService = inject(BreadcrumbService);

  wishlist = this.wishlistService.serverWishlist;
  loading = signal(true);
  addingToCartId = signal<string | null>(null);

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::CustomerDashboard', route: '/my-account' },
      { label: 'ECommerce::CustomerWishlist' },
    ]);
    this.loading.set(true);
    this.wishlistService.getList().subscribe({
      next: () => this.loading.set(false),
      error: () => this.loading.set(false),
    });
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  addToCart(item: WishlistItemDto) {
    if ((item.availableQuantity ?? 0) <= 0) return;
    this.addingToCartId.set(item.id);
    this.wishlistService.addToCart(item.id).subscribe({
      next: () => {
        this.addingToCartId.set(null);
        this.cartService.getCart().subscribe();
        this.toaster.success('ECommerce::AddToCart');
      },
      error: (err) => {
        this.addingToCartId.set(null);
        const msg = err?.error?.error?.message ?? err?.message ?? 'ECommerce::ErrorLoadingProducts';
        this.toaster.error(msg);
      },
    });
  }

  remove(item: WishlistItemDto) {
    this.wishlistService.removeItem(item.id).subscribe({
      next: () => this.toaster.success('ECommerce::RemoveFromWishlist'),
      error: () => this.toaster.error('ECommerce::ErrorLoadingProducts'),
    });
  }
}
