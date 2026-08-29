import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { BrandEditComponent } from './brand-edit.component';
import { BrandService } from './brand.service';

describe('BrandEditComponent', () => {
  let component: BrandEditComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FormBuilder,
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'brand-1' }),
            },
          },
        },
        {
          provide: Router,
          useValue: {
            navigate: vi.fn(),
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
          provide: BrandService,
          useValue: {
            get: vi.fn().mockReturnValue(
              of({
                Name: 'Apple',
                Slug: 'apple',
                Description: 'Brand',
                IsActive: true,
                Translations: [
                  { Language: 'en', Name: 'Apple', Description: 'EN' },
                  { Language: 'fr', Name: 'Pomme', Description: 'FR' },
                ],
              })
            ),
            create: vi.fn(),
            update: vi.fn(),
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

    component = TestBed.runInInjectionContext(() => new BrandEditComponent());
  });

  it('loads PascalCase translation payload into form array', () => {
    component.ngOnInit();

    expect(component.form.get('name')?.value).toBe('Apple');
    expect(component.translations.length).toBe(2);
    expect(component.translations.at(0).get('language')?.value).toBe('en');
    expect(component.translations.at(1).get('name')?.value).toBe('Pomme');
  });
});
