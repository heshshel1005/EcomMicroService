import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';

export interface Product {
  id: string;
  categoryId: string;
  brandId: string;
  modelId?: string;
  variantId?: string;
  name: string;
  sku: string;
  description: string;
  price: number;
  stockQuantity: number;
  imageUrl: string;
  isFeatured: boolean;
  isActive: boolean;
}

export interface Category {
  id: string;
  name: string;
  code: string;
  parentId?: string;
  isActive: boolean;
}

export interface Brand {
  id: string;
  name: string;
  logoUrl?: string;
  isActive: boolean;
}

export interface Model {
  id: string;
  brandId: string;
  name: string;
  releaseYear?: number;
}

export interface BasketItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  imageUrl?: string;
}

export interface Basket {
  items: BasketItem[];
  totalPrice: number;
}

interface PublicProductListDto {
  id: string;
  productNumber: string;
  name: string;
  categoryId?: string | null;
  priceFrom?: number | null;
  isInStock: boolean;
  primaryMediaId?: string | null;
  brandId: string;
  modelId?: string | null;
}

interface ProductDto {
  id: string;
  productNumber: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  brandId?: string;
  modelId?: string | null;
  primaryMediaId?: string | null;
  variants?: { id: string; sku: string; price?: number | null; availableQuantity: number }[];
}

interface CategoryTreeDto {
  id: string;
  parentId?: string | null;
  name: string;
  slug?: string | null;
  children: CategoryTreeDto[];
}

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private gatewayUrl = 'https://localhost:7500';
  private basketSubject = new BehaviorSubject<Basket>({ items: [], totalPrice: 0 });
  public basket$ = this.basketSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadBasket();
  }

  private getAnonymousId(): string {
    let id = localStorage.getItem('ecom_anonymous_id');
    const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!id || !guidPattern.test(id)) {
      id = crypto.randomUUID();
      localStorage.setItem('ecom_anonymous_id', id);
    }
    return id;
  }

  private mediaUrl(mediaId?: string | null): string {
    if (!mediaId) {
      return '';
    }
    return `${this.gatewayUrl}/api/catalog/product-media/${mediaId}/file`;
  }

  private mapListItem(p: PublicProductListDto): Product {
    return {
      id: p.id,
      categoryId: p.categoryId || '',
      brandId: p.brandId,
      modelId: p.modelId || undefined,
      name: p.name,
      sku: p.productNumber,
      description: '',
      price: p.priceFrom ?? 0,
      stockQuantity: p.isInStock ? 99 : 0,
      imageUrl: this.mediaUrl(p.primaryMediaId),
      isFeatured: false,
      isActive: true,
    };
  }

  getProducts(filters: {
    filter?: string;
    categoryId?: string;
    brandId?: string;
    modelId?: string;
    isFeatured?: boolean;
    skipCount?: number;
    maxResultCount?: number;
  }): Observable<{ items: Product[]; totalCount: number }> {
    let params = new HttpParams();
    if (filters.filter) params = params.set('Search', filters.filter);
    if (filters.categoryId) params = params.set('CategoryId', filters.categoryId);
    if (filters.brandId) params = params.set('BrandId', filters.brandId);
    if (filters.modelId) params = params.set('ModelId', filters.modelId);
    if (filters.skipCount !== undefined) params = params.set('SkipCount', filters.skipCount.toString());
    if (filters.maxResultCount !== undefined) params = params.set('MaxResultCount', filters.maxResultCount.toString());

    return this.http.get<{ items?: PublicProductListDto[]; totalCount?: number }>(
      `${this.gatewayUrl}/api/catalog/public-catalog/products`,
      { params }
    ).pipe(
      map(res => ({
        items: (res.items || []).map(p => this.mapListItem(p)),
        totalCount: res.totalCount || 0
      })),
      catchError(err => {
        console.error('Error fetching products', err);
        return of({ items: [], totalCount: 0 });
      })
    );
  }

  getProduct(id: string): Observable<Product | null> {
    return this.http.get<ProductDto>(`${this.gatewayUrl}/api/catalog/public-catalog/products/${id}`).pipe(
      map(p => {
        const variant = p.variants?.[0];
        return {
          id: p.id,
          categoryId: p.categoryId || '',
          brandId: p.brandId || '',
          modelId: p.modelId || undefined,
          variantId: variant?.id,
          name: p.name,
          sku: variant?.sku || p.productNumber,
          description: p.description || '',
          price: variant?.price ?? 0,
          stockQuantity: variant?.availableQuantity ?? 0,
          imageUrl: this.mediaUrl(p.primaryMediaId),
          isFeatured: false,
          isActive: true,
        } as Product;
      }),
      catchError(err => {
        console.error(`Error fetching product ${id}`, err);
        return of(null);
      })
    );
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<CategoryTreeDto[]>(`${this.gatewayUrl}/api/catalog/public-catalog/categories/tree`).pipe(
      map(tree => this.flattenCategories(tree)),
      catchError(err => {
        console.error('Error fetching categories', err);
        return of([]);
      })
    );
  }

  private flattenCategories(nodes: CategoryTreeDto[], acc: Category[] = []): Category[] {
    for (const n of nodes || []) {
      acc.push({ id: n.id, name: n.name, code: n.slug || '', parentId: n.parentId || undefined, isActive: true });
      if (n.children?.length) {
        this.flattenCategories(n.children, acc);
      }
    }
    return acc;
  }

  getBrands(): Observable<Brand[]> {
    return this.http.get<{ brands?: { id: string; name: string }[] }>(
      `${this.gatewayUrl}/api/catalog/public-catalog/filter-options`
    ).pipe(
      map(res => (res.brands || []).map(b => ({ id: b.id, name: b.name, isActive: true }))),
      catchError(err => {
        console.error('Error fetching brands', err);
        return of([]);
      })
    );
  }

  getModels(brandId?: string): Observable<Model[]> {
    return this.http.get<{ models?: { id: string; brandId: string; name: string }[] }>(
      `${this.gatewayUrl}/api/catalog/public-catalog/filter-options`
    ).pipe(
      map(res => (res.models || [])
        .filter(m => !brandId || m.brandId === brandId)
        .map(m => ({ id: m.id, brandId: m.brandId, name: m.name }))),
      catchError(err => {
        console.error('Error fetching models', err);
        return of([]);
      })
    );
  }

  loadBasket(): void {
    const anonId = this.getAnonymousId();
    this.http.get<any>(`${this.gatewayUrl}/api/basket/cart`, {
      params: new HttpParams().set('guestCartId', anonId)
    }).pipe(
      catchError(err => {
        console.warn('Basket fetch failed, initializing empty basket.', err);
        return of({ items: [], itemCount: 0 });
      })
    ).subscribe((cart) => {
      const items: BasketItem[] = (cart.items || []).map((i: any) => ({
        productId: i.productId,
        productName: i.productName,
        unitPrice: i.unitPrice || 0,
        quantity: i.quantity
      }));
      const basket: Basket = { items, totalPrice: items.reduce((s, i) => s + i.unitPrice * i.quantity, 0) };
      this.basketSubject.next(basket);
      localStorage.setItem('ecom_basket_count', String(cart.itemCount || 0));
      window.dispatchEvent(new Event('storage'));
    });
  }

  addToCart(product: Product, quantity: number = 1): Observable<Basket> {
    const anonId = this.getAnonymousId();
    const variantId = product.variantId;
    if (variantId) {
      return this.http.post<any>(`${this.gatewayUrl}/api/basket/cart/items`, {
        productVariantId: variantId,
        quantity
      }, { params: new HttpParams().set('guestCartId', anonId) }).pipe(
        tap((cart) => {
          const items: BasketItem[] = (cart.items || []).map((i: any) => ({
            productId: i.productId,
            productName: i.productName,
            unitPrice: i.unitPrice || 0,
            quantity: i.quantity
          }));
          const basket: Basket = { items, totalPrice: items.reduce((s, i) => s + i.unitPrice * i.quantity, 0) };
          this.basketSubject.next(basket);
          localStorage.setItem('ecom_basket_count', String(cart.itemCount || items.reduce((s, i) => s + i.quantity, 0)));
          window.dispatchEvent(new Event('storage'));
        }),
        catchError(err => {
          console.error('Failed to update basket', err);
          return of(this.basketSubject.value);
        })
      );
    }

    const currentBasket = this.basketSubject.value;
    const existingItem = currentBasket.items.find(item => item.productId === product.id);

    let updatedItems = [...currentBasket.items];
    if (existingItem) {
      existingItem.quantity += quantity;
    } else {
      updatedItems.push({
        productId: product.id,
        productName: product.name,
        unitPrice: product.price,
        quantity: quantity,
        imageUrl: product.imageUrl
      });
    }

    const payload: Basket = {
      items: updatedItems,
      totalPrice: updatedItems.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
    };

    return this.http.put<Basket>(`${this.gatewayUrl}/api/basket?anonymousId=${anonId}`, payload).pipe(
      tap((basket: Basket) => {
        this.basketSubject.next(basket);
        localStorage.setItem('ecom_basket_count', basket.items.reduce((sum: number, item: BasketItem) => sum + item.quantity, 0).toString());
        window.dispatchEvent(new Event('storage'));
      }),
      catchError(err => {
        console.error('Failed to update basket', err);
        return of(payload);
      })
    );
  }
}
