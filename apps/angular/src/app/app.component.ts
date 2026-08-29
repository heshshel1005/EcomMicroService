import { Component, OnInit } from '@angular/core';
import { RoutesService } from '@abp/ng.core';

@Component({
  selector: 'app-root',
  standalone: false,
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
})
export class AppComponent implements OnInit {
  constructor(private routesService: RoutesService) {}

  ngOnInit(): void {
    this.updateCartCount();
    
    // Listen to changes from local storage
    window.addEventListener('storage', () => {
      this.updateCartCount();
    });

    // Fallback polling for robust real-time synchronization
    setInterval(() => {
      this.updateCartCount();
    }, 2000);
  }

  private updateCartCount(): void {
    const countStr = localStorage.getItem('ecom_basket_count') || '0';
    const count = parseInt(countStr, 10);
    const displayName = count > 0 ? `Shopping Cart (${count})` : 'Shopping Cart';
    
    this.routesService.patch('/cart', {
      name: displayName
    });
  }
}
