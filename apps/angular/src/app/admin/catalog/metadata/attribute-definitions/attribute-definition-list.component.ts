import { NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  AttributeDefinitionDataType,
  AttributeDefinitionDto,
  AttributeDefinitionGovernanceStatus,
  AttributeDefinitionService,
} from './attribute-definition.service';

@Component({
  selector: 'app-attribute-definition-list',
  standalone: true,
  imports: [RouterLink, LocalizationPipe, NgClass],
  templateUrl: './attribute-definition-list.component.html',
})
export class AttributeDefinitionListComponent implements OnInit {
  private readonly service = inject(AttributeDefinitionService);
  private readonly toaster = inject(ToasterService);

  items = signal<AttributeDefinitionDto[]>([]);
  loading = signal(true);
  deletingId = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getList().subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  deleteItem(item: AttributeDefinitionDto): void {
    if (!confirm('Delete this attribute definition?')) {
      return;
    }

    this.deletingId.set(item.id);
    this.service.delete(item.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toaster.success('ECommerce::Success', 'Success');
        this.load();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.toaster.error(err?.error?.error?.message || 'ECommerce::Error', 'Error');
      },
    });
  }

  getGovernanceStatusLabel(value: AttributeDefinitionGovernanceStatus): string {
    const map: Record<number, string> = {
      0: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Draft',
      1: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.PendingReview',
      2: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Published',
      3: 'Enum:ECommerce.Catalog.AttributeDefinitionGovernanceStatus.Archived',
    };
    return map[value] ?? 'ECommerce::Unknown';
  }

  governanceBadgeClass(status: AttributeDefinitionGovernanceStatus): string {
    switch (status) {
      case AttributeDefinitionGovernanceStatus.Draft:
        return 'bg-secondary';
      case AttributeDefinitionGovernanceStatus.PendingReview:
        return 'bg-warning text-dark';
      case AttributeDefinitionGovernanceStatus.Published:
        return 'bg-success';
      case AttributeDefinitionGovernanceStatus.Archived:
        return 'bg-dark';
      default:
        return 'bg-secondary';
    }
  }

  getDataTypeLabel(value: AttributeDefinitionDataType): string {
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
}
