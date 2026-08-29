import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { AuthService } from '@abp/ng.core';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import {
  GiftRegistryService,
  GiftRegistryDto,
  GiftRegistryItemDto,
  ClaimRegistryItemDto,
} from '../gift-registry.service';
import { CartService } from '../cart.service';

@Component({
  selector: 'app-registry-view',
  standalone: true,
  imports: [DecimalPipe, DatePipe, RouterLink, LocalizationPipe],
  template: `
    <div class="registry-page">
      @if (loading()) {
        <p class="text-muted">{{ 'ECommerce::Loading' | abpLocalization }}</p>
      } @else if (error()) {
        <div class="alert alert-danger">{{ error() }}</div>
      } @else if (!registry()) {
        <div class="alert alert-warning">{{ 'ECommerce::GiftRegistryNotFound' | abpLocalization }}</div>
      } @else {
        <h1 class="mb-2">{{ registry()!.title }}</h1>
        @if (registry()!.eventDate) {
          <p class="text-muted mb-4">{{ 'ECommerce::EventDate' | abpLocalization }}: {{ registry()!.eventDate | date:'longDate' }}</p>
        }
        @if (!registry()!.items?.length) {
          <p class="text-muted">{{ 'ECommerce::RegistryEmpty' | abpLocalization }}</p>
        } @else {
          <ul class="list-group">
            @for (item of registry()!.items; track item.id) {
              <li class="list-group-item d-flex justify-content-between align-items-center flex-wrap gap-2">
                <div>
                  <a [routerLink]="['/catalog/product', item.productId]">{{ item.productName }}</a>
                  <span class="text-muted ms-2">({{ item.sku }})</span>
                  @if (item.price !== null) {
                    <span class="ms-2">{{ item.price | number:'1.2-2' }} {{ 'ECommerce::Price' | abpLocalization }}</span>
                  }
                  <div class="small text-muted mt-1">
                    {{ 'ECommerce::Quantity' | abpLocalization }}: {{ item.quantityRemaining }} / {{ item.desiredQuantity }} {{ 'ECommerce::Remaining' | abpLocalization }}
                  </div>
                </div>
                @if (item.quantityRemaining > 0) {
                  <div class="d-flex flex-column gap-2 align-items-end">
                    <input
                      type="number"
                      class="form-control form-control-sm"
                      style="width: 80px"
                      min="1"
                      [max]="item.quantityRemaining"
                      [value]="claimQuantities()[item.id] ?? 1"
                      (change)="setClaimQty(item.id, $any($event.target).value)"
                    />
                    <div class="d-flex gap-2">
                      <button
                        type="button"
                        class="btn btn-sm btn-outline-primary"
                        [disabled]="claimingId() === item.id"
                        (click)="claim(item, false)"
                      >
                        @if (claimingId() === item.id) {
                          <span class="spinner-border spinner-border-sm" role="status"></span>
                        } @else {
                          {{ 'ECommerce::Reserve' | abpLocalization }}
                        }
                      </button>
                      <button
                        type="button"
                        class="btn btn-sm btn-primary"
                        [disabled]="claimingId() === item.id"
                        (click)="claim(item, true)"
                      >
                        {{ 'ECommerce::Purchase' | abpLocalization }} &amp; {{ 'ECommerce::AddToCart' | abpLocalization }}
                      </button>
                    </div>
                  </div>
                } @else {
                  <span class="badge bg-secondary">{{ 'ECommerce::AllClaimed' | abpLocalization }}</span>
                }
              </li>
            }
          </ul>
        }
      }
    </div>
  `,
})
export class RegistryViewComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly registryService = inject(GiftRegistryService);
  private readonly toaster = inject(ToasterService);
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly cartService = inject(CartService);
  protected readonly authService = inject(AuthService);

  registry = signal<GiftRegistryDto | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  claimingId = signal<string | null>(null);
  claimQuantities = signal<Record<string, number>>({});

  ngOnInit() {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.loading.set(false);
      this.error.set('Missing slug');
      return;
    }
    this.breadcrumbService.setItems([
      { label: 'ECommerce::GiftRegistry', route: null },
      { label: slug },
    ]);
    this.registryService.getBySlug(slug).subscribe({
      next: (r) => {
        this.registry.set(r ?? null);
        const qty: Record<string, number> = {};
        if (r?.items) for (const i of r.items) qty[i.id] = 1;
        this.claimQuantities.set(qty);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.error?.message ?? err?.message ?? 'Failed to load registry.');
      },
    });
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  setClaimQty(itemId: string, value: string | number): void {
    const n = Math.max(1, Math.min(999, parseInt(String(value), 10) || 1));
    this.claimQuantities.update((q) => ({ ...q, [itemId]: n }));
  }

  claim(item: GiftRegistryItemDto, addToCart: boolean): void {
    const qty = this.claimQuantities()[item.id] ?? 1;
    if (qty > item.quantityRemaining) {
      this.toaster.warn('ECommerce::GiftRegistryInsufficientQuantity');
      return;
    }
    if (addToCart && !this.authService.isAuthenticated) {
      this.toaster.warn('ECommerce::LoginRequiredForAddToCart');
      return;
    }
    this.claimingId.set(item.id);
    const body: ClaimRegistryItemDto = {
      giftRegistryItemId: item.id,
      quantity: qty,
      addToCart,
    };
    this.registryService.claim(body).subscribe({
      next: () => {
        this.claimingId.set(null);
        this.toaster.success(addToCart ? 'ECommerce::AddToCart' : 'ECommerce::Reserve');
        if (addToCart) this.cartService.getCart().subscribe();
        const slug = this.route.snapshot.paramMap.get('slug');
        if (slug) {
          this.registryService.getBySlug(slug).subscribe((r) => this.registry.set(r ?? null));
        }
      },
      error: (err) => {
        this.claimingId.set(null);
        this.toaster.error(err?.error?.error?.message ?? err?.message ?? 'Failed to claim.');
      },
    });
  }
}
