import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

/** Matches `OrganizationBusinessType` (Domain.Shared). */
export type OrganizationBusinessType = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9;

export interface OrganizationSignupSubmitDto {
  tenantName: string;
  displayName: string;
  legalName?: string | null;
  businessType: OrganizationBusinessType;
  website?: string | null;
  phone?: string | null;
  shortDescription?: string | null;
  logoUploadSessionId?: string | null;
  logoRelativePath?: string | null;
  adminEmail: string;
  adminUserName: string;
  adminDisplayName: string;
  adminPassword: string;
}

export interface OrganizationSignupSubmitResultDto {
  requestId: string;
  message: string;
}

export interface OrganizationSignupLogoUploadDto {
  uploadSessionId: string;
  relativePath: string;
}

/**
 * Anonymous organization signup API (host-scoped). Proxies `/api/saas/organization-signup/*`.
 */
@Injectable({ providedIn: 'root' })
export class OrganizationSignupPublicService {
  private readonly rest = inject(RestService);

  /**
   * Upload a logo file; returns session id and relative path required for submit (or omit both for no logo).
   * Multipart field name must be `file` (OpenAPI / ASP.NET Core binding).
   */
  uploadLogo(file: File): Observable<OrganizationSignupLogoUploadDto> {
    const form = new FormData();
    form.append('file', file);
    return this.rest
      .request<FormData, OrganizationSignupLogoUploadDto & Record<string, unknown>>({
        method: 'POST',
        url: '/api/saas/organization-signup/upload-logo',
        body: form,
      })
      .pipe(
        map((r) => ({
          uploadSessionId: String((r as { uploadSessionId?: string }).uploadSessionId ?? (r as { UploadSessionId?: string }).UploadSessionId ?? ''),
          relativePath: String((r as { relativePath?: string }).relativePath ?? (r as { RelativePath?: string }).RelativePath ?? ''),
        })),
      );
  }

  submit(input: OrganizationSignupSubmitDto): Observable<OrganizationSignupSubmitResultDto> {
    return this.rest
      .request<OrganizationSignupSubmitDto, OrganizationSignupSubmitResultDto & Record<string, unknown>>({
        method: 'POST',
        url: '/api/saas/organization-signup/submit',
        body: input,
      })
      .pipe(
        map((r) => ({
          requestId: String((r as { requestId?: string }).requestId ?? (r as { RequestId?: string }).RequestId ?? ''),
          message: String((r as { message?: string }).message ?? (r as { Message?: string }).Message ?? ''),
        })),
      );
  }
}
