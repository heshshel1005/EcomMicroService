import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { OrderingService, Basket, OrderDto } from '../../services/ordering.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cart-page.component.html',
  styleUrl: './cart-page.component.scss'
})
export class CartPageComponent implements OnInit, OnDestroy {
  basket: Basket = { items: [], totalPrice: 0 };
  private basketSub!: Subscription;

  // View state: 'cart' | 'checkout' | 'success'
  viewState: 'cart' | 'checkout' | 'success' = 'cart';

  // Checkout Form
  shippingAddress = {
    street: '',
    city: '',
    state: '',
    zipCode: ''
  };

  // Form Validation and Loading
  submitting = false;
  placedOrder: OrderDto | null = null;

  constructor(private orderingService: OrderingService) {}

  ngOnInit(): void {
    // Force a fresh load of the basket from Redis
    this.orderingService.loadBasket();
    this.basketSub = this.orderingService.basket$.subscribe(b => {
      this.basket = b;
    });
  }

  ngOnDestroy(): void {
    if (this.basketSub) {
      this.basketSub.unsubscribe();
    }
  }

  updateQuantity(productId: string, currentQty: number, delta: number): void {
    const newQty = currentQty + delta;
    if (newQty > 0) {
      this.orderingService.updateItemQuantity(productId, newQty).subscribe();
    } else {
      this.removeItem(productId);
    }
  }

  removeItem(productId: string): void {
    if (confirm('Are you sure you want to remove this item from your cart?')) {
      this.orderingService.removeItem(productId).subscribe();
    }
  }

  clearCart(): void {
    if (confirm('Are you sure you want to clear your shopping cart?')) {
      this.orderingService.clearBasket().subscribe();
    }
  }

  proceedToCheckout(): void {
    if (this.basket.items.length > 0) {
      this.viewState = 'checkout';
    }
  }

  backToCart(): void {
    this.viewState = 'cart';
  }

  submitCheckout(): void {
    if (!this.shippingAddress.street || !this.shippingAddress.city || !this.shippingAddress.state || !this.shippingAddress.zipCode) {
      alert('Please fill out all address fields.');
      return;
    }

    this.submitting = true;
    this.orderingService.createOrder(this.shippingAddress).subscribe({
      next: (order) => {
        this.submitting = false;
        if (order) {
          this.placedOrder = order;
          this.viewState = 'success';
          // Reset shipping address form
          this.shippingAddress = { street: '', city: '', state: '', zipCode: '' };
        } else {
          alert('There was an issue processing your order. Please try again.');
        }
      },
      error: () => {
        this.submitting = false;
        alert('There was an error connecting to the Ordering Service.');
      }
    });
  }
}
