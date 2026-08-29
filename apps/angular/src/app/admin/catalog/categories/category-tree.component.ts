import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { CategoryTreeDto } from './category.service';

@Component({
  selector: 'app-category-tree',
  standalone: true,
  imports: [RouterLink, CategoryTreeComponent],
  template: `
    <ul class="list-group list-group-flush">
      @for (node of nodes; track node.id) {
        <li class="list-group-item d-flex justify-content-between align-items-center">
          <div class="d-flex align-items-center flex-grow-1">
            @if (node.children?.length) {
              <span class="me-2 text-muted">{{ node.name }}</span>
              <span class="badge bg-secondary rounded-pill">{{ node.children.length }}</span>
            } @else {
              <span>{{ node.name }}</span>
            }
            <span class="text-muted small ms-2">/ {{ node.slug }}</span>
          </div>
          <div class="btn-group btn-group-sm">
            <a [routerLink]="['/admin/catalog/categories/edit', node.id]" class="btn btn-outline-primary">
              <i class="fa fa-edit"></i>
            </a>
            <button
              type="button"
              class="btn btn-outline-danger"
              [disabled]="deletingId !== null"
              (click)="delete.emit(node)"
            >
              @if (deletingId === node.id) {
                <span class="spinner-border spinner-border-sm"></span>
              } @else {
                <i class="fa fa-trash"></i>
              }
            </button>
          </div>
        </li>
        @if (node.children?.length) {
          <li class="list-group-item ps-4">
            <app-category-tree
              [nodes]="node.children"
              [deletingId]="deletingId"
              (delete)="delete.emit($event)"
            />
          </li>
        }
      }
    </ul>
  `,
})
export class CategoryTreeComponent {
  @Input() nodes: CategoryTreeDto[] = [];
  @Input() deletingId: string | null = null;
  @Output() delete = new EventEmitter<CategoryTreeDto>();
}
