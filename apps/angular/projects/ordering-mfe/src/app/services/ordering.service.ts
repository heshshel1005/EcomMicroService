import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

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

export interface CreateOrderItemDto {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface CreateOrderDto {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  orderItems: CreateOrderItemDto[];
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  orderDate: string;
  status: number;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  totalPrice: number;
  items: any[];
}

@Injectable({
  providedIn: 'root'
})
export class OrderingService {
  private gatewayUrl = 'https://localhost:7500';
  private basketSubject = new BehaviorSubject<Basket>({ items: [], totalPrice: 0 });
  public basket$ = this.basketSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadBasket();
  }

  // Retrieve anonymous ID
  private getAnonymousId(): string {
    let id = localStorage.getItem('ecom_anonymous_id');
    const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!id || !guidPattern.test(id)) {
      id = crypto.randomUUID();
      localStorage.setItem('ecom_anonymous_id', id);
    }
    return id;
  }

  loadBasket(): void {
    const anonId = this.getAnonymousId();
    this.http.get<any>(`${this.gatewayUrl}/api/basket/cart`, {
      params: new HttpParams().set('guestCartId', anonId)
    }).pipe(
      catchError(() => of({ items: [], itemCount: 0 }))
    ).subscribe(cart => {
      const items: BasketItem[] = (cart.items || []).map((i: any) => ({
        productId: i.productId,
        productName: i.productName,
        unitPrice: i.unitPrice || 0,
        quantity: i.quantity
      }));
      this.basketSubject.next({ items, totalPrice: items.reduce((s, i) => s + i.unitPrice * i.quantity, 0) });
      localStorage.setItem('ecom_basket_count', String(cart.itemCount || 0));
      window.dispatchEvent(new Event('storage'));
    });
  }

  // Update quantity in basket
  updateItemQuantity(productId: string, quantity: number): Observable<Basket> {
    const anonId = this.getAnonymousId();
    const currentBasket = this.basketSubject.value;
    
    let updatedItems = currentBasket.items
      .map(item => {
        if (item.productId === productId) {
          return { ...item, quantity };
        }
        return item;
      })
      .filter(item => item.quantity > 0);

    const payload: Basket = {
      items: updatedItems,
      totalPrice: updatedItems.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0)
    };

    return this.http.put<Basket>(`${this.gatewayUrl}/api/basket?anonymousId=${anonId}`, payload).pipe(
      tap(basket => {
        this.basketSubject.next(basket);
        localStorage.setItem('ecom_basket_count', basket.items.reduce((sum, item) => sum + item.quantity, 0).toString());
        window.dispatchEvent(new Event('storage'));
      }),
      catchError(err => {
        console.error('Failed to update basket quantity', err);
        return of(payload);
      })
    );
  }

  // Remove item from basket
  removeItem(productId: string): Observable<Basket> {
    return this.updateItemQuantity(productId, 0);
  }

  // Clear basket
  clearBasket(): Observable<any> {
    const anonId = this.getAnonymousId();
    return this.http.post(`${this.gatewayUrl}/api/basket/cart/clear`, null, {
      params: new HttpParams().set('guestCartId', anonId)
    }).pipe(
      tap(() => {
        this.basketSubject.next({ items: [], totalPrice: 0 });
        localStorage.setItem('ecom_basket_count', '0');
        window.dispatchEvent(new Event('storage'));
      }),
      catchError(err => {
        console.error('Failed to clear basket', err);
        return of(null);
      })
    );
  }

  // Checkout and create order
  createOrder(shippingAddress: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
  }): Observable<OrderDto | null> {
    const anonId = this.getAnonymousId();
    const payload = {
      contactEmail: 'guest@local.test',
      contactName: 'Guest',
      shippingAddress: {
        street: shippingAddress.street,
        city: shippingAddress.city,
        region: shippingAddress.state,
        postalCode: shippingAddress.zipCode,
        country: 'US',
      },
      billingSameAsShipping: true,
      shippingMethodCode: 'standard',
    };
    return this.http.post<any>(`${this.gatewayUrl}/api/ordering/checkout/submit`, payload, {
      params: new HttpParams().set('guestCartId', anonId)
    }).pipe(
      tap(() => this.clearBasket().subscribe()),
      map(res => ({
        id: res.orderId,
        orderNumber: res.orderId,
        orderDate: new Date().toISOString(),
        status: 0,
        street: shippingAddress.street,
        city: shippingAddress.city,
        state: shippingAddress.state,
        zipCode: shippingAddress.zipCode,
        totalPrice: 0,
        items: []
      } as OrderDto)),
      catchError(err => {
        console.error('Failed to create order', err);
        return of(null);
      })
    );
  }
}
