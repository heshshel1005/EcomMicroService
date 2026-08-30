import {
  Component,
  inject,
  OnInit,
  signal,
  computed,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormArray,
  FormGroup,
  FormControl,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { ConfigStateService, LocalizationPipe, SessionStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  ProductService,
  ProductDto,
  ProductMediaDto,
  ProductTypeDto,
  ProductTypeAttributeRequirementItemDto,
  ProductTypeAttributeRequirementsDto,
} from './product.service';
import { CategoryService, CategoryDto } from '../categories/category.service';
import { BrandService, BrandDto } from '../brands/brand.service';
import { BrandModelService, BrandModelDto } from '../brand-models/brand-model.service';
import {
  hasDuplicateTranslationLanguages,
  hasMissingDefaultLanguageTranslation,
} from '../shared/translation-form.validation';
import { of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

const MEDIA_TYPE_IMAGE = 0;
const MEDIA_TYPE_VIDEO = 1;

@Component({
  selector: 'app-product-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LocalizationPipe],
  templateUrl: './product-edit.component.html',
  styleUrls: ['./product-edit.component.scss'],
})
export class ProductEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly configState = inject(ConfigStateService);
  private readonly sessionState = inject(SessionStateService);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly brandService = inject(BrandService);
  private readonly brandModelService = inject(BrandModelService);
  private readonly toaster = inject(ToasterService);

  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  id = signal<string | null>(null);
  isCreate = computed(() => this.id() === null);
  loading = signal(true);
  saving = signal(false);
  uploadingMedia = signal(false);
  categoryOptions = signal<CategoryDto[]>([]);
  brandOptions = signal<BrandDto[]>([]);
  modelOptions = signal<BrandModelDto[]>([]);
  productTypeOptions = signal<ProductTypeDto[]>([]);
  requiredAttributeDefinitions = signal<ProductTypeAttributeRequirementItemDto[]>([]);
  recommendedAttributeDefinitions = signal<ProductTypeAttributeRequirementItemDto[]>([]);
  dynamicAttributeServerErrors = signal<Record<string, string>>({});
  attributeOptions = signal<{ id: string; name: string }[]>([]);
  mediaList = signal<ProductMediaDto[]>([]);
  pendingMediaType = signal(MEDIA_TYPE_IMAGE);
  languages = signal<{ cultureName: string; displayName: string }[]>([]);
  defaultLanguage = signal<string | null>(null);

  form = this.fb.group({
    productNumber: ['', [Validators.required, Validators.maxLength(64)]],
    name: ['', [Validators.required, Validators.maxLength(512)]],
    description: ['', [Validators.maxLength(4000)]],
    brandId: ['', [Validators.required]],
    modelId: [null as string | null],
    categoryId: [null as string | null],
    productTypeId: [null as string | null],
    dynamicAttributes: this.fb.group({}),
    isPublished: [false],
    translations: this.fb.array<FormGroup>([]),
    variants: this.fb.array<ReturnType<typeof this.createVariantGroup>>([]),
  });

  get variants(): FormArray {
    return this.form.get('variants') as FormArray;
  }

  get translations(): FormArray<FormGroup> {
    return this.form.get('translations') as FormArray<FormGroup>;
  }

  get dynamicAttributesGroup(): FormGroup {
    return this.form.get('dynamicAttributes') as FormGroup;
  }

  createVariantGroup = () =>
    this.fb.group({
      id: [null as string | null],
      sku: ['', [Validators.required, Validators.maxLength(64)]],
      price: [null as number | null],
      quantity: [0, [Validators.required, Validators.min(0)]],
      dynamicAttributes: this.createVariantDynamicAttributesGroup(),
      attributes: this.fb.array<ReturnType<typeof this.createAttributeGroup>>([]),
    });

  createAttributeGroup = () =>
    this.fb.group({
      productAttributeId: ['', Validators.required],
      value: ['', [Validators.required, Validators.maxLength(128)]],
    });

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

    // Initially disable model selection until a brand (with models) is chosen
    this.form.get('modelId')?.disable({ emitEvent: false });
    this.brandService.getList(true).subscribe({
      next: (list) => this.brandOptions.set(list),
      error: () => {},
    });
    this.categoryService.getList().subscribe({
      next: (list) => this.categoryOptions.set(Array.isArray(list) ? list : []),
      error: () => {},
    });
    this.productService.getAttributes().subscribe({
      next: (list) => this.attributeOptions.set(list),
      error: () => {},
    });
    this.productService.getProductTypes().subscribe({
      next: (list) => this.productTypeOptions.set(list.filter((x) => x.isActive)),
      error: () => {},
    });

    this.form.get('brandId')?.valueChanges.subscribe((brandId) => {
      if (!brandId) {
        this.modelOptions.set([]);
        this.form.patchValue({ modelId: null }, { emitEvent: false });
        this.form.get('modelId')?.disable({ emitEvent: false });
        return;
      }
      this.brandModelService.getListByBrandId(brandId).subscribe({
        next: (list) => {
          this.modelOptions.set(list);
          const currentModelId = this.form.get('modelId')?.value;
          if (currentModelId && !list.some((m) => m.id === currentModelId)) {
            this.form.patchValue({ modelId: null }, { emitEvent: false });
          }
          if (list.length > 0) {
            this.form.get('modelId')?.enable({ emitEvent: false });
          } else {
            this.form.get('modelId')?.disable({ emitEvent: false });
          }
        },
        error: () => {
          this.modelOptions.set([]);
          this.form.patchValue({ modelId: null }, { emitEvent: false });
          this.form.get('modelId')?.disable({ emitEvent: false });
        },
      });
    });

    this.form.get('productTypeId')?.valueChanges.subscribe((productTypeId) => {
      this.loadAttributeRequirements(productTypeId);
    });
    this.sessionState.getLanguage$().subscribe(() => {
      const selectedProductTypeId = this.form.get('productTypeId')?.value;
      if (!selectedProductTypeId) {
        return;
      }
      this.loadAttributeRequirements(selectedProductTypeId, this.dynamicAttributesGroup.getRawValue() as Record<string, unknown>);
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.productService
        .get(id)
        .pipe(
          switchMap((dto) => {
            const productTypeId = dto.productTypeId;
            if (!productTypeId) {
              return of({
                dto,
                requirements: null as ProductTypeAttributeRequirementsDto | null,
              });
            }
            return this.productService.getAttributeRequirementsByProductType(productTypeId).pipe(
              map((requirements) => ({ dto, requirements })),
              catchError(() => of({ dto, requirements: null as ProductTypeAttributeRequirementsDto | null })),
            );
          }),
        )
        .subscribe({
          next: ({ dto, requirements }) => {
            if (requirements) {
              this.requiredAttributeDefinitions.set(requirements.requiredAttributes ?? []);
              this.recommendedAttributeDefinitions.set(requirements.recommendedAttributes ?? []);
              this.rebuildDynamicAttributeControls(
                requirements.requiredAttributes ?? [],
                requirements.recommendedAttributes ?? [],
                dto.dynamicAttributes ?? {},
              );
            } else {
              this.requiredAttributeDefinitions.set([]);
              this.recommendedAttributeDefinitions.set([]);
              this.rebuildDynamicAttributeControls([], []);
            }

            this.form.patchValue({
              productNumber: dto.productNumber,
              name: dto.name,
              description: dto.description ?? '',
              brandId: dto.brandId,
              modelId: dto.modelId ?? null,
              categoryId: dto.categoryId ?? null,
              productTypeId: dto.productTypeId ?? null,
              isPublished: dto.isPublished,
            });
            this.setTranslations(dto.translations ?? []);
            this.variants.clear();
            for (const v of dto.variants) {
              const attrArray = this.fb.array(
                v.attributes.map((a) =>
                  this.fb.group({
                    productAttributeId: [a.productAttributeId, Validators.required],
                    value: [a.value, [Validators.required, Validators.maxLength(128)]],
                  }),
                ),
              );
              this.variants.push(
                this.fb.group({
                  id: [v.id],
                  sku: [v.sku, [Validators.required, Validators.maxLength(64)]],
                  price: [v.price ?? null],
                  quantity: [v.quantity, [Validators.required, Validators.min(0)]],
                  dynamicAttributes: this.createVariantDynamicAttributesGroup(v.dynamicAttributes ?? {}),
                  attributes: attrArray,
                }),
              );
            }
            this.loading.set(false);
            if (dto.brandId) {
              this.brandModelService.getListByBrandId(dto.brandId).subscribe({
                next: (list) => {
                  this.modelOptions.set(list);
                  if (list.length > 0) {
                    this.form.get('modelId')?.enable({ emitEvent: false });
                  } else {
                    this.form.get('modelId')?.disable({ emitEvent: false });
                  }
                },
                error: () => {
                  this.modelOptions.set([]);
                  this.form.patchValue({ modelId: null }, { emitEvent: false });
                  this.form.get('modelId')?.disable({ emitEvent: false });
                },
              });
            } else {
              this.modelOptions.set([]);
              this.form.patchValue({ modelId: null }, { emitEvent: false });
              this.form.get('modelId')?.disable({ emitEvent: false });
            }
            this.loadMedia(id);
          },
          error: () => {
            this.loading.set(false);
            this.toaster.error('ECommerce::ErrorLoadingProduct', 'Error');
          },
        });
    } else {
      this.loading.set(false);
      this.addVariant();
    }
  }

  loadMedia(productId: string): void {
    this.productService.getMediaByProduct(productId).subscribe({
      next: (list) => this.mediaList.set(list),
      error: () => {},
    });
  }

  addVariant(): void {
    this.variants.push(this.createVariantGroup());
  }

  removeVariant(index: number): void {
    this.variants.removeAt(index);
  }

  getVariantAttributes(index: number): FormArray {
    return this.variants.at(index).get('attributes') as FormArray;
  }

  getVariantDynamicAttributesGroup(index: number): FormGroup {
    return this.variants.at(index).get('dynamicAttributes') as FormGroup;
  }

  getVariantAttributeControl(variantIndex: number, attributeKey: string): FormControl {
    return this.getVariantDynamicAttributesGroup(variantIndex).get(attributeKey) as FormControl;
  }

  addAttributeToVariant(variantIndex: number): void {
    this.getVariantAttributes(variantIndex).push(this.createAttributeGroup());
  }

  removeAttributeFromVariant(variantIndex: number, attrIndex: number): void {
    this.getVariantAttributes(variantIndex).removeAt(attrIndex);
  }

  onSubmit(): void {
    this.clearDynamicAttributeServerErrors();
    this.form.markAllAsTouched();
    if (this.form.invalid || this.hasDuplicateLanguages() || this.hasMissingDefaultLanguageTranslation() || this.saving()) return;
    const v = this.form.getRawValue();
    const productId = this.id();
    const payload = {
      productNumber: v.productNumber ?? '',
      name: v.name ?? '',
      description: v.description || null,
      brandId: v.brandId ?? '',
      modelId: v.modelId ?? null,
      categoryId: v.categoryId ?? null,
      productTypeId: v.productTypeId ?? null,
      dynamicAttributes: this.collectDynamicAttributesPayload(),
      isPublished: v.isPublished ?? false,
      translations: this.getValidTranslations(),
      variants: (v.variants ?? []).map((vr: {
        id?: string | null;
        sku?: string | null;
        price?: number | null;
        quantity?: number | null;
        dynamicAttributes?: Record<string, unknown> | null;
        attributes?: { productAttributeId?: string | null; value?: string | null }[] | null;
      }) => ({
        id: vr.id ?? null,
        sku: vr.sku ?? '',
        price: vr.price ?? null,
        quantity: vr.quantity ?? 0,
        dynamicAttributes: this.collectVariantDynamicAttributesPayload(vr.dynamicAttributes as Record<string, unknown> | undefined),
        attributes: (vr.attributes ?? []).map((a) => ({
          productAttributeId: a.productAttributeId ?? '',
          value: a.value ?? '',
        })),
      })),
    };
    this.saving.set(true);
    if (productId) {
      this.productService.update(productId, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.toaster.success('ECommerce::ProductUpdated', 'Success');
          this.router.navigate(['/admin/catalog/products']);
        },
        error: (err) => {
          this.saving.set(false);
          this.applyBackendValidationErrors(err);
          this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
        },
      });
    } else {
      this.productService.create(payload).subscribe({
        next: (created) => {
          this.saving.set(false);
          this.toaster.success('ECommerce::ProductCreated', 'Success');
          this.router.navigate(['/admin/catalog/products/edit', created.id]);
        },
        error: (err) => {
          this.saving.set(false);
          this.applyBackendValidationErrors(err);
          this.toaster.error(err?.error?.error?.message || 'Error', 'Error');
        },
      });
    }
  }

  triggerFileUpload(mediaType: number): void {
    this.pendingMediaType.set(mediaType);
    this.fileInput?.nativeElement?.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    const productId = this.id();
    if (!file || !productId) return;
    const mediaType = this.pendingMediaType();
    const isPrimary = this.mediaList().length === 0;
    this.uploadingMedia.set(true);
    this.productService.uploadMedia(productId, file, mediaType, this.mediaList().length, isPrimary).subscribe({
      next: () => {
        this.uploadingMedia.set(false);
        this.toaster.success('Success', 'Uploaded');
        this.loadMedia(productId);
      },
      error: (err) => {
        this.uploadingMedia.set(false);
        this.toaster.error(err?.error?.error?.message || 'Upload failed', 'Error');
      },
    });
  }

  setPrimaryMedia(media: ProductMediaDto): void {
    const productId = this.id();
    if (!productId) return;
    this.productService.updateMedia(media.id, { isPrimary: true, sortOrder: media.sortOrder, altText: media.altText ?? undefined }).subscribe({
      next: () => this.loadMedia(productId),
      error: (err) => this.toaster.error(err?.error?.error?.message || 'Error', 'Error'),
    });
  }

  deleteMedia(media: ProductMediaDto): void {
    if (!confirm('Delete this media?')) return;
    const productId = this.id();
    this.productService.deleteMedia(media.id).subscribe({
      next: () => {
        this.toaster.success('Deleted', 'Success');
        if (productId) this.loadMedia(productId);
      },
      error: (err) => this.toaster.error(err?.error?.error?.message || 'Error', 'Error'),
    });
  }

  getMediaFileUrl(id: string): string {
    return this.productService.getMediaFileUrl(id);
  }

  isImage(media: ProductMediaDto): boolean {
    return media.mediaType === MEDIA_TYPE_IMAGE;
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

  private setTranslations(
    translations: Array<{ language: string; name: string; description?: string | null }>
  ): void {
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

  private getValidTranslations(): Array<{
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

  private createTranslationGroup(language = '', name = '', description = ''): FormGroup {
    return this.fb.group({
      language: [language, [Validators.required, Validators.maxLength(16)]],
      name: [name, [Validators.required, Validators.maxLength(512)]],
      description: [description, [Validators.maxLength(4000)]],
    });
  }

  getAttributeControl(attributeKey: string): FormControl {
    return this.dynamicAttributesGroup.get(attributeKey) as FormControl;
  }

  getAllowedValues(attribute: ProductTypeAttributeRequirementItemDto): string[] {
    if (attribute.localizedOptions && attribute.localizedOptions.length > 0) {
      return attribute.localizedOptions
        .map((option) => option.value)
        .filter((value): value is string => typeof value === 'string' && value.trim().length > 0);
    }

    if (!attribute.allowedValuesJson) {
      return [];
    }

    try {
      const values = JSON.parse(attribute.allowedValuesJson) as unknown;
      if (!Array.isArray(values)) {
        return [];
      }

      return values.filter((value): value is string => typeof value === 'string' && value.trim().length > 0);
    } catch {
      return [];
    }
  }

  getAttributeInputType(attribute: ProductTypeAttributeRequirementItemDto): string {
    switch (attribute.dataType) {
      case 1:
        return 'number';
      case 3:
        return 'date';
      default:
        return 'text';
    }
  }

  getAttributeDisplayName(attribute: ProductTypeAttributeRequirementItemDto): string {
    return attribute.displayName?.trim()
      || attribute.fallbackDisplayName?.trim()
      || attribute.key;
  }

  getAttributeHelpText(attribute: ProductTypeAttributeRequirementItemDto): string | null {
    return attribute.description?.trim()
      || attribute.fallbackDescription?.trim()
      || null;
  }

  getOptionLabel(attribute: ProductTypeAttributeRequirementItemDto, optionValue: string): string {
    const localizedOption = attribute.localizedOptions?.find((option) => option.value === optionValue);
    return localizedOption?.displayName?.trim()
      || localizedOption?.fallbackDisplayName?.trim()
      || optionValue;
  }

  getDynamicAttributeErrorMessage(attribute: ProductTypeAttributeRequirementItemDto, variantIndex = 0): string | null {
    const attributeDisplayName = this.getAttributeDisplayName(attribute);
    if (this.variants.length <= variantIndex) {
      return null;
    }
    const control = this.getVariantAttributeControl(variantIndex, attribute.key);
    if (!control || (!control.touched && !control.dirty)) {
      return null;
    }

    if (control.hasError('backend')) {
      const serverMessage = this.dynamicAttributeServerErrors()[attribute.key];
      return serverMessage ?? `Please provide a valid value for ${attributeDisplayName}.`;
    }
    if (control.hasError('required')) {
      return `${attributeDisplayName} is required.`;
    }
    if (control.hasError('allowedValues')) {
      return `${attributeDisplayName} must be one of the allowed values.`;
    }
    if (control.hasError('pattern')) {
      return `${attributeDisplayName} format is invalid.`;
    }
    if (control.hasError('min')) {
      return `${attributeDisplayName} must be greater than or equal to ${attribute.minValue}.`;
    }
    if (control.hasError('max')) {
      return `${attributeDisplayName} must be less than or equal to ${attribute.maxValue}.`;
    }
    return null;
  }

  private loadAttributeRequirements(productTypeId: string | null | undefined, existingValues?: Record<string, unknown>): void {
    if (!productTypeId) {
      this.requiredAttributeDefinitions.set([]);
      this.recommendedAttributeDefinitions.set([]);
      this.rebuildDynamicAttributeControls([], []);
      return;
    }

    this.productService.getAttributeRequirementsByProductType(productTypeId).subscribe({
      next: (requirements) => {
        const required = requirements?.requiredAttributes ?? [];
        const recommended = requirements?.recommendedAttributes ?? [];
        this.requiredAttributeDefinitions.set(required);
        this.recommendedAttributeDefinitions.set(recommended);
        this.rebuildDynamicAttributeControls(required, recommended, existingValues);
        this.rebuildVariantDynamicAttributeControls(required, recommended);
      },
      error: () => {
        this.requiredAttributeDefinitions.set([]);
        this.recommendedAttributeDefinitions.set([]);
        this.rebuildDynamicAttributeControls([], []);
        this.rebuildVariantDynamicAttributeControls([], []);
      },
    });
  }

  private createVariantDynamicAttributesGroup(existingValues?: Record<string, unknown>): FormGroup {
    const group = this.fb.group({});
    const allAttributes = [...this.requiredAttributeDefinitions(), ...this.recommendedAttributeDefinitions()];
    for (const attribute of allAttributes) {
      const validators = this.buildDynamicAttributeValidators(attribute);
      const existingValue = existingValues?.[attribute.key] ?? '';
      group.addControl(
        attribute.key,
        this.fb.control(this.normalizeDynamicControlValue(existingValue), validators),
      );
    }

    return group;
  }

  private rebuildVariantDynamicAttributeControls(
    requiredAttributes: ProductTypeAttributeRequirementItemDto[],
    recommendedAttributes: ProductTypeAttributeRequirementItemDto[],
  ): void {
    for (let i = 0; i < this.variants.length; i++) {
      const currentValues = this.getVariantDynamicAttributesGroup(i).getRawValue() as Record<string, unknown>;
      (this.variants.at(i) as FormGroup).setControl(
        'dynamicAttributes',
        this.createVariantDynamicAttributesGroup(currentValues),
      );
    }
  }

  private rebuildDynamicAttributeControls(
    _requiredAttributes: ProductTypeAttributeRequirementItemDto[],
    _recommendedAttributes: ProductTypeAttributeRequirementItemDto[],
    _existingValues?: Record<string, unknown>,
  ): void {
    this.clearDynamicAttributeServerErrors();
    for (const key of Object.keys(this.dynamicAttributesGroup.controls)) {
      this.dynamicAttributesGroup.removeControl(key);
    }
    // Dynamic attribute fields are only on variants in the UI. Duplicating the same
    // validated controls on the root form left it invalid when those hidden fields were empty.
  }

  private buildDynamicAttributeValidators(attribute: ProductTypeAttributeRequirementItemDto): ValidatorFn[] {
    const validators: ValidatorFn[] = [];
    if (attribute.isRequired) {
      validators.push(Validators.required);
    }
    if (attribute.regexPattern?.trim()) {
      validators.push(this.nonEmptyPatternValidator(attribute.regexPattern));
    }
    const allowedValues = this.getAllowedValues(attribute);
    if (allowedValues.length > 0) {
      validators.push(this.createAllowedValuesValidator(allowedValues));
    }
    if (attribute.dataType === 1) {
      if (attribute.minValue != null) {
        validators.push(this.optionalNumberMinValidator(attribute.minValue));
      }
      if (attribute.maxValue != null) {
        validators.push(this.optionalNumberMaxValidator(attribute.maxValue));
      }
    }
    return validators;
  }

  /** Pattern applies only when the user entered a value (recommended optional fields may stay empty). */
  private nonEmptyPatternValidator(patternStr: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const raw = control.value;
      if (raw === null || raw === undefined || String(raw).trim() === '') {
        return null;
      }
      let regex: RegExp;
      try {
        regex = new RegExp(patternStr);
      } catch {
        return { pattern: true };
      }
      return regex.test(String(raw)) ? null : { pattern: true };
    };
  }

  private optionalNumberMinValidator(min: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const raw = control.value;
      if (raw === null || raw === undefined || raw === '') {
        return null;
      }
      const n = typeof raw === 'number' ? raw : Number(raw);
      if (Number.isNaN(n)) {
        return { min: { min, actual: raw } };
      }
      return n >= min ? null : { min: { min, actual: n } };
    };
  }

  private optionalNumberMaxValidator(max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const raw = control.value;
      if (raw === null || raw === undefined || raw === '') {
        return null;
      }
      const n = typeof raw === 'number' ? raw : Number(raw);
      if (Number.isNaN(n)) {
        return { max: { max, actual: raw } };
      }
      return n <= max ? null : { max: { max, actual: n } };
    };
  }

  private createAllowedValuesValidator(allowedValues: string[]): ValidatorFn {
    const allowedSet = new Set(allowedValues);
    return (control) => {
      const rawValue = control.value;
      if (rawValue === null || rawValue === undefined || rawValue === '') {
        return null;
      }
      const value = String(rawValue);
      return allowedSet.has(value) ? null : { allowedValues: true };
    };
  }

  private normalizeDynamicControlValue(value: unknown): string | number | boolean | null {
    if (value === null || value === undefined) {
      return '';
    }
    if (typeof value === 'number' || typeof value === 'boolean') {
      return value;
    }
    return String(value);
  }

  private collectDynamicAttributesPayload(): Record<string, unknown> {
    const values = this.dynamicAttributesGroup.getRawValue() as Record<string, unknown>;
    const payload: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(values)) {
      if (value === null || value === undefined) {
        continue;
      }
      if (typeof value === 'string') {
        const trimmed = value.trim();
        if (trimmed.length > 0) {
          payload[key] = trimmed;
        }
        continue;
      }
      payload[key] = value;
    }
    return payload;
  }

  private collectVariantDynamicAttributesPayload(values?: Record<string, unknown>): Record<string, unknown> {
    if (!values) {
      return {};
    }
    const payload: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(values)) {
      if (value === null || value === undefined) {
        continue;
      }
      if (typeof value === 'string') {
        const trimmed = value.trim();
        if (trimmed.length > 0) {
          payload[key] = trimmed;
        }
        continue;
      }
      payload[key] = value;
    }
    return payload;
  }

  private clearDynamicAttributeServerErrors(): void {
    this.dynamicAttributeServerErrors.set({});
    for (const controlName of Object.keys(this.dynamicAttributesGroup.controls)) {
      const control = this.getAttributeControl(controlName);
      if (!control?.errors?.['backend']) {
        continue;
      }
      const { backend: _backend, ...remainingErrors } = control.errors;
      control.setErrors(Object.keys(remainingErrors).length > 0 ? remainingErrors : null);
    }
  }

  private applyBackendValidationErrors(error: unknown): void {
    const validationErrors = (error as { error?: { error?: { validationErrors?: Array<{ message?: string; members?: string[] }> } } })?.error?.error?.validationErrors;
    if (!validationErrors || validationErrors.length === 0) {
      return;
    }

    const controlNames = new Set(Object.keys(this.dynamicAttributesGroup.controls).map((key) => key.toLowerCase()));
    const serverErrors: Record<string, string> = {};

    for (const validationError of validationErrors) {
      const message = validationError?.message?.trim();
      if (!message) {
        continue;
      }

      for (const member of validationError.members ?? []) {
        const dynamicAttributeKey = this.extractDynamicAttributeKey(member, controlNames);
        if (!dynamicAttributeKey) {
          continue;
        }

        serverErrors[dynamicAttributeKey] = message;
        const control = this.getAttributeControl(dynamicAttributeKey);
        const existingErrors = control.errors ?? {};
        control.setErrors({ ...existingErrors, backend: true });
        control.markAsTouched();
      }
    }

    if (Object.keys(serverErrors).length > 0) {
      this.dynamicAttributeServerErrors.set(serverErrors);
    }
  }

  private extractDynamicAttributeKey(memberPath: string, dynamicControlNames: Set<string>): string | null {
    if (!memberPath) {
      return null;
    }

    const normalizedParts = memberPath
      .replace(/^\$\./, '')
      .replace(/\[(\w+)\]/g, '.$1')
      .split('.')
      .filter((part) => part.length > 0)
      .map((part) => part.trim());

    for (let i = 0; i < normalizedParts.length; i++) {
      if (normalizedParts[i].toLowerCase() !== 'dynamicattributes') {
        continue;
      }
      const key = normalizedParts[i + 1];
      if (!key) {
        return null;
      }

      const matchingKey = Object.keys(this.dynamicAttributesGroup.controls).find(
        (controlKey) => controlKey.toLowerCase() === key.toLowerCase(),
      );
      return matchingKey ?? null;
    }

    const lastSegment = normalizedParts.at(-1);
    if (!lastSegment || !dynamicControlNames.has(lastSegment.toLowerCase())) {
      return null;
    }

    return Object.keys(this.dynamicAttributesGroup.controls).find(
      (controlKey) => controlKey.toLowerCase() === lastSegment.toLowerCase(),
    ) ?? null;
  }
}
