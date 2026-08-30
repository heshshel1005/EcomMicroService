import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface CategoryDto {
  id: string;
  parentId?: string | null;
  name: string;
  slug: string;
  displayOrder: number;
  translations?: CategoryTranslationDto[];
}

export interface CategoryTreeDto extends CategoryDto {
  children: CategoryTreeDto[];
}

export interface CreateCategoryDto {
  name: string;
  slug: string;
  parentId?: string | null;
  displayOrder: number;
  translations?: CategoryTranslationDto[];
}

export type UpdateCategoryDto = CreateCategoryDto;

export interface CategoryTranslationDto {
  language: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly rest = inject(RestService);

  getTree(): Observable<CategoryTreeDto[]> {
    return this.rest
      .request<void, unknown>(
        {
          method: 'GET',
          url: '/api/catalog/category/tree',
        },
        { skipHandleError: true },
      )
      .pipe(map((res) => unwrapList(res).map((item) => normalizeCategoryTree(item))));
  }

  getList(): Observable<CategoryDto[]> {
    return this.rest
      .request<void, unknown>(
        {
          method: 'GET',
          url: '/api/catalog/category/list',
        },
        { skipHandleError: true },
      )
      .pipe(map((res) => unwrapList(res).map((item) => normalizeCategory(item))));
  }

  get(id: string): Observable<CategoryDto> {
    return this.rest
      .request<void, unknown>(
        {
          method: 'GET',
          url: `/api/catalog/category/${id}`,
        },
        { skipHandleError: true },
      )
      .pipe(map((res) => normalizeCategory(unwrapPayload(res))));
  }

  create(input: CreateCategoryDto): Observable<CategoryDto> {
    return this.rest.request<CreateCategoryDto, CategoryDto>({
      method: 'POST',
      url: '/api/catalog/category',
      body: input,
    });
  }

  update(id: string, input: UpdateCategoryDto): Observable<CategoryDto> {
    return this.rest.request<UpdateCategoryDto, CategoryDto>({
      method: 'PUT',
      url: `/api/catalog/category/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.rest.request<void, void>({
      method: 'DELETE',
      url: `/api/catalog/category/${id}`,
    });
  }
}

function unwrapPayload(res: unknown): unknown {
  if (res == null || Array.isArray(res) || typeof res !== 'object') {
    return res;
  }
  const raw = res as Record<string, unknown>;
  return raw.result ?? raw.Result ?? raw.body ?? raw.Body ?? res;
}

function unwrapList(res: unknown): unknown[] {
  const payload = unwrapPayload(res);
  if (Array.isArray(payload)) {
    return payload;
  }
  if (payload != null && typeof payload === 'object') {
    const raw = payload as Record<string, unknown>;
    const items = raw.items ?? raw.Items;
    if (Array.isArray(items)) {
      return items;
    }
  }
  return [];
}

function normalizeTranslations(value: unknown): CategoryTranslationDto[] {
  if (!Array.isArray(value)) {
    return [];
  }
  return value.map((item) => {
    const row = (item != null && typeof item === 'object' ? item : {}) as Record<string, unknown>;
    return {
      language: String(row.language ?? row.Language ?? ''),
      name: String(row.name ?? row.Name ?? ''),
    };
  });
}

function normalizeCategory(item: unknown): CategoryDto {
  const row = (item != null && typeof item === 'object' ? item : {}) as Record<string, unknown>;
  const parentId = row.parentId ?? row.ParentId;
  return {
    id: String(row.id ?? row.Id ?? ''),
    parentId: parentId == null || parentId === '' ? null : String(parentId),
    name: String(row.name ?? row.Name ?? ''),
    slug: String(row.slug ?? row.Slug ?? ''),
    displayOrder: Number(row.displayOrder ?? row.DisplayOrder ?? 0),
    translations: normalizeTranslations(row.translations ?? row.Translations),
  };
}

function normalizeCategoryTree(item: unknown): CategoryTreeDto {
  const row = (item != null && typeof item === 'object' ? item : {}) as Record<string, unknown>;
  const children = row.children ?? row.Children;
  return {
    ...normalizeCategory(item),
    children: Array.isArray(children) ? children.map((child) => normalizeCategoryTree(child)) : [],
  };
}
