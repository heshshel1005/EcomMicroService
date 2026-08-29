import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { NewsletterService } from './newsletter.service';

@Component({
  selector: 'app-newsletter-unsubscribe',
  standalone: true,
  imports: [ReactiveFormsModule, LocalizationPipe],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">{{ 'ECommerce::Unsubscribe' | abpLocalization }}</h5>
              @if (done()) {
                <p class="text-muted mb-0">{{ 'ECommerce::NewsletterUnsubscribeSuccess' | abpLocalization }}</p>
              } @else {
                <form [formGroup]="form" (ngSubmit)="onSubmit()">
                  <div class="mb-3">
                    <label class="form-label">{{ 'ECommerce::YourEmail' | abpLocalization }}</label>
                    <input
                      type="email"
                      class="form-control"
                      formControlName="email"
                      [placeholder]="'ECommerce::YourEmail' | abpLocalization"
                    />
                  </div>
                  <button type="submit" class="btn btn-primary" [disabled]="form.invalid || submitting()">
                    @if (submitting()) {
                      <span class="spinner-border spinner-border-sm me-1" role="status"></span>
                    }
                    {{ 'ECommerce::Unsubscribe' | abpLocalization }}
                  </button>
                </form>
              }
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class NewsletterUnsubscribeComponent implements OnInit {
  private readonly newsletterService = inject(NewsletterService);
  private readonly toaster = inject(ToasterService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });
  submitting = signal(false);
  done = signal(false);

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email?.trim()) {
      this.form.patchValue({ email: email.trim() });
      this.performUnsubscribe(email.trim());
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.submitting()) return;
    const email = (this.form.get('email')?.value ?? '').trim();
    this.performUnsubscribe(email);
  }

  private performUnsubscribe(email: string): void {
    this.submitting.set(true);
    this.newsletterService.unsubscribe(email).subscribe({
      next: () => {
        this.submitting.set(false);
        this.done.set(true);
        this.toaster.success('ECommerce::NewsletterUnsubscribeSuccess');
      },
      error: () => {
        this.submitting.set(false);
        this.toaster.error('ECommerce::NewsletterUnsubscribeError');
      },
    });
  }
}
