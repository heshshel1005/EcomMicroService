import { Injectable, inject, signal, computed } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { AuthService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

const WISHLIST_STORAGE_KEY = 'ecommerce_wishlist_product_ids';

export interface WishlistItemDto {
  id: string;
  productVariantId: string;
  productId: string;
  productName: string;
  sku: string;
  price?: number | null;
  availableQuantity?: number | null;
}

export interface WishlistDto {
  id: string;
  items: WishlistItemDto[];
}

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly rest = inject(RestService);
  private readonly authService = inject(AuthService);

  /** Client-only wishlist (product IDs) for guests, from localStorage. */
  private readonly localIds = signal<Set<string>>(this.loadFromStorage());

  /** Server wishlist for authenticated users. */
  readonly serverWishlist = signal<WishlistDto | null>(null);

  wishlistIds = computed(() => {
    const server = this.serverWishlist();
    if (this.authService.isAuthenticated && server?.items?.length) {
      return new Set(server.items.map((i) => i.productId));
    }
    return this.localIds();
  });

  isInWishlist(productId: string): boolean {
    return this.wishlistIds().has(productId);
  }

  add(productId: string): void {
    this.localIds.update((set) => {
      const next = new Set(set);
      next.add(productId);
      this.saveToStorage(next);
      return next;
    });
  }

  /** Add by product variant ID (for API). When authenticated, call addItemAsync and refresh. */
  addVariant(productVariantId: string): Observable<WishlistDto> {
    return this.rest
      .request<void, WishlistDto>({
        method: 'POST',
        url: '/api/marketing/wishlist/items',
        params: { productVariantId },
      })
      .pipe(
        tap((w) => this.serverWishlist.set(this.normalizeWishlist(w)))
      );
  }

  remove(productId: string): void {
    this.localIds.update((set) => {
      const next = new Set(set);
      next.delete(productId);
      this.saveToStorage(next);
      return next;
    });
  }

  removeItem(wishlistItemId: string): Observable<WishlistDto> {
    return this.rest
      .request<void, WishlistDto>({
        method: 'DELETE',
        url: `/api/marketing/wishlist/items/${wishlistItemId}`,
      })
      .pipe(
        tap((w) => this.serverWishlist.set(this.normalizeWishlist(w)))
      );
  }

  toggle(productId: string): boolean {
    if (this.isInWishlist(productId)) {
      this.remove(productId);
      return false;
    }
    this.add(productId);
    return true;
  }

  getList(): Observable<WishlistDto> {
    return this.rest
      .request<void, WishlistDto>({
        method: 'GET',
        url: '/api/marketing/wishlist',
      })
      .pipe(
        tap((w) => this.serverWishlist.set(this.normalizeWishlist(w)))
      );
  }

  addToCart(wishlistItemId: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'POST',
      url: `/api/marketing/wishlist/items/${wishlistItemId}/add-to-cart`,
    });
  }

  getProductIds(): string[] {
    return Array.from(this.wishlistIds());
  }

  private loadFromStorage(): Set<string> {
    try {
      const raw = localStorage.getItem(WISHLIST_STORAGE_KEY);
      if (!raw) return new Set();
      const arr = JSON.parse(raw) as unknown;
      return new Set(Array.isArray(arr) ? arr.filter((x): x is string => typeof x === 'string') : []);
    } catch {
      return new Set();
    }
  }

  private saveToStorage(set: Set<string>): void {
    try {
      localStorage.setItem(WISHLIST_STORAGE_KEY, JSON.stringify(Array.from(set)));
    } catch {
      // ignore
    }
  }

  private normalizeWishlist(res: unknown): WishlistDto {
    const o = (res != null && typeof res === 'object' ? res : {}) as Record<string, unknown>;
    const items = ((o.items ?? o.Items) ?? []) as unknown[];
    return {
      id: (o.id ?? o.Id) as string,
      items: items.map((it) => {
        const i = (it != null && typeof it === 'object' ? it : {}) as Record<string, unknown>;
        return {
          id: (i.id ?? i.Id) as string,
          productVariantId: (i.productVariantId ?? i.ProductVariantId) as string,
          productId: (i.productId ?? i.ProductId) as string,
          productName: (i.productName ?? i.ProductName) as string ?? '',
          sku: (i.sku ?? i.Sku) as string ?? '',
          price: (i.price ?? i.Price) as number | null,
          availableQuantity: (i.availableQuantity ?? i.AvailableQuantity) as number | null,
        };
      }),
    };
  }
}
