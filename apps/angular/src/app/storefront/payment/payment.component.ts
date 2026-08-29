import { Component, inject, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { AuthService } from '@abp/ng.core';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import { OrderService, OrderDto } from '../customer-dashboard/order.service';
import {
  PaymentService,
  PaymentGatewayDto,
  CreatePaymentIntentResult,
} from './payment.service';

const PAYMOB_ORDER_ID_KEY = 'paymobPendingOrderId';

declare global {
  interface Window {
    paypal?: {
      Buttons: (config: PaypalButtonConfig) => { render: (selector: string) => Promise<void> };
    };
  }
}

interface PaypalButtonConfig {
  createOrder?: () => Promise<string>;
  onApprove?: (data: { orderID: string }) => Promise<void>;
  style?: { layout: string; color?: string };
}

const CASH_ON_DELIVERY_NAME = 'CashOnDelivery';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [DecimalPipe, RouterLink, LocalizationPipe],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss'],
})
export class PaymentComponent implements OnInit, OnDestroy {
  @ViewChild('paypalContainer') paypalContainerRef?: ElementRef<HTMLDivElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly orderService = inject(OrderService);
  private readonly paymentService = inject(PaymentService);
  private readonly toaster = inject(ToasterService);
  private readonly authService = inject(AuthService);
  private readonly breadcrumbService = inject(BreadcrumbService);

  order = signal<OrderDto | null>(null);
  gateways = signal<PaymentGatewayDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  selectedGateway = signal<PaymentGatewayDto | null>(null);
  intentLoading = signal(false);
  confirmLoading = signal(false);
  paypalOrderId = signal<string | null>(null);
  paypalClientId = signal<string | null>(null);
  paymobPaymentToken = signal<string | null>(null);
  paymobIframeId = signal<string | null>(null);

  orderId = computed(() => this.route.snapshot.queryParamMap.get('orderId'));
  isAuthenticated = computed(() => this.authService.isAuthenticated);
  isCod = computed(() => this.selectedGateway()?.name === CASH_ON_DELIVERY_NAME);
  isPaymob = computed(() => this.selectedGateway()?.name === 'Paymob');
  isPayPal = computed(() => this.selectedGateway()?.name === 'PayPal');

  private readonly sanitizer = inject(DomSanitizer);
  paymobIframeUrl = computed<SafeResourceUrl | null>(() => {
    const token = this.paymobPaymentToken();
    const iframeId = this.paymobIframeId();
    if (!token || !iframeId) return null;
    const url = `https://accept.paymob.com/api/acceptance/iframes/${iframeId}?payment_token=${encodeURIComponent(token)}`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  });

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::Cart', route: '/cart' },
      { label: 'ECommerce::Checkout', route: '/checkout' },
      { label: 'ECommerce::Payment' },
    ]);
    const oid = this.orderId();
    if (!oid) {
      this.error.set('Missing order.');
      this.loading.set(false);
      return;
    }
    if (!this.isAuthenticated()) {
      this.error.set('Please log in to complete payment.');
      this.loading.set(false);
      return;
    }
    this.loadOrderAndGateways(oid);
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  private loadOrderAndGateways(oid: string) {
    this.loading.set(true);
    this.error.set(null);
    this.orderService.get(oid).subscribe({
      next: (order) => {
        this.order.set(order ?? null);
        if (!order) {
          this.error.set('Order not found.');
          this.loading.set(false);
          return;
        }
        this.paymentService.getGateways().subscribe({
          next: (list) => {
            this.gateways.set(list);
            this.loading.set(false);
          },
          error: (err) => {
            this.error.set(err?.error?.error?.message ?? err?.message ?? 'Failed to load payment options.');
            this.loading.set(false);
          },
        });
      },
      error: (err) => {
        this.error.set(err?.error?.error?.message ?? err?.message ?? 'Failed to load order.');
        this.loading.set(false);
      },
    });
  }

  selectGateway(gw: PaymentGatewayDto) {
    this.selectedGateway.set(gw);
    this.paymobPaymentToken.set(null);
    this.paymobIframeId.set(null);
    this.paypalOrderId.set(null);
    this.paypalClientId.set(null);
    if (gw.name === CASH_ON_DELIVERY_NAME) {
      this.payWithCod();
    } else if (gw.name === 'Paymob') {
      this.createPaymobIntent();
    } else if (gw.name === 'PayPal') {
      this.createPayPalIntent();
    }
  }

  private get orderIdValue(): string | null {
    return this.orderId();
  }

  private createPaymobIntent() {
    const oid = this.orderIdValue;
    if (!oid) return;
    this.intentLoading.set(true);
    try {
      localStorage.setItem(PAYMOB_ORDER_ID_KEY, oid);
    } catch {}
    this.paymentService.createIntent(oid, 'Paymob').subscribe({
      next: (result: CreatePaymentIntentResult) => {
        this.intentLoading.set(false);
        if (!result.success) {
          this.toaster.error(result.errorMessage ?? 'Failed to create Paymob payment.');
          return;
        }
        this.paymobPaymentToken.set(result.clientSecret ?? null);
        this.paymobIframeId.set(result.publishableKeyOrClientId ?? null);
      },
      error: (err) => {
        this.intentLoading.set(false);
        this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Failed to create Paymob payment.');
      },
    });
  }

  private createPayPalIntent() {
    const oid = this.orderIdValue;
    if (!oid) return;
    this.intentLoading.set(true);
    this.paymentService.createIntent(oid, 'PayPal').subscribe({
      next: (result: CreatePaymentIntentResult) => {
        this.intentLoading.set(false);
        if (!result.success) {
          this.toaster.error(result.errorMessage ?? 'Failed to create PayPal order.');
          return;
        }
        this.paypalOrderId.set(result.gatewayPaymentId ?? null);
        this.paypalClientId.set(result.publishableKeyOrClientId ?? null);
        if (result.gatewayPaymentId && result.publishableKeyOrClientId) {
          setTimeout(() =>
            this.renderPayPalButton(oid, result.gatewayPaymentId!, result.publishableKeyOrClientId!),
            200
          );
        }
      },
      error: (err) => {
        this.intentLoading.set(false);
        this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Failed to create PayPal order.');
      },
    });
  }

  private renderPayPalButton(orderId: string, paypalOrderId: string, clientId: string) {
    if (!window.paypal) {
      this.loadPayPalScript(clientId, () => this.renderPayPalButton(orderId, paypalOrderId, clientId));
      return;
    }
    const container = this.paypalContainerRef?.nativeElement;
    if (container) container.innerHTML = '';
    if (!document.getElementById('paypal-button-container')) return;
    const buttons = window.paypal.Buttons({
      createOrder: () => Promise.resolve(paypalOrderId),
      onApprove: (data: { orderID: string }) => {
        this.confirmLoading.set(true);
        this.paymentService.confirm(orderId, data.orderID).subscribe({
          next: (result) => {
            this.confirmLoading.set(false);
            if (result.success) {
              this.toaster.success('ECommerce::PaymentSuccess');
              this.router.navigate(['/my-account/orders'], { queryParams: { placed: orderId } });
            } else {
              this.toaster.error(result.errorMessage ?? 'Confirmation failed.');
            }
          },
          error: (err) => {
            this.confirmLoading.set(false);
            this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Confirmation failed.');
          },
        });
        return Promise.resolve();
      },
      style: { layout: 'vertical', color: 'blue' },
    });
    buttons.render('#paypal-button-container');
  }

  private loadPayPalScript(clientId: string, onLoad: () => void) {
    if (document.querySelector('script[data-paypal-sdk]')) {
      if (window.paypal) onLoad();
      return;
    }
    const script = document.createElement('script');
    script.src = `https://www.paypal.com/sdk/js?client-id=${clientId}&currency=USD`;
    script.setAttribute('data-paypal-sdk', '');
    script.async = true;
    script.onload = onLoad;
    document.body.appendChild(script);
  }

  payWithCod() {
    const oid = this.orderIdValue;
    if (!oid) return;
    this.confirmLoading.set(true);
    this.paymentService.createIntent(oid, 'CashOnDelivery').subscribe({
      next: (result) => {
        if (!result.success) {
          this.confirmLoading.set(false);
          this.toaster.error(result.errorMessage ?? 'Failed.');
          return;
        }
        const gatewayPaymentId = result.gatewayPaymentId ?? 'COD';
        this.paymentService.confirm(oid, gatewayPaymentId).subscribe({
          next: (res) => {
            this.confirmLoading.set(false);
            if (res.success) {
              this.toaster.success('ECommerce::OrderPlaced');
              this.router.navigate(['/my-account/orders'], { queryParams: { placed: oid } });
            } else {
              this.toaster.error(res.errorMessage ?? 'Failed.');
            }
          },
          error: (err) => {
            this.confirmLoading.set(false);
            this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Failed.');
          },
        });
      },
      error: (err) => {
        this.confirmLoading.set(false);
        this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Failed.');
      },
    });
  }

  backToCheckout() {
    this.router.navigate(['/checkout']);
  }
}
