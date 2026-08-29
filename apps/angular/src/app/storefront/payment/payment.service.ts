import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PaymentGatewayDto {
  name: string;
  displayName: string;
  publishableKeyOrClientId?: string | null;
}

export interface CreatePaymentIntentResult {
  success: boolean;
  errorCode?: string | null;
  errorMessage?: string | null;
  clientSecret?: string | null;
  gatewayPaymentId?: string | null;
  publishableKeyOrClientId?: string | null;
}

export interface ConfirmPaymentResult {
  success: boolean;
  errorCode?: string | null;
  errorMessage?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly rest = inject(RestService);

  getGateways(): Observable<PaymentGatewayDto[]> {
    return this.rest.request<void, unknown>({
      method: 'GET',
      url: '/api/payment/gateways',
    }).pipe(
      map((res) => this.normalizeGateways(res))
    );
  }

  createIntent(orderId: string, gatewayName: string): Observable<CreatePaymentIntentResult> {
    return this.rest.request<{ orderId: string; gatewayName: string }, unknown>({
      method: 'POST',
      url: '/api/payment/create-intent',
      body: { orderId, gatewayName },
    }).pipe(
      map((res) => this.normalizeCreateIntentResult(res))
    );
  }

  confirm(orderId: string, gatewayPaymentId: string): Observable<ConfirmPaymentResult> {
    return this.rest.request<{ orderId: string; gatewayPaymentId: string }, unknown>({
      method: 'POST',
      url: '/api/payment/confirm',
      body: { orderId, gatewayPaymentId },
    }).pipe(
      map((res) => this.normalizeConfirmResult(res))
    );
  }

  private normalizeGateways(res: unknown): PaymentGatewayDto[] {
    const raw = res != null && typeof res === 'object' ? (res as Record<string, unknown>) : {};
    const arr = (raw.result ?? raw.body ?? raw) as unknown[];
    if (!Array.isArray(arr)) return [];
    return arr.map((item) => {
      const o = (item != null && typeof item === 'object' ? item : {}) as Record<string, unknown>;
      return {
        name: String(o.name ?? o.Name ?? ''),
        displayName: String(o.displayName ?? o.DisplayName ?? o.name ?? o.Name ?? ''),
        publishableKeyOrClientId: (o.publishableKeyOrClientId ?? o.PublishableKeyOrClientId) as string | null | undefined,
      };
    });
  }

  private normalizeCreateIntentResult(res: unknown): CreatePaymentIntentResult {
    const raw = res != null && typeof res === 'object' ? (res as Record<string, unknown>) : {};
    const data = raw.result ?? raw.body ?? raw;
    const o = (data != null && typeof data === 'object' ? data : {}) as Record<string, unknown>;
    return {
      success: Boolean(o.success ?? o.Success),
      errorCode: (o.errorCode ?? o.ErrorCode) as string | null | undefined,
      errorMessage: (o.errorMessage ?? o.ErrorMessage) as string | null | undefined,
      clientSecret: (o.clientSecret ?? o.ClientSecret) as string | null | undefined,
      gatewayPaymentId: (o.gatewayPaymentId ?? o.GatewayPaymentId) as string | null | undefined,
      publishableKeyOrClientId: (o.publishableKeyOrClientId ?? o.PublishableKeyOrClientId) as string | null | undefined,
    };
  }

  private normalizeConfirmResult(res: unknown): ConfirmPaymentResult {
    const raw = res != null && typeof res === 'object' ? (res as Record<string, unknown>) : {};
    const data = raw.result ?? raw.body ?? raw;
    const o = (data != null && typeof data === 'object' ? data : {}) as Record<string, unknown>;
    return {
      success: Boolean(o.success ?? o.Success),
      errorCode: (o.errorCode ?? o.ErrorCode) as string | null | undefined,
      errorMessage: (o.errorMessage ?? o.ErrorMessage) as string | null | undefined,
    };
  }
}
