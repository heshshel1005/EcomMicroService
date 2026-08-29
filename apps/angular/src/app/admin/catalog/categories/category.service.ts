import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

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
    return this.rest.request<void, CategoryTreeDto[]>({
      method: 'GET',
      url: '/api/catalog/category/tree',
    });
  }

  getList(): Observable<CategoryDto[]> {
    return this.rest.request<void, CategoryDto[]>({
      method: 'GET',
      url: '/api/catalog/category/list',
    });
  }

  get(id: string): Observable<CategoryDto> {
    return this.rest.request<void, CategoryDto>({
      method: 'GET',
      url: `/api/catalog/category/${id}`,
    });
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
