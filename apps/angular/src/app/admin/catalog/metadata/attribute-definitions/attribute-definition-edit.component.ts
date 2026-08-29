import { Component, OnInit, computed, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfigStateService, LocalizationPipe, PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize, of, switchMap, take } from 'rxjs';
import {
  AttributeDefinitionDataType,
  AttributeDefinitionDto,
  AttributeDefinitionGovernanceStatus,
  AttributeDefinitionService,
  AttributeOptionTranslationDto,
  CreateAttributeDefinitionDto,
  SaveAttributeOptionTranslationsDto,
} from './attribute-definition.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageDisplayName,
  hasMissingDefaultLanguageTranslation,
} from '../../shared/translation-form.validation';

export function parseAllowedValuesJson(raw: string | null | undefined): string[] {
  const s = String(raw ?? '').trim();
  if (!s) {
    return [];
  }
  try {
    const arr = JSON.parse(s) as unknown;
    if (!Array.isArray(arr)) {
      return [];
    }
    const byLower = new Map<string, string>();
    for (const item of arr) {
      const t = String(item).trim();
      if (!t) {
        continue;
      }
      const k = t.toLowerCase();
      if (!byLower.has(k)) {
        byLower.set(k, t);
      }
    }
    return [...byLower.values()].sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }));
  } catch {
    return [];
  }
}

@Component({
  selector: 'app-attribute-definition-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './attribute-definition-edit.component.html',
})
export class AttributeDefinitionEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly service = inject(AttributeDefinitionService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly permission = inject(PermissionService);

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  governanceStatus = signal<AttributeDefinitionGovernanceStatus | null>(null);
  publishedVersion = signal(0);
  govWorkflowRunning = signal(false);
  canReviewGovernance = signal(false);
  canPublishGovernance = signal(false);
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  dataTypes = [
    { value: AttributeDefinitionDataType.Text, label: 'ECommerce::AttributeDataTypeText' },
    { value: AttributeDefinitionDataType.Number, label: 'ECommerce::AttributeDataTypeNumber' },
    { value: AttributeDefinitionDataType.Boolean, label: 'ECommerce::AttributeDataTypeBoolean' },
    { value: AttributeDefinitionDataType.Date, label: 'ECommerce::AttributeDataTypeDate' },
    { value: AttributeDefinitionDataType.Enum, label: 'ECommerce::AttributeDataTypeEnum' },
    { value: AttributeDefinitionDataType.Json, label: 'ECommerce::AttributeDataTypeJson' },
  ];

  form = this.fb.group({
    key: ['', [Validators.required, Validators.maxLength(128)]],
    dataType: [AttributeDefinitionDataType.Text, [Validators.required]],
    allowedValuesJson: [''],
    regexPattern: [''],
    minValue: [null as number | null],
    maxValue: [null as number | null],
    isRequired: [false],
    isRecommended: [false],
    translations: this.fb.array<FormGroup>([]),
  });

  /** One row per enum option (invariant value + translation rows). */
  optionRows = this.fb.array<FormGroup>([]);

  get translations(): FormArray<FormGroup> {
    return this.form.get('translations') as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.permission
      .getGrantedPolicy$('ECommerce.Catalog.AttributeDefinitions.Review')
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe((granted) => this.canReviewGovernance.set(granted));
    this.permission
      .getGrantedPolicy$('ECommerce.Catalog.AttributeDefinitions.Publish')
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe((granted) => this.canPublishGovernance.set(granted));

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

    this.form
      .get('dataType')!
      .valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncOptionRowsFromFormPreserving());

    this.form
      .get('allowedValuesJson')!
      .valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncOptionRowsFromFormPreserving());

    const routeId = this.route.snapshot.paramMap.get('id');
    if (!routeId) {
      this.loading.set(false);
      queueMicrotask(() => this.syncOptionRowsFromFormPreserving());
      return;
    }

    this.id.set(routeId);
    this.service.get(routeId).subscribe({
      next: (dto) => {
        this.governanceStatus.set(dto.governanceStatus);
        this.publishedVersion.set(dto.publishedVersion);
        if (dto.governanceStatus === AttributeDefinitionGovernanceStatus.Archived) {
          this.form.disable({ emitEvent: false });
        }

        this.form.patchValue({
          key: dto.key,
          dataType: dto.dataType,
          allowedValuesJson: dto.allowedValuesJson ?? '',
          regexPattern: dto.regexPattern ?? '',
          minValue: dto.minValue ?? null,
          maxValue: dto.maxValue ?? null,
          isRequired: dto.isRequired,
          isRecommended: dto.isRecommended,
        });
        const tr = dto.translations ?? [];
        this.setDefinitionTranslations(
          tr.map((t) => ({
            language: t.language,
            name: t.name,
            description: t.description ?? null,
          })),
        );

        if (dto.dataType === AttributeDefinitionDataType.Enum) {
          this.service.getOptionTranslations(routeId).subscribe({
            next: (opts) => {
              this.populateEnumOptionRowsFromServer(dto.allowedValuesJson, opts);
              this.loading.set(false);
            },
            error: (err) => {
              this.loading.set(false);
              this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
            },
          });
        } else {
          this.syncOptionRowsFromFormPreserving();
          this.loading.set(false);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    this.markOptionRowsTouched();
    if (
      this.form.invalid ||
      this.hasDuplicateLanguages() ||
      this.hasMissingDefaultLanguageTranslation() ||
      this.hasInvalidOptionTranslations() ||
      this.saving()
    ) {
      return;
    }

    const value = this.form.getRawValue();
    const input: CreateAttributeDefinitionDto = {
      key: value.key ?? '',
      dataType: (value.dataType ?? AttributeDefinitionDataType.Text) as AttributeDefinitionDataType,
      allowedValuesJson: value.allowedValuesJson?.trim() ? value.allowedValuesJson.trim() : null,
      regexPattern: value.regexPattern?.trim() ? value.regexPattern.trim() : null,
      minValue: value.minValue,
      maxValue: value.maxValue,
      isRequired: value.isRequired ?? false,
      isRecommended: value.isRecommended ?? false,
      translations: this.getValidDefinitionTranslations(),
    };

    this.saving.set(true);
    const currentId = this.id();
    const request = currentId ? this.service.update(currentId, input) : this.service.create(input);

    request
      .pipe(
        switchMap((dto) => {
          const defId = dto.id;
          const dt = (value.dataType ?? AttributeDefinitionDataType.Text) as AttributeDefinitionDataType;
          if (dt !== AttributeDefinitionDataType.Enum) {
            return of(null);
          }
          const body: SaveAttributeOptionTranslationsDto = this.buildSaveOptionTranslationsDto();
          return this.service.saveOptionTranslations(defId, body);
        }),
        finalize(() => this.saving.set(false)),
      )
      .subscribe({
        next: () => {
          this.toaster.success('ECommerce::Success', 'Success');
          this.router.navigate(['/admin/catalog/attribute-definitions']);
        },
        error: (err) => {
          this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
        },
      });
  }

  addTranslation(): void {
    this.translations.push(this.createDefinitionTranslationGroup());
  }

  removeTranslation(index: number): void {
    this.translations.removeAt(index);
  }

  addOptionTranslation(optionIndex: number): void {
    const row = this.optionRows.at(optionIndex);
    const ta = row.get('translations') as FormArray<FormGroup>;
    ta.push(this.createOptionTranslationGroup());
  }

  removeOptionTranslation(optionIndex: number, translationIndex: number): void {
    const row = this.optionRows.at(optionIndex);
    const ta = row.get('translations') as FormArray<FormGroup>;
    ta.removeAt(translationIndex);
  }

  optionTranslations(optionIndex: number): FormArray<FormGroup> {
    return this.optionRows.at(optionIndex).get('translations') as FormArray<FormGroup>;
  }

  isEnumDataType(): boolean {
    return this.form.getRawValue().dataType === AttributeDefinitionDataType.Enum;
  }

  governanceStatusEnum = AttributeDefinitionGovernanceStatus;

  getGovernanceStatusLabel(value: AttributeDefinitionGovernanceStatus): string {
    const map: Record<number, string> = {
      0: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Draft',
      1: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.PendingReview',
      2: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Published',
      3: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Archived',
    };
    return map[value] ?? 'ECommerce::Unknown';
  }

  submitForReview(): void {
    const id = this.id();
    if (!id || this.govWorkflowRunning()) {
      return;
    }
    this.govWorkflowRunning.set(true);
    this.service.submitForReview(id).subscribe({
      next: (dto) => this.applyGovernanceDto(dto),
      error: (err) => {
        this.govWorkflowRunning.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
      complete: () => this.govWorkflowRunning.set(false),
    });
  }

  rejectReview(): void {
    const id = this.id();
    if (!id || this.govWorkflowRunning()) {
      return;
    }
    this.govWorkflowRunning.set(true);
    this.service.rejectReview(id).subscribe({
      next: (dto) => this.applyGovernanceDto(dto),
      error: (err) => {
        this.govWorkflowRunning.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
      complete: () => this.govWorkflowRunning.set(false),
    });
  }

  publishDefinition(): void {
    const id = this.id();
    if (!id || this.govWorkflowRunning()) {
      return;
    }
    this.govWorkflowRunning.set(true);
    this.service.publish(id).subscribe({
      next: (dto) => this.applyGovernanceDto(dto),
      error: (err) => {
        this.govWorkflowRunning.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
      complete: () => this.govWorkflowRunning.set(false),
    });
  }

  archiveDefinition(): void {
    const id = this.id();
    if (!id || this.govWorkflowRunning()) {
      return;
    }

    if (!confirm('Archive this attribute definition? It will no longer be editable or used in catalog rules.')) {
      return;
    }
    this.govWorkflowRunning.set(true);
    this.service.archive(id).subscribe({
      next: (dto) => this.applyGovernanceDto(dto),
      error: (err) => {
        this.govWorkflowRunning.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
      complete: () => this.govWorkflowRunning.set(false),
    });
  }

  demoteToDraft(): void {
    const id = this.id();
    if (!id || this.govWorkflowRunning()) {
      return;
    }
    this.govWorkflowRunning.set(true);
    this.service.demoteToDraft(id).subscribe({
      next: (dto) => this.applyGovernanceDto(dto),
      error: (err) => {
        this.govWorkflowRunning.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
      complete: () => this.govWorkflowRunning.set(false),
    });
  }

  private applyGovernanceDto(dto: AttributeDefinitionDto): void {
    this.governanceStatus.set(dto.governanceStatus);
    this.publishedVersion.set(dto.publishedVersion);
    if (dto.governanceStatus === AttributeDefinitionGovernanceStatus.Archived) {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
    }
    this.toaster.success('ECommerce::Success', 'Success');
  }

  hasDuplicateLanguages(): boolean {
    return hasDuplicateTranslationLanguages(this.translations.controls);
  }

  hasMissingDefaultLanguageTranslation(): boolean {
    return hasMissingDefaultLanguageTranslation(this.translations.controls, this.defaultLanguage());
  }

  hasInvalidOptionTranslations(): boolean {
    if (!this.isEnumDataType()) {
      return false;
    }
    for (let i = 0; i < this.optionRows.length; i++) {
      const ta = this.optionRows.at(i).get('translations') as FormArray<FormGroup>;
      const ctrls = ta.controls;
      const anyFilled = ctrls.some((c) => {
        const lang = String(c.get('language')?.value ?? '').trim();
        const displayName = String(c.get('displayName')?.value ?? '').trim();
        return lang.length > 0 || displayName.length > 0;
      });
      if (!anyFilled) {
        continue;
      }
      if (hasDuplicateTranslationLanguages(ctrls)) {
        return true;
      }
      if (hasMissingDefaultLanguageDisplayName(ctrls, this.defaultLanguage())) {
        return true;
      }
    }
    return false;
  }

  private markOptionRowsTouched(): void {
    for (let i = 0; i < this.optionRows.length; i++) {
      const ta = this.optionRows.at(i).get('translations') as FormArray<FormGroup>;
      for (const c of ta.controls) {
        c.markAllAsTouched();
      }
    }
  }

  private setDefinitionTranslations(
    translations: Array<{ language: string; name: string; description?: string | null }>,
  ): void {
    this.translations.clear();
    for (const translation of translations) {
      this.translations.push(
        this.createDefinitionTranslationGroup(
          translation.language,
          translation.name,
          translation.description ?? '',
        ),
      );
    }
  }

  private getValidDefinitionTranslations(): Array<{
    language: string;
    name: string;
    description?: string | null;
  }> {
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

  private createDefinitionTranslationGroup(language = '', name = '', description = ''): FormGroup {
    return this.fb.group({
      language: [language, [Validators.required, Validators.maxLength(16)]],
      name: [name, [Validators.required, Validators.maxLength(256)]],
      description: [description, [Validators.maxLength(1024)]],
    });
  }

  private createOptionTranslationGroup(language = '', displayName = ''): FormGroup {
    return this.fb.group({
      language: [language, [Validators.required, Validators.maxLength(16)]],
      displayName: [displayName, [Validators.required, Validators.maxLength(256)]],
    });
  }

  private createOptionRowGroup(
    value: string,
    translations: Array<{ language: string; displayName: string }>,
  ): FormGroup {
    const arr = this.fb.array<FormGroup>([]);
    for (const t of translations) {
      arr.push(this.createOptionTranslationGroup(t.language, t.displayName));
    }
    return this.fb.group({
      value: [{ value, disabled: true }],
      translations: arr,
    });
  }

  private syncOptionRowsFromFormPreserving(): void {
    const dataType = this.form.getRawValue().dataType;
    if (dataType !== AttributeDefinitionDataType.Enum) {
      this.optionRows.clear();
      return;
    }
    const values = parseAllowedValuesJson(this.form.getRawValue().allowedValuesJson);
    const preserved = new Map<string, FormGroup>();
    for (let i = 0; i < this.optionRows.length; i++) {
      const g = this.optionRows.at(i) as FormGroup;
      const v = String(g.get('value')?.value ?? '').trim();
      if (v) {
        preserved.set(v.toLowerCase(), g);
      }
    }
    this.optionRows.clear();
    for (const v of values) {
      const existing = preserved.get(v.toLowerCase());
      if (existing) {
        this.optionRows.push(existing);
      } else {
        this.optionRows.push(this.createOptionRowGroup(v, []));
      }
    }
  }

  private populateEnumOptionRowsFromServer(
    allowedJson: string | null | undefined,
    server: AttributeOptionTranslationDto[],
  ): void {
    this.optionRows.clear();
    const values = parseAllowedValuesJson(allowedJson);
    for (const v of values) {
      const dto = server.find((o) => o.value.trim().toLowerCase() === v.toLowerCase());
      const items =
        dto?.translations?.map((t) => ({
          language: t.language,
          displayName: t.displayName,
        })) ?? [];
      this.optionRows.push(this.createOptionRowGroup(v, items));
    }
  }

  private buildSaveOptionTranslationsDto(): SaveAttributeOptionTranslationsDto {
    const values = parseAllowedValuesJson(this.form.getRawValue().allowedValuesJson);
    const options = values.map((v) => {
      const row = this.optionRows.controls.find(
        (r) => String(r.get('value')?.value ?? '').trim().toLowerCase() === v.toLowerCase(),
      );
      const ta = row?.get('translations') as FormArray<FormGroup> | undefined;
      const translations =
        !ta
          ? []
          : ta.controls
              .map((c) => ({
                language: String(c.get('language')?.value ?? '').trim(),
                displayName: String(c.get('displayName')?.value ?? '').trim(),
              }))
              .filter((t) => t.language.length > 0 && t.displayName.length > 0);
      return { value: v, translations };
    });
    return { options };
  }
}
