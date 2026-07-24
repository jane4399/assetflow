import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Asset } from '../../core/models/asset';
import {
  WORK_ORDER_PRIORITIES,
  WORK_ORDER_STATUSES,
  WorkOrder,
  WorkOrderPriority,
  WorkOrderStatus
} from '../../core/models/work-order';
import { AssetService } from '../../core/services/asset.service';
import { WorkOrderService } from '../../core/services/work-order.service';

@Component({
  selector: 'app-work-order-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div style="max-width: 640px;">
      <p><a routerLink="/work-orders">&larr; Back to work orders</a></p>
      <h1>{{ isNew() ? 'New work order' : 'Edit work order' }}</h1>

      @if (error()) {
        <div class="form-error">{{ error() }}</div>
      }

      @if (loading()) {
        <p class="muted">Loading…</p>
      } @else {
        <form class="card" [formGroup]="form" (ngSubmit)="submit()">
          <div class="field">
            <label for="title">Title</label>
            <input id="title" type="text" formControlName="title" />
            @if (form.controls.title.touched && form.controls.title.invalid) {
              <div class="error">Title is required (max 200 characters).</div>
            }
          </div>

          <div class="field">
            <label for="description">Description</label>
            <textarea id="description" rows="3" formControlName="description"></textarea>
          </div>

          <div class="field">
            <label for="asset">Asset</label>
            @if (isNew()) {
              <select id="asset" formControlName="assetId">
                <option value="">Select an asset…</option>
                @for (asset of assets(); track asset.id) {
                  <option [value]="asset.id">{{ asset.name }} ({{ asset.tag }})</option>
                }
              </select>
              @if (form.controls.assetId.touched && form.controls.assetId.invalid) {
                <div class="error">Please choose an asset.</div>
              }
            } @else {
              <input id="asset" type="text" [value]="current()?.assetName ?? ''" disabled />
            }
          </div>

          <div class="field">
            <label for="priority">Priority</label>
            <select id="priority" formControlName="priority">
              @for (p of priorities; track p) {
                <option [value]="p">{{ p }}</option>
              }
            </select>
          </div>

          @if (!isNew()) {
            <div class="field">
              <label for="status">Status</label>
              <select id="status" formControlName="status">
                @for (s of statuses; track s) {
                  <option [value]="s">{{ s }}</option>
                }
              </select>
            </div>
          }

          <div class="field">
            <label for="dueDate">Due date</label>
            <input id="dueDate" type="date" formControlName="dueDate" />
          </div>

          <div class="row-actions">
            <button type="submit" [disabled]="saving()">
              {{ saving() ? 'Saving…' : isNew() ? 'Create work order' : 'Save changes' }}
            </button>
            <a routerLink="/work-orders"><button type="button" class="secondary">Cancel</button></a>
          </div>
        </form>
      }
    </div>
  `
})
export class WorkOrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly workOrders = inject(WorkOrderService);
  private readonly assetService = inject(AssetService);

  readonly id = signal<string | null>(null);
  readonly isNew = computed(() => this.id() === null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly assets = signal<Asset[]>([]);
  readonly current = signal<WorkOrder | null>(null);

  readonly statuses = WORK_ORDER_STATUSES;
  readonly priorities = WORK_ORDER_PRIORITIES;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    priority: ['Medium' as WorkOrderPriority, [Validators.required]],
    status: ['Open' as WorkOrderStatus, [Validators.required]],
    assetId: ['', [Validators.required]],
    dueDate: ['']
  });

  ngOnInit(): void {
    const routeId = this.route.snapshot.paramMap.get('id');
    this.id.set(routeId);

    if (routeId) {
      this.loadWorkOrder(routeId);
    } else {
      this.loadAssets();
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();
    const dueDate = value.dueDate ? new Date(value.dueDate).toISOString() : null;

    if (this.isNew()) {
      this.workOrders
        .create({
          title: value.title,
          description: value.description || null,
          priority: value.priority,
          assetId: value.assetId,
          assignedTechnicianId: null,
          dueDate
        })
        .subscribe({ next: () => this.onSaved(), error: (err: HttpErrorResponse) => this.onError(err) });
    } else {
      this.workOrders
        .update(this.id()!, {
          title: value.title,
          description: value.description || null,
          priority: value.priority,
          status: value.status,
          assignedTechnicianId: this.current()?.assignedTechnicianId ?? null,
          dueDate
        })
        .subscribe({ next: () => this.onSaved(), error: (err: HttpErrorResponse) => this.onError(err) });
    }
  }

  private loadAssets(): void {
    this.assetService.list({ pageSize: 100, sortBy: 'name', sortDir: 'asc' }).subscribe({
      next: (result) => this.assets.set(result.items),
      error: () => this.error.set('Failed to load assets.')
    });
  }

  private loadWorkOrder(id: string): void {
    this.loading.set(true);
    this.workOrders.get(id).subscribe({
      next: (workOrder) => {
        this.current.set(workOrder);
        this.form.patchValue({
          title: workOrder.title,
          description: workOrder.description ?? '',
          priority: workOrder.priority,
          status: workOrder.status,
          assetId: workOrder.assetId,
          dueDate: workOrder.dueDate ? workOrder.dueDate.substring(0, 10) : ''
        });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load the work order.');
        this.loading.set(false);
      }
    });
  }

  private onSaved(): void {
    void this.router.navigate(['/work-orders']);
  }

  private onError(error: HttpErrorResponse): void {
    this.saving.set(false);
    this.error.set(this.extractMessage(error));
  }

  private extractMessage(error: HttpErrorResponse): string {
    const problem = error.error;
    if (problem?.errors) {
      const firstGroup = Object.values(problem.errors)[0] as string[] | undefined;
      if (firstGroup && firstGroup.length > 0) {
        return firstGroup[0];
      }
    }
    return problem?.detail ?? 'Failed to save the work order.';
  }
}
