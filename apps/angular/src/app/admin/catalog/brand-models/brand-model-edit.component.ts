import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfigStateService, LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { BrandService, BrandDto } from '../brands/brand.service';
import { BrandModelService, CreateBrandModelDto } from './brand-model.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageTranslation,
} from '../shared/translation-form.validation';

@Component({
  selector: 'app-brand-model-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './brand-model-edit.component.html',
  styleUrls: ['./brand-model-edit.component.scss'],
})
export class BrandModelEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly brandService = inject(BrandService);
  private readonly brandModelService = inject(BrandModelService);
  private readonly toaster = inject(ToasterService);

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  brands = signal<BrandDto[]>([]);
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  form = this.fb.group({
    brandId: ['', [Validators.required]],
    name: ['', [Validators.required, Validators.maxLength(256)]],
    code: ['', [Validators.maxLength(64)]],
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

    this.brandService.getList(true).subscribe({
      next: (list) => this.brands.set(list),
      error: () => {},
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.brandModelService.get(id).subscribe({
        next: (dto) => {
          this.form.patchValue({
            brandId: dto.brandId,
            name: dto.name,
            code: dto.code ?? '',
            isActive: dto.isActive,
          });
          this.setTranslations(dto.translations ?? []);
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
    const input: CreateBrandModelDto = {
      brandId: v.brandId ?? '',
      name: v.name ?? '',
      code: v.code || null,
      isActive: v.isActive ?? true,
      translations: this.getValidTranslations(),
    };
    this.saving.set(true);
    const id = this.id();
    const req = id
      ? this.brandModelService.update(id, input)
      : this.brandModelService.create(input);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.toaster.success(
          id ? 'ECommerce::ModelUpdated' : 'ECommerce::ModelCreated',
          'Success',
        );
        this.router.navigate(['/admin/catalog/models']);
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

  private setTranslations(translations: Array<{ language: string; name: string }>): void {
    this.translations.clear();
    for (const translation of translations) {
      this.translations.push(this.createTranslationGroup(translation.language, translation.name));
    }
  }

  private getValidTranslations(): Array<{ language: string; name: string }> {
    return this.translations.controls
      .map((control) => ({
        language: String(control.get('language')?.value ?? '').trim(),
        name: String(control.get('name')?.value ?? '').trim(),
      }))
      .filter((translation) => translation.language.length > 0 && translation.name.length > 0);
  }

  private createTranslationGroup(language = '', name = ''): FormGroup {
    return this.fb.group({
      language: [language, [Validators.required, Validators.maxLength(16)]],
      name: [name, [Validators.required, Validators.maxLength(256)]],
    });
  }
}

