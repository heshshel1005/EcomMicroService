import { Injectable } from '@angular/core';

export interface BreadcrumbItem {
  text?: string;
  label?: string;
  link?: string;
  route?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  setItems(_items: BreadcrumbItem[]): void {
    // Storefront breadcrumbs are optional in the microservice shell.
  }

  clear(): void {}
}
