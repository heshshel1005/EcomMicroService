import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { CategoryEditComponent } from './category-edit.component';
import { CategoryService } from './category.service';

describe('CategoryEditComponent', () => {
  let component: CategoryEditComponent;
  const getList = vi.fn();
  const create = vi.fn();
  const update = vi.fn();
  const navigate = vi.fn();

  beforeEach(() => {
    getList.mockReturnValue(of([]));
    create.mockReturnValue(of({ id: 'new-id' }));
    update.mockReturnValue(of({ id: 'updated-id' }));
    navigate.mockResolvedValue(true);

    TestBed.configureTestingModule({
      providers: [
        FormBuilder,
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({}),
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
              languages: [
                { cultureName: 'en', displayName: 'English', isDefault: true },
                { cultureName: 'fr', displayName: 'French', isDefault: false },
              ],
              currentCulture: { cultureName: 'en' },
            }),
          },
        },
        {
          provide: CategoryService,
          useValue: {
            getList,
            get: vi.fn(),
            create,
            update,
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

    component = TestBed.runInInjectionContext(() => new CategoryEditComponent());
    component.ngOnInit();
  });

  it('prevents submit when translations contain duplicate languages', () => {
    component.form.patchValue({ name: 'Shoes', slug: 'shoes', displayOrder: 1 });
    component.addTranslation();
    component.addTranslation();
    component.translations.at(0).patchValue({ language: 'en', name: 'Shoes' });
    component.translations.at(1).patchValue({ language: 'en', name: 'Chaussures' });

    component.onSubmit();

    expect(component.hasDuplicateLanguages()).toBe(true);
    expect(create).not.toHaveBeenCalled();
  });

  it('creates category with valid translation rows only', () => {
    component.form.patchValue({ name: 'Shoes', slug: 'shoes', displayOrder: 5 });
    component.addTranslation();
    component.addTranslation();
    component.translations.at(0).patchValue({ language: 'en', name: 'Shoes' });
    component.translations.at(1).patchValue({ language: '  ', name: '  ' });

    component.onSubmit();

    expect(create).toHaveBeenCalledOnce();
    expect(create).toHaveBeenCalledWith({
      name: 'Shoes',
      slug: 'shoes',
      parentId: null,
      displayOrder: 5,
      translations: [{ language: 'en', name: 'Shoes' }],
    });
    expect(navigate).toHaveBeenCalledWith(['/admin/catalog/categories']);
  });
});
