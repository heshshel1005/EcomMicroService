import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface CustomerProfileDto {
  id: string;
  userId: string;
  displayName: string;
  phoneNumber?: string | null;
  email?: string | null;
}

export interface UpdateCustomerProfileDto {
  displayName: string;
  phoneNumber?: string | null;
}

export interface CustomerAddressDto {
  id: string;
  userId: string;
  label: string;
  street: string;
  city?: string | null;
  region?: string | null;
  postalCode?: string | null;
  country?: string | null;
  isDefaultShipping: boolean;
  isDefaultBilling: boolean;
}

export interface CreateUpdateCustomerAddressDto {
  label: string;
  street: string;
  city?: string | null;
  region?: string | null;
  postalCode?: string | null;
  country?: string | null;
  isDefaultShipping: boolean;
  isDefaultBilling: boolean;
}

@Injectable({ providedIn: 'root' })
export class CustomerProfileService {
  private readonly rest = inject(RestService);

  getMyProfile(): Observable<CustomerProfileDto> {
    return this.rest.request<void, unknown>({
      method: 'GET',
      url: '/api/customer/profile',
    }).pipe(map((res) => this.normalizeProfile(res)));
  }

  updateMyProfile(dto: UpdateCustomerProfileDto): Observable<CustomerProfileDto> {
    return this.rest.request<UpdateCustomerProfileDto, unknown>({
      method: 'PUT',
      url: '/api/customer/profile',
      body: dto,
    }).pipe(map((res) => this.normalizeProfile(res)));
  }

  getMyAddresses(): Observable<CustomerAddressDto[]> {
    return this.rest.request<void, unknown>({
      method: 'GET',
      url: '/api/customer/profile/addresses',
    }).pipe(map((res) => this.normalizeAddressList(res)));
  }

  createAddress(dto: CreateUpdateCustomerAddressDto): Observable<CustomerAddressDto> {
    return this.rest.request<CreateUpdateCustomerAddressDto, unknown>({
      method: 'POST',
      url: '/api/customer/profile/addresses',
      body: dto,
    }).pipe(map((res) => this.normalizeAddress(res)));
  }

  updateAddress(id: string, dto: CreateUpdateCustomerAddressDto): Observable<CustomerAddressDto> {
    return this.rest.request<CreateUpdateCustomerAddressDto, unknown>({
      method: 'PUT',
      url: `/api/customer/profile/addresses/${id}`,
      body: dto,
    }).pipe(map((res) => this.normalizeAddress(res)));
  }

  deleteAddress(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/customer/profile/addresses/${id}`,
    });
  }

  /** Unwrap result/body and normalize to CustomerProfileDto (camelCase). */
  private normalizeProfile(res: unknown): CustomerProfileDto {
    const raw = (res != null && typeof res === 'object' ? res as Record<string, unknown> : {}) as Record<string, unknown>;
    const o = raw.result ?? raw.body ?? raw;
    const p = (o != null && typeof o === 'object' ? o : {}) as Record<string, unknown>;
    return {
      id: String(p.id ?? p.Id ?? ''),
      userId: String(p.userId ?? p.UserId ?? ''),
      displayName: String(p.displayName ?? p.DisplayName ?? ''),
      phoneNumber: (p.phoneNumber ?? p.PhoneNumber) as string | null | undefined,
      email: (p.email ?? p.Email) as string | null | undefined,
    };
  }

  /** Unwrap result/body and normalize to CustomerAddressDto[] (camelCase). */
  private normalizeAddressList(res: unknown): CustomerAddressDto[] {
    const raw = (res != null && typeof res === 'object' ? res as Record<string, unknown> : {}) as Record<string, unknown>;
    const arr = raw.result ?? raw.body ?? raw;
    if (!Array.isArray(arr)) return [];
    return arr.map((item) => this.normalizeAddress(item));
  }

  private normalizeAddress(o: unknown): CustomerAddressDto {
    const a = (o != null && typeof o === 'object' ? o : {}) as Record<string, unknown>;
    return {
      id: String(a.id ?? a.Id ?? ''),
      userId: String(a.userId ?? a.UserId ?? ''),
      label: String(a.label ?? a.Label ?? ''),
      street: String(a.street ?? a.Street ?? ''),
      city: (a.city ?? a.City) as string | null | undefined,
      region: (a.region ?? a.Region) as string | null | undefined,
      postalCode: (a.postalCode ?? a.PostalCode) as string | null | undefined,
      country: (a.country ?? a.Country) as string | null | undefined,
      isDefaultShipping: !!(a.isDefaultShipping ?? a.IsDefaultShipping),
      isDefaultBilling: !!(a.isDefaultBilling ?? a.IsDefaultBilling),
    };
  }
}
