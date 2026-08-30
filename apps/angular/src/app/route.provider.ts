import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/catalog',
        name: 'Catalog',
        iconClass: 'fas fa-store',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/cart',
        name: 'Shopping Cart',
        iconClass: 'fas fa-shopping-cart',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/admin/catalog/products',
        name: 'Admin Catalog',
        iconClass: 'fas fa-boxes',
        order: 10,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog',
      },
      {
        path: '/admin/catalog/categories',
        name: 'Categories',
        iconClass: 'fas fa-sitemap',
        order: 11,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog',
      },
      {
        path: '/admin/catalog/product-types',
        name: 'Product types',
        iconClass: 'fas fa-list',
        order: 12,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog',
      },
      {
        path: '/admin/catalog/attribute-definitions',
        name: 'Attributes',
        iconClass: 'fas fa-sliders-h',
        order: 13,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog',
      },
      {
        path: '/admin/catalog/brands',
        name: 'Brands',
        iconClass: 'fas fa-tags',
        order: 14,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog.Brands',
      },
      {
        path: '/admin/catalog/models',
        name: 'Models',
        iconClass: 'fas fa-car',
        order: 15,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog.BrandModels',
      },
      {
        path: '/admin/catalog/reviews',
        name: 'Reviews',
        iconClass: 'fas fa-star',
        order: 16,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Catalog',
      },
      {
        path: '/admin/orders',
        name: 'Orders',
        iconClass: 'fas fa-receipt',
        order: 17,
        layout: eLayoutType.application,
        requiredPolicy: 'ECommerce.Administration',
      },
      {
        path: '/my-account',
        name: 'My account',
        iconClass: 'fas fa-user',
        order: 4,
        layout: eLayoutType.application,
      },
      {
        path: '/checkout',
        name: 'Checkout',
        iconClass: 'fas fa-credit-card',
        order: 5,
        layout: eLayoutType.application,
      },
    ]);
  };
}
