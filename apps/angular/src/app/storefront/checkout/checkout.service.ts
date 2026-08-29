import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { AuthService } from '@abp/ng.core';

export const GUEST_CART_ID_KEY = 'ecom_anonymous_id';

export interface CartItemDto {
  id: string;
  cartId: string;
  productVariantId: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice?: number | null;
  quantity: number;
  availableStock?: number | null;
}

export interface CartDto {
  id: string;
  isAuthenticated: boolean;
  items: CartItemDto[];
  itemCount: number;
}

export interface CheckoutAddressDto {
  street: string;
  street2?: string | null;
  city?: string | null;
  region?: string | null;
  postalCode?: string | null;
  country?: string | null;
  deliveryInstructions?: string | null;
}

export interface ShippingOptionDto {
  code: string;
  name: string;
  amount: number;
}

export interface CheckoutSummaryDto {
  cart: CartDto;
  subTotal: number;
  discountAmount: number;
  appliedCouponCode?: string | null;
  shippingOptions: ShippingOptionDto[];
  taxAmount: number;
  defaultShippingMethodCode?: string | null;
}

export interface SubmitCheckoutDto {
  contactEmail: string;
  contactPhone?: string | null;
  contactName?: string | null;
  shippingAddress: CheckoutAddressDto;
  billingSameAsShipping: boolean;
  billingAddress?: CheckoutAddressDto | null;
  shippingMethodCode: string;
  couponCode?: string | null;
}

export interface SubmitCheckoutResultDto {
  orderId: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly rest = inject(RestService);
  private readonly authService = inject(AuthService);

  /** Guest cart id for anonymous users (from sessionStorage). */
  getGuestCartId(): string | null {
    if (this.authService.isAuthenticated) return null;
    try {
      return localStorage.getItem(GUEST_CART_ID_KEY);
    } catch {
      return null;
    }
  }

  setGuestCartId(id: string | null): void {
    try {
      if (id) localStorage.setItem(GUEST_CART_ID_KEY, id);
      else localStorage.removeItem(GUEST_CART_ID_KEY);
    } catch {}
  }

  getSummary(guestCartId?: string | null, couponCode?: string | null): Observable<CheckoutSummaryDto> {
    const params: Record<string, string> = {};
    const gid = guestCartId ?? this.getGuestCartId();
    if (gid) params.guestCartId = gid;
    if (couponCode?.trim()) params.couponCode = couponCode.trim();
    return this.rest.request<void, CheckoutSummaryDto>({
      method: 'GET',
      url: '/api/ordering/checkout/summary',
      params: Object.keys(params).length ? params : undefined,
    });
  }

  submitOrder(body: SubmitCheckoutDto, guestCartId?: string | null): Observable<SubmitCheckoutResultDto> {
    const params: Record<string, string> = {};
    const gid = guestCartId ?? this.getGuestCartId();
    if (gid) params.guestCartId = gid;
    return this.rest.request<SubmitCheckoutDto, SubmitCheckoutResultDto>({
      method: 'POST',
      url: '/api/ordering/checkout/submit',
      body,
      params: Object.keys(params).length ? params : undefined,
    });
  }
}
