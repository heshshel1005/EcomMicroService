import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';

@Component({
  selector: 'app-customer-dashboard-overview',
  template: `
    <div class="card">
      <div class="card-body">
        <h3 class="card-title">{{ 'ECommerce::CustomerDashboardOverview' | abpLocalization }}</h3>
        <p class="text-muted">{{ 'ECommerce::CustomerDashboardOverviewDescription' | abpLocalization }}</p>
        <div class="d-flex flex-wrap gap-2">
          <a routerLink="/my-account/orders" class="btn btn-outline-primary">{{ 'ECommerce::OrderHistory' | abpLocalization }}</a>
          <a routerLink="/my-account/profile" class="btn btn-outline-primary">{{ 'ECommerce::Profile' | abpLocalization }}</a>
          <a routerLink="/my-account/wishlist" class="btn btn-outline-primary">{{ 'ECommerce::Wishlist' | abpLocalization }}</a>
        </div>
      </div>
    </div>
  `,
  imports: [LocalizationPipe, RouterLink],
})
export class CustomerDashboardOverviewComponent implements OnInit, OnDestroy {
  private readonly breadcrumbService = inject(BreadcrumbService);

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::CustomerDashboard', route: '/my-account' },
      { label: 'ECommerce::CustomerDashboardOverview' },
    ]);
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }
}
