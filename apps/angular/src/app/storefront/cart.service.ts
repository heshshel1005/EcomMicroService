import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { GUEST_CART_ID_KEY } from './checkout/checkout.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly rest = inject(RestService);

  getCart(): Observable<unknown> {
    const guestCartId = localStorage.getItem(GUEST_CART_ID_KEY) ?? '';
    return this.rest
      .request<void, { itemCount?: number }>({
        method: 'GET',
        url: '/api/basket/cart',
        params: { guestCartId },
      })
      .pipe(
        tap((cart) => {
          localStorage.setItem('ecom_basket_count', String(cart?.itemCount ?? 0));
          window.dispatchEvent(new Event('storage'));
        }),
        catchError(() => of(null))
      );
  }
}
