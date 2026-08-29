import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { NewsletterService } from './newsletter.service';

@Component({
  selector: 'app-newsletter-subscribe',
  standalone: true,
  imports: [ReactiveFormsModule, LocalizationPipe],
  template: `
    <div class="newsletter-subscribe">
      <p class="newsletter-subscribe-label">{{ 'ECommerce::SubscribeToNewsletter' | abpLocalization }}</p>
      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="newsletter-subscribe-form">
        <input
          type="email"
          class="form-control form-control-sm newsletter-email"
          formControlName="email"
          [placeholder]="'ECommerce::YourEmail' | abpLocalization"
          aria-label="Email"
        />
        <input
          type="text"
          class="form-control form-control-sm newsletter-name"
          formControlName="name"
          [placeholder]="'ECommerce::Name' | abpLocalization"
          aria-label="Name (optional)"
        />
        <button type="submit" class="btn btn-sm btn-outline-light" [disabled]="form.invalid || submitting()">
          @if (submitting()) {
            <span class="spinner-border spinner-border-sm" role="status"></span>
          } @else {
            {{ 'ECommerce::NewsletterSubscribe' | abpLocalization }}
          }
        </button>
      </form>
    </div>
  `,
  styles: [
    `
      .newsletter-subscribe {
        margin-top: 0.5rem;
      }
      .newsletter-subscribe-label {
        margin-bottom: 0.35rem;
        font-size: 0.9rem;
      }
      .newsletter-subscribe-form {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        align-items: center;
      }
      .newsletter-email {
        min-width: 180px;
      }
      .newsletter-name {
        min-width: 120px;
      }
    `,
  ],
})
export class NewsletterSubscribeComponent {
  private readonly newsletterService = inject(NewsletterService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    name: [''],
  });
  submitting = signal(false);

  onSubmit(): void {
    if (this.form.invalid || this.submitting()) return;
    const name = (this.form.get('name')?.value ?? '').trim() || null;
    this.submitting.set(true);
    this.newsletterService.subscribe({ name }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.toaster.success('ECommerce::NewsletterSubscribeSuccess');
        this.form.reset({ email: '', name: '' });
      },
      error: (err) => {
        this.submitting.set(false);
        const msg = err?.error?.error?.message ?? 'ECommerce::NewsletterEmailRequired';
        this.toaster.error(msg);
      },
    });
  }
}
