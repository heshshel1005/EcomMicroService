import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import {
  CheckoutService,
  CheckoutSummaryDto,
  SubmitCheckoutDto,
  CheckoutAddressDto,
  ShippingOptionDto,
} from './checkout.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, LocalizationPipe, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss'],
})
export class CheckoutComponent implements OnInit, OnDestroy {
  private readonly checkoutService = inject(CheckoutService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly breadcrumbService = inject(BreadcrumbService);

  summary = signal<CheckoutSummaryDto | null>(null);
  loading = signal(true);
  submitLoading = signal(false);
  error = signal<string | null>(null);

  form!: FormGroup;
  billingSameAsShipping = true;
  selectedShippingCode = signal<string | null>(null);
  /** Coupon code entered by user; applied when summary is loaded with this code. */
  couponCode = signal<string>('');
  applyingCoupon = signal(false);

  shippingAmount = computed(() => {
    const s = this.summary();
    const code = this.selectedShippingCode();
    if (!s?.shippingOptions?.length) return 0;
    const opt = code
      ? s.shippingOptions.find((o) => o.code === code)
      : s.shippingOptions[0];
    return opt?.amount ?? 0;
  });

  taxAmount = computed(() => this.summary()?.taxAmount ?? 0);
  subTotal = computed(() => this.summary()?.subTotal ?? 0);
  discountAmount = computed(() => this.summary()?.discountAmount ?? 0);
  total = computed(() => Math.max(0, this.subTotal() - this.discountAmount()) + this.shippingAmount() + this.taxAmount());

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::Cart', route: '/cart' },
      { label: 'ECommerce::Checkout' },
    ]);
    this.buildForm();
    this.loadSummary();
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  private buildForm() {
    this.form = this.fb.group({
      couponCode: [''],
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: [''],
      contactName: [''],
      shippingStreet: ['', Validators.required],
      shippingStreet2: [''],
      shippingCity: [''],
      shippingRegion: [''],
      shippingPostalCode: [''],
      shippingCountry: [''],
      deliveryInstructions: [''],
      billingSameAsShipping: [true],
      billingStreet: [''],
      billingStreet2: [''],
      billingCity: [''],
      billingRegion: [''],
      billingPostalCode: [''],
      billingCountry: [''],
    });
    this.form.get('billingSameAsShipping')?.valueChanges.subscribe((v) => {
      this.billingSameAsShipping = !!v;
    });
  }

  private loadSummary(couponCode?: string | null) {
    this.loading.set(true);
    this.error.set(null);
    this.checkoutService.getSummary(undefined, couponCode ?? undefined).subscribe({
      next: (s) => {
        this.summary.set(s);
        const code = s.defaultShippingMethodCode ?? s.shippingOptions?.[0]?.code ?? null;
        this.selectedShippingCode.set(code);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.error?.message ?? err?.message ?? 'Failed to load checkout summary.';
        this.error.set(msg);
      },
    });
  }

  applyCoupon() {
    const code = this.form.get('couponCode')?.value?.trim() ?? this.couponCode();
    if (!code) return;
    this.couponCode.set(code);
    this.applyingCoupon.set(true);
    this.checkoutService.getSummary(undefined, code).subscribe({
      next: (s) => {
        this.summary.set(s);
        this.applyingCoupon.set(false);
        if (s.appliedCouponCode) {
          this.toaster.success('ECommerce::CouponApplied', '');
        } else if (code) {
          this.toaster.warn('ECommerce::CouponInvalid', '');
        }
      },
      error: () => this.applyingCoupon.set(false),
    });
  }

  selectShipping(option: ShippingOptionDto) {
    this.selectedShippingCode.set(option.code);
  }

  placeOrder() {
    if (this.form.invalid || this.submitLoading()) return;
    const s = this.summary();
    if (!s) return;
    const code = this.selectedShippingCode();
    if (!code) {
      this.toaster.error('ECommerce::ShippingMethod', 'Select a shipping method.');
      return;
    }
    const v = this.form.value;
    const shippingAddress: CheckoutAddressDto = {
      street: v.shippingStreet,
      street2: v.shippingStreet2 || null,
      city: v.shippingCity || null,
      region: v.shippingRegion || null,
      postalCode: v.shippingPostalCode || null,
      country: v.shippingCountry || null,
      deliveryInstructions: v.deliveryInstructions || null,
    };
    const billingAddress = this.billingSameAsShipping
      ? undefined
      : {
          street: v.billingStreet,
          street2: v.billingStreet2 || null,
          city: v.billingCity || null,
          region: v.billingRegion || null,
          postalCode: v.billingPostalCode || null,
          country: v.billingCountry || null,
          deliveryInstructions: null,
        };
    const body: SubmitCheckoutDto = {
      contactEmail: v.contactEmail,
      contactPhone: v.contactPhone || null,
      contactName: v.contactName || null,
      shippingAddress,
      billingSameAsShipping: this.billingSameAsShipping,
      billingAddress: billingAddress ?? null,
      shippingMethodCode: code,
      couponCode: this.couponCode() || v.couponCode || null,
    };
    this.submitLoading.set(true);
    this.checkoutService.submitOrder(body).subscribe({
      next: (result) => {
        this.submitLoading.set(false);
        this.checkoutService.setGuestCartId(null);
        this.toaster.success('ECommerce::OrderCreated');
        this.router.navigate(['/checkout/payment'], { queryParams: { orderId: result.orderId } });
      },
      error: (err) => {
        this.submitLoading.set(false);
        const msg = err?.error?.error?.message ?? err?.message ?? 'Failed to place order.';
        this.toaster.error(msg);
      },
    });
  }
}
