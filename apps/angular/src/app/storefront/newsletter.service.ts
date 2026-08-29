import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface SubscribeNewsletterDto {
  email?: string;
  name?: string | null;
}

export interface NewsletterSubscriptionStatusDto {
  isSubscribed: boolean;
}

@Injectable({ providedIn: 'root' })
export class NewsletterService {
  private readonly rest = inject(RestService);

  /** Requires auth. Returns whether the current user is subscribed. */
  getMyStatus(): Observable<NewsletterSubscriptionStatusDto> {
    return this.rest.request<void, NewsletterSubscriptionStatusDto>({
      method: 'GET',
      url: '/api/marketing/newsletter/my-status',
    });
  }

  /** Requires auth. Subscribes the current user's email; optional name in body. */
  subscribe(dto?: { name?: string | null }): Observable<void> {
    return this.rest.request<SubscribeNewsletterDto, void>({
      method: 'POST',
      url: '/api/marketing/newsletter/subscribe',
      body: dto ?? {},
    });
  }

  /** With no args: requires auth, unsubscribes current user. With email: for public unsubscribe link. */
  unsubscribe(email?: string): Observable<void> {
    const params: Record<string, string> = {};
    if (email != null && email !== '') params['email'] = email;
    return this.rest.request<void, void>({
      method: 'POST',
      url: '/api/marketing/newsletter/unsubscribe',
      params: Object.keys(params).length ? params : undefined,
    });
  }
}
