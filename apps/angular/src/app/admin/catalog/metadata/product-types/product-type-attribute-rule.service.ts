import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ProductTypeAttributeRuleDto {
  id: string;
  productTypeId: string;
  attributeDefinitionId: string;
  displayOrder: number;
}

export interface UpdateProductTypeAttributeRuleDto {
  attributeDefinitionId: string;
  displayOrder: number;
}

@Injectable({ providedIn: 'root' })
export class ProductTypeAttributeRuleService {
  private readonly rest = inject(RestService);

  getListByProductType(productTypeId: string): Observable<ProductTypeAttributeRuleDto[]> {
    return this.rest.request<void, ProductTypeAttributeRuleDto[]>({
      method: 'GET',
      url: '/api/catalog/product-type-attribute-rule/list-by-product-type',
      params: { productTypeId },
    });
  }

  replaceForProductType(productTypeId: string, input: UpdateProductTypeAttributeRuleDto[]): Observable<void> {
    return this.rest.request<UpdateProductTypeAttributeRuleDto[], void>({
      method: 'POST',
      url: '/api/catalog/product-type-attribute-rule/replace-for-product-type',
      params: { productTypeId },
      body: input,
    });
  }
}
