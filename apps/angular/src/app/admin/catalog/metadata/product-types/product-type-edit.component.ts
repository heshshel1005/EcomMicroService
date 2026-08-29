import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfigStateService, LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { CreateProductTypeDto, ProductTypeService } from './product-type.service';
import {
  AttributeDefinitionDto,
  AttributeDefinitionGovernanceStatus,
  AttributeDefinitionService,
} from '../attribute-definitions/attribute-definition.service';
import {
  ProductTypeAttributeRuleDto,
  ProductTypeAttributeRuleService,
  UpdateProductTypeAttributeRuleDto,
} from './product-type-attribute-rule.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageTranslation,
} from '../../shared/translation-form.validation';

@Component({
  selector: 'app-product-type-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './product-type-edit.component.html',
})
export class ProductTypeEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly service = inject(ProductTypeService);
  private readonly attributeDefinitionService = inject(AttributeDefinitionService);
  private readonly productTypeAttributeRuleService = inject(ProductTypeAttributeRuleService);
  private readonly toaster = inject(ToasterService);

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  savingRules = signal(false);
  definitions = signal<AttributeDefinitionDto[]>([]);
  rules = signal<ProductTypeAttributeRuleDto[]>([]);
  /** Definitions that may appear on product types (published only). */
  publishedDefinitionsForMapping = computed(() =>
    this.definitions().filter((d) => d.governanceStatus === AttributeDefinitionGovernanceStatus.Published),
  );
  unpublishedRulesPendingRemoval = computed(() => {
    const publishedIds = new Set(this.publishedDefinitionsForMapping().map((d) => d.id));
    return this.rules().filter((r) => !publishedIds.has(r.attributeDefinitionId));
  });
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(64)]],
    name: ['', [Validators.required, Validators.maxLength(256)]],
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
        })),
    );
    const defaultLanguage =
      languageList.find((lang) => lang?.isDefault)?.cultureName ??
      localization?.currentCulture?.cultureName ??
      this.languages()[0]?.cultureName ??
      null;
    this.defaultLanguage.set(defaultLanguage);

    this.attributeDefinitionService.getList().subscribe({
      next: (list) => this.definitions.set(list),
      error: () => this.definitions.set([]),
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      if (this.translations.length === 0) {
        const lang = this.defaultLanguage() ?? '';
        this.translations.push(this.createTranslationGroup(lang, ''));
      }
      this.loading.set(false);
      return;
    }

    this.id.set(id);
    this.service.get(id).subscribe({
      next: (dto) => {
        const rawDto = dto as unknown as Record<string, unknown>;
        this.form.patchValue({
          code: String(rawDto.code ?? rawDto.Code ?? ''),
          name: String(rawDto.name ?? rawDto.Name ?? ''),
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
          })),
        );
        this.loadRules(id);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (
      this.form.invalid ||
      this.hasDuplicateLanguages() ||
      this.hasMissingDefaultLanguageTranslation() ||
      this.saving()
    ) {
      return;
    }

    const value = this.form.getRawValue();
    const input: CreateProductTypeDto = {
      code: value.code ?? '',
      name: value.name ?? '',
      isActive: value.isActive ?? true,
      translations: this.getValidTranslations(),
    };

    this.saving.set(true);
    const id = this.id();
    const request = id ? this.service.update(id, input) : this.service.create(input);
    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.toaster.success('ECommerce::Success', 'Success');
        this.router.navigate(['/admin/catalog/product-types']);
      },
      error: (err) => {
        this.saving.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
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

  loadRules(productTypeId: string): void {
    this.productTypeAttributeRuleService.getListByProductType(productTypeId).subscribe({
      next: (list) => this.rules.set(list),
      error: () => this.rules.set([]),
    });
  }

  isDefinitionSelected(definitionId: string): boolean {
    return this.rules().some((x) => x.attributeDefinitionId === definitionId);
  }

  getDisplayOrder(definitionId: string): number {
    return this.rules().find((x) => x.attributeDefinitionId === definitionId)?.displayOrder ?? 0;
  }

  onToggleDefinition(definitionId: string, checked: boolean): void {
    const current = [...this.rules()];
    const existing = current.find((x) => x.attributeDefinitionId === definitionId);
    if (checked && !existing) {
      current.push({
        id: '',
        productTypeId: this.id() ?? '',
        attributeDefinitionId: definitionId,
        displayOrder: current.length,
      });
    } else if (!checked && existing) {
      const idx = current.findIndex((x) => x.attributeDefinitionId === definitionId);
      if (idx >= 0) {
        current.splice(idx, 1);
      }
    }
    this.rules.set(current);
  }

  onChangeDisplayOrder(definitionId: string, value: string): void {
    const order = Number(value);
    if (Number.isNaN(order) || order < 0) {
      return;
    }

    this.rules.set(
      this.rules().map((x) =>
        x.attributeDefinitionId === definitionId ? { ...x, displayOrder: order } : x,
      ),
    );
  }

  saveRules(): void {
    const productTypeId = this.id();
    if (!productTypeId || this.savingRules()) {
      return;
    }

    const publishedIds = new Set(this.publishedDefinitionsForMapping().map((d) => d.id));
    const payload: UpdateProductTypeAttributeRuleDto[] = this.rules()
      .filter((x) => publishedIds.has(x.attributeDefinitionId))
      .map((x) => ({
        attributeDefinitionId: x.attributeDefinitionId,
        displayOrder: x.displayOrder,
      }))
      .sort((a, b) => a.displayOrder - b.displayOrder);

    this.savingRules.set(true);
    this.productTypeAttributeRuleService.replaceForProductType(productTypeId, payload).subscribe({
      next: () => {
        this.savingRules.set(false);
        this.toaster.success('ECommerce::Success', 'Success');
        this.loadRules(productTypeId);
      },
      error: (err) => {
        this.savingRules.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  getDataTypeLabel(value: number): string {
    const map: Record<number, string> = {
      0: 'ECommerce::AttributeDataTypeText',
      1: 'ECommerce::AttributeDataTypeNumber',
      2: 'ECommerce::AttributeDataTypeBoolean',
      3: 'ECommerce::AttributeDataTypeDate',
      4: 'ECommerce::AttributeDataTypeEnum',
      5: 'ECommerce::AttributeDataTypeJson',
    };
    return map[value] ?? 'ECommerce::Unknown';
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
