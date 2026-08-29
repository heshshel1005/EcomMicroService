import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { PaymentService } from './payment.service';

const PAYMOB_ORDER_ID_KEY = 'paymobPendingOrderId';

/**
 * Handles redirect from Paymob after payment. Configure Paymob callback URL to point here.
 * Expects query param: id (transaction id). Order id is read from sessionStorage (set when opening Paymob iframe).
 */
@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="container py-5 text-center">
      @if (processing) {
        <p class="text-muted">
          <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
          Confirming your payment...
        </p>
      } @else if (error) {
        <p class="text-danger">{{ error }}</p>
        <a routerLink="/my-account/orders" class="btn btn-outline-primary">View orders</a>
      }
    </div>
  `,
})
export class PaymentSuccessComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly paymentService = inject(PaymentService);
  private readonly toaster = inject(ToasterService);

  processing = true;
  error: string | null = null;

  ngOnInit() {
    const transactionId = this.route.snapshot.queryParamMap.get('id') || this.route.snapshot.queryParamMap.get('transaction_id');
    let orderId: string | null = null;
    try {
      orderId = localStorage.getItem(PAYMOB_ORDER_ID_KEY);
      if (orderId) localStorage.removeItem(PAYMOB_ORDER_ID_KEY);
    } catch {}

    if (!transactionId || !orderId) {
      this.processing = false;
      this.error = 'Missing payment or order information. Check your orders.';
      return;
    }

    this.paymentService.confirm(orderId, transactionId).subscribe({
      next: (result) => {
        this.processing = false;
        if (result.success) {
          this.toaster.success('ECommerce::PaymentSuccess');
          this.router.navigate(['/my-account/orders'], { queryParams: { placed: orderId } });
        } else {
          this.error = result.errorMessage ?? 'Confirmation failed.';
        }
      },
      error: (err) => {
        this.processing = false;
        this.error = err?.error?.error?.message ?? err?.message ?? 'Confirmation failed.';
      },
    });
  }
}
