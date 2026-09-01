import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PagedResult } from '../../core/models/pagination';
import {
  WORK_ORDER_PRIORITIES,
  WORK_ORDER_STATUSES,
  WorkOrder,
  WorkOrderPriority,
  WorkOrderQuery,
  WorkOrderStatus
} from '../../core/models/work-order';
import { AuthService } from '../../core/services/auth.service';
import { WorkOrderService } from '../../core/services/work-order.service';

@Component({
  selector: 'app-work-order-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  template: `
    <div style="display:flex; align-items:center; margin-bottom:16px;">
      <h1 style="margin:0;">Work Orders</h1>
      <span class="spacer"></span>
      <a routerLink="/work-orders/new"><button type="button">+ New work order</button></a>
    </div>

    <div class="card">
      <form class="toolbar" [formGroup]="filterForm" (ngSubmit)="applyFilters()">
        <div class="field">
          <label for="search">Search title</label>
          <input id="search" type="text" formControlName="search" placeholder="e.g. seal" />
        </div>
        <div class="field">
          <label for="status">Status</label>
          <select id="status" formControlName="status">
            <option value="">All</option>
            @for (s of statuses; track s) {
              <option [value]="s">{{ s }}</option>
            }
          </select>
        </div>
        <div class="field">
          <label for="priority">Priority</label>
          <select id="priority" formControlName="priority">
            <option value="">All</option>
            @for (p of priorities; track p) {
              <option [value]="p">{{ p }}</option>
            }
          </select>
        </div>
        <button type="submit">Apply</button>
      </form>

      @if (error()) {
        <div class="form-error">{{ error() }}</div>
      }

      @if (loading()) {
        <p class="muted">Loading…</p>
      } @else {
        @if (result(); as data) {
          @if (data.items.length === 0) {
            <p class="muted">No work orders match your filters.</p>
          } @else {
            <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Asset</th>
                <th>Priority</th>
                <th>Status</th>
                <th>Due</th>
                <th>Technician</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (wo of data.items; track wo.id) {
                <tr>
                  <td><a [routerLink]="['/work-orders', wo.id]">{{ wo.title }}</a></td>
                  <td>{{ wo.assetName }}</td>
                  <td><span class="badge priority-{{ wo.priority }}">{{ wo.priority }}</span></td>
                  <td><span class="badge status-{{ wo.status }}">{{ wo.status }}</span></td>
                  <td>{{ wo.dueDate ? (wo.dueDate | date: 'mediumDate') : '—' }}</td>
                  <td>{{ wo.assignedTechnicianName ?? '—' }}</td>
                  <td class="row-actions">
                    <a [routerLink]="['/work-orders', wo.id]">Edit</a>
                    @if (auth.isAdmin()) {
                      <a href="#" class="error" (click)="remove(wo.id, $event)">Delete</a>
                    }
                  </td>
                </tr>
              }
            </tbody>
            </table>

            <div class="pagination">
              <span class="muted">
                Page {{ data.page }} of {{ data.totalPages }} · {{ data.totalCount }} total
              </span>
              <button class="secondary" type="button" [disabled]="!data.hasPreviousPage" (click)="changePage(-1)">
                Prev
              </button>
              <button class="secondary" type="button" [disabled]="!data.hasNextPage" (click)="changePage(1)">
                Next
              </button>
            </div>
          }
        }
      }
    </div>
  `
})
export class WorkOrderListComponent implements OnInit {
  private readonly service = inject(WorkOrderService);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);

  readonly result = signal<PagedResult<WorkOrder> | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly statuses = WORK_ORDER_STATUSES;
  readonly priorities = WORK_ORDER_PRIORITIES;

  private page = 1;
  private readonly pageSize = 10;

  readonly filterForm = this.fb.nonNullable.group({
    search: '',
    status: '' as '' | WorkOrderStatus,
    priority: '' as '' | WorkOrderPriority
  });

  ngOnInit(): void {
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  changePage(delta: number): void {
    const current = this.result();
    if (!current) {
      return;
    }
    const next = this.page + delta;
    if (next < 1 || next > current.totalPages) {
      return;
    }
    this.page = next;
    this.load();
  }

  remove(id: string, event: Event): void {
    event.preventDefault();
    if (!confirm('Delete this work order?')) {
      return;
    }
    this.service.delete(id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Failed to delete the work order.')
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    const filters = this.filterForm.getRawValue();
    const query: WorkOrderQuery = {
      page: this.page,
      pageSize: this.pageSize,
      sortBy: 'createdAtUtc',
      sortDir: 'desc',
      search: filters.search || undefined,
      status: filters.status || undefined,
      priority: filters.priority || undefined
    };

    this.service.list(query).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load work orders.');
        this.loading.set(false);
      }
    });
  }
}
