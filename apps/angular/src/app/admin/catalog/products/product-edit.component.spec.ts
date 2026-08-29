import '@angular/compiler';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { ConfigStateService, SessionStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ProductEditComponent } from './product-edit.component';
import { ProductService } from './product.service';
import { CategoryService } from '../categories/category.service';
import { BrandService } from '../brands/brand.service';
import { BrandModelService } from '../brand-models/brand-model.service';

describe('ProductEditComponent', () => {
  const getAttributes = vi.fn();
  const getProductTypes = vi.fn();
  const getAttributeRequirementsByProductType = vi.fn();
  const create = vi.fn();
  const update = vi.fn();
  const get = vi.fn();
  const getMediaByProduct = vi.fn();
  const getCategories = vi.fn();
  const getBrands = vi.fn();
  const getBrandModels = vi.fn();
  const navigate = vi.fn();

  const requiredAttributes = [
    {
      attributeDefinitionId: 'attr-part-number',
      key: 'part_number',
      dataType: 0,
      allowedValuesJson: null,
      regexPattern: null,
      minValue: null,
      maxValue: null,
      displayOrder: 1,
      isRequired: true,
      isRecommended: false,
      conditionalAttributeKey: null,
      conditionalOperator: null,
      conditionalExpectedValue: null,
    },
    {
      attributeDefinitionId: 'attr-condition',
      key: 'condition',
      dataType: 0,
      allowedValuesJson: '["NEW","USED"]',
      regexPattern: null,
      minValue: null,
      maxValue: null,
      displayOrder: 2,
      isRequired: true,
      isRecommended: false,
      conditionalAttributeKey: null,
      conditionalOperator: null,
      conditionalExpectedValue: null,
    },
  ];

  const recommendedAttributes = [
    {
      attributeDefinitionId: 'attr-gtin',
      key: 'gtin_upc',
      dataType: 0,
      allowedValuesJson: null,
      regexPattern: '^\\d{12,14}$',
      minValue: null,
      maxValue: null,
      displayOrder: 3,
      isRequired: false,
      isRecommended: true,
      conditionalAttributeKey: null,
      conditionalOperator: null,
      conditionalExpectedValue: null,
    },
    {
      attributeDefinitionId: 'attr-fitment',
      key: 'fitment_type',
      dataType: 0,
      allowedValuesJson: '["UNIVERSAL","DIRECT_FIT"]',
      regexPattern: null,
      minValue: null,
      maxValue: null,
      displayOrder: 4,
      isRequired: false,
      isRecommended: true,
      conditionalAttributeKey: null,
      conditionalOperator: null,
      conditionalExpectedValue: null,
    },
  ];

  beforeEach(() => {
    getAttributes.mockReturnValue(of([]));
    getProductTypes.mockReturnValue(
      of([{ id: 'pt-auto', code: 'AUTO_PART', name: 'Auto Part', isActive: true }]),
    );
    getAttributeRequirementsByProductType.mockReturnValue(
      of({
        productTypeId: 'pt-auto',
        requiredAttributes,
        recommendedAttributes,
        conditionalAttributes: [],
      }),
    );
    create.mockReturnValue(of({ id: 'product-1' }));
    update.mockReturnValue(of({ id: 'product-1' }));
    get.mockReturnValue(
      of({
        id: 'product-1',
        productNumber: 'AP-100',
        name: 'Brake Pad',
        description: 'Auto part',
        categoryId: 'cat-1',
        brandId: 'brand-1',
        modelId: null,
        productTypeId: 'pt-auto',
        dynamicAttributes: {},
        isPublished: false,
        translations: [],
        variants: [
          {
            id: 'variant-1',
            productId: 'product-1',
            sku: 'AP-100-V1',
            price: 99.9,
            quantity: 10,
            reserved: 0,
            availableQuantity: 10,
            dynamicAttributes: {
              part_number: 'PN-123',
              condition: 'NEW',
              gtin_upc: '123456789012',
              fitment_type: 'DIRECT_FIT',
            },
            attributes: [],
          },
        ],
      }),
    );
    getMediaByProduct.mockReturnValue(of([]));
    getCategories.mockReturnValue(of([{ id: 'cat-1', name: 'Brakes' }]));
    getBrands.mockReturnValue(of([{ id: 'brand-1', name: 'Brand A' }]));
    getBrandModels.mockReturnValue(of([]));
    navigate.mockResolvedValue(true);
  });

  function configureProviders(productId?: string): ProductEditComponent {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        FormBuilder,
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(productId ? { id: productId } : {}),
            },
          },
        },
        {
          provide: Router,
          useValue: {
            navigate,
          },
        },
        {
          provide: ConfigStateService,
          useValue: {
            getOne: () => ({
              languages: [{ cultureName: 'en', displayName: 'English', isDefault: true }],
              currentCulture: { cultureName: 'en' },
            }),
          },
        },
        {
          provide: SessionStateService,
          useValue: {
            getLanguage$: () => of('en'),
          },
        },
        {
          provide: ProductService,
          useValue: {
            getAttributes,
            getProductTypes,
            getAttributeRequirementsByProductType,
            create,
            update,
            get,
            getMediaByProduct,
            uploadMedia: vi.fn(),
            updateMedia: vi.fn(),
            deleteMedia: vi.fn(),
            getMediaFileUrl: vi.fn().mockReturnValue('/media/mock'),
          },
        },
        {
          provide: CategoryService,
          useValue: {
            getList: getCategories,
          },
        },
        {
          provide: BrandService,
          useValue: {
            getList: getBrands,
          },
        },
        {
          provide: BrandModelService,
          useValue: {
            getListByBrandId: getBrandModels,
          },
        },
        {
          provide: ToasterService,
          useValue: {
            success: vi.fn(),
            error: vi.fn(),
          },
        },
      ],
    });

    return TestBed.runInInjectionContext(() => new ProductEditComponent());
  }

  it('supports create-edit-publish flow with AUTO_PART dynamic attributes', () => {
    const createComponent = configureProviders();
    createComponent.ngOnInit();
    createComponent.addTranslation();
    createComponent.translations.at(0).patchValue({ language: 'en', name: 'Brake Pad' });

    createComponent.form.patchValue({
      productNumber: 'AP-100',
      name: 'Brake Pad',
      brandId: 'brand-1',
      categoryId: 'cat-1',
      productTypeId: 'pt-auto',
      isPublished: false,
    });

    createComponent.getVariantAttributeControl(0, 'part_number').setValue('PN-123');
    createComponent.getVariantAttributeControl(0, 'condition').setValue('NEW');
    createComponent.getVariantAttributeControl(0, 'gtin_upc').setValue('123456789012');
    createComponent.getVariantAttributeControl(0, 'fitment_type').setValue('DIRECT_FIT');

    createComponent.variants.at(0).patchValue({
      sku: 'AP-100-V1',
      quantity: 10,
      price: 99.9,
    });

    createComponent.onSubmit();

    expect(create).toHaveBeenCalledOnce();
    expect(create).toHaveBeenCalledWith(
      expect.objectContaining({
        productTypeId: 'pt-auto',
        dynamicAttributes: {},
        isPublished: false,
        variants: [
          expect.objectContaining({
            sku: 'AP-100-V1',
            dynamicAttributes: {
              part_number: 'PN-123',
              condition: 'NEW',
              gtin_upc: '123456789012',
              fitment_type: 'DIRECT_FIT',
            },
          }),
        ],
      }),
    );
    expect(navigate).toHaveBeenCalledWith(['/admin/catalog/products/edit', 'product-1']);

    const editComponent = configureProviders('product-1');
    editComponent.ngOnInit();
    editComponent.getVariantAttributeControl(0, 'condition').setValue('USED');
    editComponent.form.patchValue({ isPublished: true });

    editComponent.onSubmit();

    expect(update).toHaveBeenCalledWith(
      'product-1',
      expect.objectContaining({
        productTypeId: 'pt-auto',
        dynamicAttributes: {},
        isPublished: true,
        variants: [
          expect.objectContaining({
            dynamicAttributes: expect.objectContaining({
              part_number: 'PN-123',
              condition: 'USED',
              gtin_upc: '123456789012',
              fitment_type: 'DIRECT_FIT',
            }),
          }),
        ],
      }),
    );
  });

  it('prefers localized admin labels and falls back when missing', () => {
    const component = configureProviders();
    component.ngOnInit();

    const localizedAttribute = {
      key: 'condition',
      displayName: 'Etat',
      fallbackDisplayName: 'Condition',
      description: 'Etat actuel du produit',
      fallbackDescription: 'Current product condition',
      localizedOptions: [
        { value: 'NEW', displayName: 'Neuf', fallbackDisplayName: 'New' },
        { value: 'USED', displayName: null, fallbackDisplayName: 'Used' },
      ],
    } as any;

    expect(component.getAttributeDisplayName(localizedAttribute)).toBe('Etat');
    expect(component.getAttributeHelpText(localizedAttribute)).toBe('Etat actuel du produit');
    expect(component.getOptionLabel(localizedAttribute, 'NEW')).toBe('Neuf');
    expect(component.getOptionLabel(localizedAttribute, 'USED')).toBe('Used');

    const fallbackOnlyAttribute = {
      key: 'fitment_type',
      displayName: null,
      fallbackDisplayName: 'Fitment Type',
      description: '',
      fallbackDescription: 'Compatibility type',
      localizedOptions: [{ value: 'DIRECT_FIT', displayName: '', fallbackDisplayName: 'Direct Fit' }],
    } as any;

    expect(component.getAttributeDisplayName(fallbackOnlyAttribute)).toBe('Fitment Type');
    expect(component.getAttributeHelpText(fallbackOnlyAttribute)).toBe('Compatibility type');
    expect(component.getOptionLabel(fallbackOnlyAttribute, 'DIRECT_FIT')).toBe('Direct Fit');
  });
});
