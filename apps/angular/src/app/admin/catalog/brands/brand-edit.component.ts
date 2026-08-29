import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfigStateService, LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BrandService, CreateBrandDto } from './brand.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageTranslation,
} from '../shared/translation-form.validation';

@Component({
  selector: 'app-brand-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './brand-edit.component.html',
  styleUrls: ['./brand-edit.component.scss'],
})
export class BrandEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly brandService = inject(BrandService);
  private readonly toaster = inject(ToasterService);

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    slug: ['', [Validators.maxLength(256)]],
    description: ['', [Validators.maxLength(1024)]],
    isActive: [true],
    translations: this.fb.array<FormGroup>([]),
  });

  get translations(): FormArray<FormGroup> {
    return this.form.get('translations') as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    const localization = this.configState.getOne('localization');
    const languageList = (localization?.languages ?? []) as Array<{
      cultureName?: string;
      displayName?: string;
      isDefault?: boolean;
    }>;
    this.languages.set(
      languageList
        .filter((lang) => !!lang.cultureName)
        .map((lang) => ({
          cultureName: lang.cultureName!,
          displayName: lang.displayName ?? lang.cultureName!,
        }))
    );
    const defaultLanguage =
      languageList.find((lang) => lang?.isDefault)?.cultureName ??
      localization?.currentCulture?.cultureName ??
      this.languages()[0]?.cultureName ??
      null;
    this.defaultLanguage.set(defaultLanguage);

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.brandService.get(id).subscribe({
        next: (dto) => {
          const rawDto = dto as unknown as Record<string, unknown>;
          this.form.patchValue({
            name: String(rawDto.name ?? rawDto.Name ?? ''),
            slug: String(rawDto.slug ?? rawDto.Slug ?? ''),
            description: String(rawDto.description ?? rawDto.Description ?? ''),
            isActive: Boolean(rawDto.isActive ?? rawDto.IsActive),
          });
          const translations =
            (rawDto.translations as Array<Record<string, unknown>> | undefined) ??
            (rawDto.Translations as Array<Record<string, unknown>> | undefined) ??
            [];
          this.setTranslations(
            translations.map((translation) => ({
              language: String(translation.language ?? translation.Language ?? ''),
              name: String(translation.name ?? translation.Name ?? ''),
              description: (() => {
                const value =
                  translation.description ?? translation.Description ?? null;
                return value == null ? null : String(value);
              })(),
            }))
          );
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.toaster.error(
            err?.error?.error?.message || 'ECommerce::Error',
            'Error',
          );
        },
      });
    } else {
      this.loading.set(false);
    }
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.hasDuplicateLanguages() || this.hasMissingDefaultLanguageTranslation() || this.saving()) {
      return;
    }
    const v = this.form.getRawValue();
    const input: CreateBrandDto = {
      name: v.name ?? '',
      slug: v.slug || null,
      description: v.description || null,
      isActive: v.isActive ?? true,
      translations: this.getValidTranslations(),
    };
    this.saving.set(true);
    const id = this.id();
    const req = id
      ? this.brandService.update(id, input)
      : this.brandService.create(input);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.toaster.success(
          id ? 'ECommerce::BrandUpdated' : 'ECommerce::BrandCreated',
          'Success',
        );
        this.router.navigate(['/admin/catalog/brands']);
      },
      error: (err) => {
        this.saving.set(false);
        this.toaster.error(
          err?.error?.error?.message || 'ECommerce::Error',
          'Error',
        );
      },
    });
  }

  addTranslation(): void {
    this.translations.push(this.createTranslationGroup());
  }

  removeTranslation(index: number): void {
    this.translations.removeAt(index);
  }

  hasDuplicateLanguages(): boolean {
    return hasDuplicateTranslationLanguages(this.translations.controls);
  }

  hasMissingDefaultLanguageTranslation(): boolean {
    return hasMissingDefaultLanguageTranslation(this.translations.controls, this.defaultLanguage());
  }

  private setTranslations(translations: Array<{ language: string; name: string; description?: string | null }>): void {
    this.translations.clear();
    for (const translation of translations) {
      this.translations.push(
        this.createTranslationGroup(
          translation.language,
          translation.name,
          translation.description ?? ''
        )
      );
    }
  }

  private getValidTranslations(): Array<{ language: string; name: string; description?: string | null }> {
    return this.translations.controls
      .map((control) => ({
        language: String(control.get('language')?.value ?? '').trim(),
        name: String(control.get('name')?.value ?? '').trim(),
        description: String(control.get('description')?.value ?? '').trim(),
      }))
      .filter((translation) => translation.language.length > 0 && translation.name.length > 0)
      .map((translation) => ({
        language: translation.language,
        name: translation.name,
        description: translation.description.length > 0 ? translation.description : null,
      }));
  }

  private createTranslationGroup(language = '', name = '', description = ''): FormGroup {
    return this.fb.group({
      language: [language, [Validators.required, Validators.maxLength(16)]],
      name: [name, [Validators.required, Validators.maxLength(256)]],
      description: [description, [Validators.maxLength(1024)]],
    });
  }
}

