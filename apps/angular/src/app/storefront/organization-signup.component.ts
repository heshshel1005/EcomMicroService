import { Component, inject, OnDestroy, OnInit, signal, ViewChild, ElementRef } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BreadcrumbService } from '../shared/breadcrumbs/breadcrumb.service';
import {
  OrganizationSignupPublicService,
  type OrganizationBusinessType,
} from './organization-signup-public.service';

function matchAdminPassword(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const parent = control.parent;
    if (!parent) return null;
    const pwd = (parent.get('adminPassword')?.value ?? '').toString();
    const v = (control.value ?? '').toString();
    if (!v.length) return null;
    return pwd === v ? null : { mismatch: true };
  };
}

@Component({
  selector: 'app-organization-signup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './organization-signup.component.html',
  styleUrls: ['./organization-signup.component.scss'],
})
export class OrganizationSignupComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly signupApi = inject(OrganizationSignupPublicService);
  private readonly toaster = inject(ToasterService);
  private readonly breadcrumbService = inject(BreadcrumbService);

  @ViewChild('logoFileInput') logoFileInput?: ElementRef<HTMLInputElement>;

  readonly businessTypeOptions: { value: OrganizationBusinessType; nameKey: string }[] = [
    { value: 0, nameKey: 'General' },
    { value: 1, nameKey: 'AutoParts' },
    { value: 2, nameKey: 'Clothing' },
    { value: 3, nameKey: 'Electronics' },
    { value: 4, nameKey: 'FoodAndBeverage' },
    { value: 5, nameKey: 'HomeAndGarden' },
    { value: 6, nameKey: 'HealthAndBeauty' },
    { value: 7, nameKey: 'Sports' },
    { value: 8, nameKey: 'Books' },
    { value: 9, nameKey: 'Other' },
  ];

  businessTypeLabelKey(nameKey: string): string {
    return `ECommerce::Enum:OrganizationBusinessType.${nameKey}`;
  }

  submitting = signal(false);
  logoUploading = signal(false);
  logoSessionId = signal<string | null>(null);
  logoRelativePath = signal<string | null>(null);
  logoFileName = signal<string | null>(null);
  submitResult = signal<{ requestId: string; message: string } | null>(null);
  showPassword = false;
  showPasswordConfirm = false;

  form = this.fb.group({
    tenantName: ['', [Validators.required, Validators.maxLength(64)]],
    displayName: ['', [Validators.required, Validators.maxLength(256)]],
    legalName: ['', [Validators.maxLength(256)]],
    businessType: [0 as OrganizationBusinessType, [Validators.required]],
    website: ['', [Validators.maxLength(512)]],
    phone: ['', [Validators.maxLength(32)]],
    shortDescription: ['', [Validators.maxLength(2000)]],
    adminEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    adminUserName: ['', [Validators.required, Validators.maxLength(256)]],
    adminDisplayName: ['', [Validators.required, Validators.maxLength(256)]],
    adminPassword: ['', [Validators.required, Validators.minLength(6)]],
    adminPasswordConfirm: ['', [Validators.required, matchAdminPassword()]],
  });

  ngOnInit(): void {
    this.breadcrumbService.setItems([{ label: 'ECommerce::OrganizationSignupPageTitle' }]);
    this.form.get('adminPassword')?.valueChanges.subscribe(() => {
      this.form.get('adminPasswordConfirm')?.updateValueAndValidity({ emitEvent: false });
    });
  }

  ngOnDestroy(): void {
    this.breadcrumbService.clear();
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.logoUploading.set(true);
    this.logoSessionId.set(null);
    this.logoRelativePath.set(null);
    this.logoFileName.set(null);

    this.signupApi.uploadLogo(file).subscribe({
      next: (dto) => {
        this.logoUploading.set(false);
        if (!dto.uploadSessionId || !dto.relativePath) {
          this.toaster.error('ECommerce::OrganizationSignupLogoUploadFailed');
          input.value = '';
          return;
        }
        this.logoSessionId.set(dto.uploadSessionId);
        this.logoRelativePath.set(dto.relativePath);
        this.logoFileName.set(file.name);
        this.toaster.success('ECommerce::OrganizationSignupLogoUploaded');
      },
      error: () => {
        this.logoUploading.set(false);
        input.value = '';
      },
    });
  }

  clearLogo(): void {
    this.logoSessionId.set(null);
    this.logoRelativePath.set(null);
    this.logoFileName.set(null);
    const el = this.logoFileInput?.nativeElement;
    if (el) el.value = '';
  }

  onSubmit(): void {
    if (this.submitResult() || this.submitting() || this.logoUploading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toaster.warn('ECommerce::OrganizationSignupValidationError');
      return;
    }

    const v = this.form.getRawValue();
    const session = this.logoSessionId();
    const path = this.logoRelativePath();
    const hasLogo = !!(session && path);

    const body = {
      tenantName: (v.tenantName ?? '').trim(),
      displayName: (v.displayName ?? '').trim(),
      legalName: this.trimOrNull(v.legalName),
      businessType: v.businessType as OrganizationBusinessType,
      website: this.trimOrNull(v.website),
      phone: this.trimOrNull(v.phone),
      shortDescription: this.trimOrNull(v.shortDescription),
      logoUploadSessionId: hasLogo ? session : null,
      logoRelativePath: hasLogo ? path : null,
      adminEmail: (v.adminEmail ?? '').trim(),
      adminUserName: (v.adminUserName ?? '').trim(),
      adminDisplayName: (v.adminDisplayName ?? '').trim(),
      adminPassword: (v.adminPassword ?? '').toString(),
    };

    this.submitting.set(true);
    this.signupApi.submit(body).subscribe({
      next: (res) => {
        this.submitting.set(false);
        this.submitResult.set({
          requestId: res.requestId,
          message: res.message,
        });
        this.toaster.success('ECommerce::OrganizationSignupSuccessTitle');
      },
      error: () => {
        this.submitting.set(false);
      },
    });
  }

  private trimOrNull(s: string | null | undefined): string | null {
    const t = (s ?? '').trim();
    return t.length ? t : null;
  }
}
