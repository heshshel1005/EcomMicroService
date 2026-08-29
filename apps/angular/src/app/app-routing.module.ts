import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/module-federation';
import { authGuard, permissionGuard } from '@abp/ng.core';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.module').then(m => m.HomeModule),
  },
  {
    path: 'catalog',
    loadChildren: () =>
      loadRemoteModule({
        type: 'module',
        remoteEntry: 'http://localhost:4201/remoteEntry.js',
        exposedModule: './Module',
      }).then(m => m.routes),
  },
  {
    path: 'cart',
    loadChildren: () =>
      loadRemoteModule({
        type: 'module',
        remoteEntry: 'http://localhost:4202/remoteEntry.js',
        exposedModule: './Module',
      }).then(m => m.routes),
  },
  {
    path: 'checkout',
    loadComponent: () => import('./storefront/checkout/checkout.component').then(c => c.CheckoutComponent),
  },
  {
    path: 'payment/:orderId',
    loadComponent: () => import('./storefront/payment/payment.component').then(c => c.PaymentComponent),
  },
  {
    path: 'payment-success',
    loadComponent: () =>
      import('./storefront/payment/payment-success.component').then(c => c.PaymentSuccessComponent),
  },
  {
    path: 'my-account',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./storefront/customer-dashboard/customer-dashboard-layout.component').then(
        c => c.CustomerDashboardLayoutComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./storefront/customer-dashboard/customer-dashboard-overview.component').then(
            c => c.CustomerDashboardOverviewComponent
          ),
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./storefront/customer-dashboard/customer-order-history.component').then(
            c => c.CustomerOrderHistoryComponent
          ),
      },
      {
        path: 'orders/:id',
        loadComponent: () =>
          import('./storefront/customer-dashboard/customer-order-detail.component').then(
            c => c.CustomerOrderDetailComponent
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./storefront/customer-dashboard/customer-profile.component').then(
            c => c.CustomerProfileComponent
          ),
      },
      {
        path: 'wishlist',
        loadComponent: () =>
          import('./storefront/customer-dashboard/customer-wishlist.component').then(
            c => c.CustomerWishlistComponent
          ),
      },
    ],
  },
  {
    path: 'organization-signup',
    loadComponent: () =>
      import('./storefront/organization-signup.component').then(c => c.OrganizationSignupComponent),
  },
  {
    path: 'newsletter/unsubscribe',
    loadComponent: () =>
      import('./storefront/newsletter-unsubscribe.component').then(c => c.NewsletterUnsubscribeComponent),
  },
  {
    path: 'registry/:slug',
    loadComponent: () =>
      import('./storefront/gift-registry/registry-view.component').then(c => c.RegistryViewComponent),
  },
  {
    path: 'admin/catalog',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'ECommerce.Catalog' },
    children: [
      {
        path: 'categories',
        loadComponent: () =>
          import('./admin/catalog/categories/category-list.component').then(c => c.CategoryListComponent),
      },
      {
        path: 'categories/create',
        loadComponent: () =>
          import('./admin/catalog/categories/category-edit.component').then(c => c.CategoryEditComponent),
      },
      {
        path: 'categories/edit/:id',
        loadComponent: () =>
          import('./admin/catalog/categories/category-edit.component').then(c => c.CategoryEditComponent),
      },
      {
        path: 'brands',
        data: { requiredPolicy: 'ECommerce.Catalog.Brands' },
        loadComponent: () =>
          import('./admin/catalog/brands/brand-list.component').then(c => c.BrandListComponent),
      },
      {
        path: 'brands/create',
        data: { requiredPolicy: 'ECommerce.Catalog.Brands' },
        loadComponent: () =>
          import('./admin/catalog/brands/brand-edit.component').then(c => c.BrandEditComponent),
      },
      {
        path: 'brands/edit/:id',
        data: { requiredPolicy: 'ECommerce.Catalog.Brands' },
        loadComponent: () =>
          import('./admin/catalog/brands/brand-edit.component').then(c => c.BrandEditComponent),
      },
      {
        path: 'models',
        data: { requiredPolicy: 'ECommerce.Catalog.BrandModels' },
        loadComponent: () =>
          import('./admin/catalog/brand-models/brand-model-list.component').then(c => c.BrandModelListComponent),
      },
      {
        path: 'models/create',
        data: { requiredPolicy: 'ECommerce.Catalog.BrandModels' },
        loadComponent: () =>
          import('./admin/catalog/brand-models/brand-model-edit.component').then(c => c.BrandModelEditComponent),
      },
      {
        path: 'models/edit/:id',
        data: { requiredPolicy: 'ECommerce.Catalog.BrandModels' },
        loadComponent: () =>
          import('./admin/catalog/brand-models/brand-model-edit.component').then(c => c.BrandModelEditComponent),
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./admin/catalog/products/product-list.component').then(c => c.ProductListComponent),
      },
      {
        path: 'products/create',
        loadComponent: () =>
          import('./admin/catalog/products/product-edit.component').then(c => c.ProductEditComponent),
      },
      {
        path: 'products/edit/:id',
        loadComponent: () =>
          import('./admin/catalog/products/product-edit.component').then(c => c.ProductEditComponent),
      },
      {
        path: 'product-types',
        loadComponent: () =>
          import('./admin/catalog/metadata/product-types/product-type-list.component').then(
            c => c.ProductTypeListComponent
          ),
      },
      {
        path: 'product-types/create',
        loadComponent: () =>
          import('./admin/catalog/metadata/product-types/product-type-edit.component').then(
            c => c.ProductTypeEditComponent
          ),
      },
      {
        path: 'product-types/edit/:id',
        loadComponent: () =>
          import('./admin/catalog/metadata/product-types/product-type-edit.component').then(
            c => c.ProductTypeEditComponent
          ),
      },
      {
        path: 'attribute-definitions',
        loadComponent: () =>
          import('./admin/catalog/metadata/attribute-definitions/attribute-definition-list.component').then(
            c => c.AttributeDefinitionListComponent
          ),
      },
      {
        path: 'attribute-definitions/create',
        loadComponent: () =>
          import('./admin/catalog/metadata/attribute-definitions/attribute-definition-edit.component').then(
            c => c.AttributeDefinitionEditComponent
          ),
      },
      {
        path: 'attribute-definitions/edit/:id',
        loadComponent: () =>
          import('./admin/catalog/metadata/attribute-definitions/attribute-definition-edit.component').then(
            c => c.AttributeDefinitionEditComponent
          ),
      },
      {
        path: 'reviews',
        loadComponent: () =>
          import('./admin/catalog/reviews/review-list.component').then(c => c.ReviewListComponent),
      },
    ],
  },
  {
    path: 'admin/orders',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'ECommerce.Administration' },
    children: [
      {
        path: '',
        loadComponent: () => import('./admin/orders/order-list.component').then(c => c.OrderListComponent),
      },
      {
        path: ':id',
        loadComponent: () => import('./admin/orders/order-detail.component').then(c => c.OrderDetailComponent),
      },
    ],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.IdentityModule.forLazy()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.TenantManagementModule.forLazy()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
