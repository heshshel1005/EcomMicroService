import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface GiftRegistryItemDto {
  id: string;
  productVariantId: string;
  productId: string;
  productName: string;
  sku: string;
  price?: number | null;
  desiredQuantity: number;
  quantityClaimed: number;
  quantityRemaining: number;
  note?: string | null;
}

export interface GiftRegistryDto {
  id: string;
  title: string;
  slug: string;
  eventDate?: string | null;
  items: GiftRegistryItemDto[];
}

export interface ClaimRegistryItemDto {
  giftRegistryItemId: string;
  quantity: number;
  claimantName?: string | null;
  message?: string | null;
  addToCart: boolean;
}

export interface CreateGiftRegistryDto {
  title: string;
  slug: string;
  eventDate?: string | null;
}

export interface AddGiftRegistryItemDto {
  productVariantId: string;
  desiredQuantity: number;
  note?: string | null;
}

@Injectable({ providedIn: 'root' })
export class GiftRegistryService {
  private readonly rest = inject(RestService);

  getBySlug(slug: string): Observable<GiftRegistryDto | null> {
    return this.rest.request<void, GiftRegistryDto | null>({
      method: 'GET',
      url: `/api/marketing/gift-registry/by-slug/${encodeURIComponent(slug)}`,
    });
  }

  claim(body: ClaimRegistryItemDto): Observable<void> {
    return this.rest.request<ClaimRegistryItemDto, void>({
      method: 'POST',
      url: '/api/marketing/gift-registry/claim',
      body,
    });
  }

  create(body: CreateGiftRegistryDto): Observable<GiftRegistryDto> {
    return this.rest.request<CreateGiftRegistryDto, GiftRegistryDto>({
      method: 'POST',
      url: '/api/marketing/gift-registry',
      body,
    });
  }

  getMyRegistries(): Observable<GiftRegistryDto[]> {
    return this.rest.request<void, GiftRegistryDto[]>({
      method: 'GET',
      url: '/api/marketing/gift-registry/my',
    });
  }

  addItem(giftRegistryId: string, body: AddGiftRegistryItemDto): Observable<GiftRegistryDto> {
    return this.rest.request<AddGiftRegistryItemDto, GiftRegistryDto>({
      method: 'POST',
      url: `/api/marketing/gift-registry/${giftRegistryId}/items`,
      body,
    });
  }

  removeItem(giftRegistryId: string, giftRegistryItemId: string): Observable<GiftRegistryDto> {
    return this.rest.request<void, GiftRegistryDto>({
      method: 'DELETE',
      url: `/api/marketing/gift-registry/${giftRegistryId}/items/${giftRegistryItemId}`,
    });
  }
}
