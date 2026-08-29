import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BreadcrumbService } from '../../shared/breadcrumbs/breadcrumb.service';
import { NewsletterService } from '../newsletter.service';
import {
  CustomerProfileService,
  CustomerProfileDto,
  CustomerAddressDto,
  CreateUpdateCustomerAddressDto,
} from './customer-profile.service';

@Component({
  selector: 'app-customer-profile',
  templateUrl: './customer-profile.component.html',
  imports: [ReactiveFormsModule, LocalizationPipe],
})
export class CustomerProfileComponent implements OnInit, OnDestroy {
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly newsletterService = inject(NewsletterService);
  private readonly profileService = inject(CustomerProfileService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);
  private readonly localization = inject(LocalizationService);

  profile = signal<CustomerProfileDto | null>(null);
  profileLoading = signal(true);
  editingProfile = signal(false);
  savingProfile = signal(false);
  profileForm = this.fb.group({
    displayName: ['', [Validators.required, Validators.maxLength(256)]],
    phoneNumber: ['', [Validators.maxLength(32)]],
  });

  addresses = signal<CustomerAddressDto[]>([]);
  addressesLoading = signal(true);
  showAddressForm = signal(false);
  editingAddressId = signal<string | null>(null);
  savingAddress = signal(false);
  addressForm = this.fb.group({
    label: ['', [Validators.maxLength(64)]],
    street: ['', [Validators.required, Validators.maxLength(512)]],
    city: ['', [Validators.maxLength(128)]],
    region: ['', [Validators.maxLength(128)]],
    postalCode: ['', [Validators.maxLength(32)]],
    country: ['', [Validators.maxLength(128)]],
    isDefaultShipping: [false],
    isDefaultBilling: [false],
  });

  newsletterSubscribed = signal(false);
  newsletterLoading = signal(true);
  newsletterActionInProgress = signal(false);

  ngOnInit() {
    this.breadcrumbService.setItems([
      { label: 'ECommerce::CustomerDashboard', route: '/my-account' },
      { label: 'ECommerce::CustomerProfile' },
    ]);
    this.loadProfile();
    this.loadAddresses();
    this.loadNewsletterStatus();
  }

  ngOnDestroy() {
    this.breadcrumbService.clear();
  }

  private loadProfile(): void {
    this.profileLoading.set(true);
    this.profileService.getMyProfile().subscribe({
      next: (p) => {
        this.profile.set(p);
        this.profileForm.patchValue({ displayName: p.displayName ?? '', phoneNumber: p.phoneNumber ?? '' });
        this.profileLoading.set(false);
      },
      error: () => {
        this.profile.set(null);
        this.profileLoading.set(false);
      },
    });
  }

  private loadAddresses(): void {
    this.addressesLoading.set(true);
    this.profileService.getMyAddresses().subscribe({
      next: (list) => {
        this.addresses.set(list);
        this.addressesLoading.set(false);
      },
      error: () => {
        this.addresses.set([]);
        this.addressesLoading.set(false);
      },
    });
  }

  startEditProfile(): void {
    const p = this.profile();
    this.profileForm.patchValue({ displayName: p?.displayName ?? '', phoneNumber: p?.phoneNumber ?? '' });
    this.editingProfile.set(true);
  }

  cancelEditProfile(): void {
    this.editingProfile.set(false);
  }

  saveProfile(): void {
    if (this.profileForm.invalid || this.savingProfile()) return;
    this.savingProfile.set(true);
    const v = this.profileForm.value;
    this.profileService.updateMyProfile({ displayName: v.displayName ?? '', phoneNumber: v.phoneNumber || null }).subscribe({
      next: (p) => {
        this.profile.set(p);
        this.editingProfile.set(false);
        this.savingProfile.set(false);
        this.toaster.success('ECommerce::ProfileUpdated');
      },
      error: () => this.savingProfile.set(false),
    });
  }

  addAddress(): void {
    this.editingAddressId.set(null);
    this.addressForm.reset({
      label: '',
      street: '',
      city: '',
      region: '',
      postalCode: '',
      country: '',
      isDefaultShipping: false,
      isDefaultBilling: false,
    });
    this.showAddressForm.set(true);
  }

  editAddress(addr: CustomerAddressDto): void {
    this.editingAddressId.set(addr.id);
    this.addressForm.patchValue({
      label: addr.label,
      street: addr.street,
      city: addr.city ?? '',
      region: addr.region ?? '',
      postalCode: addr.postalCode ?? '',
      country: addr.country ?? '',
      isDefaultShipping: addr.isDefaultShipping,
      isDefaultBilling: addr.isDefaultBilling,
    });
    this.showAddressForm.set(true);
  }

  cancelAddressForm(): void {
    this.showAddressForm.set(false);
    this.editingAddressId.set(null);
  }

  saveAddress(): void {
    if (this.addressForm.invalid || this.savingAddress()) return;
    const v = this.addressForm.value;
    const dto: CreateUpdateCustomerAddressDto = {
      label: v.label ?? '',
      street: v.street ?? '',
      city: v.city || null,
      region: v.region || null,
      postalCode: v.postalCode || null,
      country: v.country || null,
      isDefaultShipping: !!v.isDefaultShipping,
      isDefaultBilling: !!v.isDefaultBilling,
    };
    this.savingAddress.set(true);
    const id = this.editingAddressId();
    const req = id
      ? this.profileService.updateAddress(id, dto)
      : this.profileService.createAddress(dto);
    req.subscribe({
      next: () => {
        this.loadAddresses();
        this.cancelAddressForm();
        this.savingAddress.set(false);
        this.toaster.success(id ? 'ECommerce::AddressUpdated' : 'ECommerce::AddressCreated');
      },
      error: () => this.savingAddress.set(false),
    });
  }

  deleteAddress(id: string): void {
    if (!confirm(this.localization.instant('ECommerce::ConfirmDeleteAddress') || 'Delete this address?')) return;
    this.profileService.deleteAddress(id).subscribe({
      next: () => {
        this.loadAddresses();
        this.toaster.success('ECommerce::AddressDeleted');
      },
    });
  }

  private loadNewsletterStatus(): void {
    this.newsletterLoading.set(true);
    this.newsletterService.getMyStatus().subscribe({
      next: (res) => {
        this.newsletterSubscribed.set(res?.isSubscribed ?? false);
        this.newsletterLoading.set(false);
      },
      error: () => {
        this.newsletterSubscribed.set(false);
        this.newsletterLoading.set(false);
      },
    });
  }

  subscribeNewsletter(): void {
    if (this.newsletterActionInProgress()) return;
    this.newsletterActionInProgress.set(true);
    this.newsletterService.subscribe().subscribe({
      next: () => {
        this.newsletterActionInProgress.set(false);
        this.newsletterSubscribed.set(true);
        this.toaster.success('ECommerce::NewsletterSubscribeSuccess');
      },
      error: () => {
        this.newsletterActionInProgress.set(false);
        this.toaster.error('ECommerce::NewsletterEmailRequired');
      },
    });
  }

  unsubscribeNewsletter(): void {
    if (this.newsletterActionInProgress()) return;
    this.newsletterActionInProgress.set(true);
    this.newsletterService.unsubscribe().subscribe({
      next: () => {
        this.newsletterActionInProgress.set(false);
        this.newsletterSubscribed.set(false);
        this.toaster.success('ECommerce::NewsletterUnsubscribeSuccess');
      },
      error: () => {
        this.newsletterActionInProgress.set(false);
        this.toaster.error('ECommerce::Error');
      },
    });
  }
}
