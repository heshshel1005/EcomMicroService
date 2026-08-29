import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray, FormGroup } from '@angular/forms';
import { LocalizationPipe, ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { CategoryService, CategoryDto, CreateCategoryDto } from './category.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageTranslation,
} from '../shared/translation-form.validation';

@Component({
  selector: 'app-category-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './category-edit.component.html',
  styleUrls: ['./category-edit.component.scss'],
})
export class CategoryEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly categoryService = inject(CategoryService);
  private readonly toaster = inject(ToasterService);

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  parentOptions = signal<CategoryDto[]>([]);
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    slug: ['', [Validators.required, Validators.maxLength(256)]],
    parentId: [null as string | null],
    displayOrder: [0, [Validators.required]],
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

    this.categoryService.getList().subscribe({
      next: (list) => this.parentOptions.set(list),
      error: () => this.loading.set(false),
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.categoryService.get(id).subscribe({
        next: (dto) => {
          this.form.patchValue({
            name: dto.name,
            slug: dto.slug,
            parentId: dto.parentId ?? null,
            displayOrder: dto.displayOrder,
          });
          this.setTranslations(dto.translations ?? []);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.toaster.error('ECommerce::ErrorLoadingCategory', 'Error');
        },
      });
    } else {
      this.loading.set(false);
    }
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.hasDuplicateLanguages() || this.hasMissingDefaultLanguageTranslation() || this.saving()) return;
    const v = this.form.getRawValue();
    const input: CreateCategoryDto = {
      name: v.name ?? '',
      slug: v.slug ?? '',
      parentId: v.parentId ?? null,
      displayOrder: v.displayOrder ?? 0,
      translations: this.getValidTranslations(),
    };
    this.saving.set(true);
    const id = this.id();
    const req = id
      ? this.categoryService.update(id, input)
      : this.categoryService.create(input);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.toaster.success(id ? 'ECommerce::CategoryUpdated' : 'ECommerce::CategoryCreated', 'Success');
        this.router.navigate(['/admin/catalog/categories']);
      },
      error: (err) => {
        this.saving.set(false);
        this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
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
