import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ASSET_STATUSES, Asset, AssetQuery, AssetStatus } from '../../core/models/asset';
import { PagedResult } from '../../core/models/pagination';
import { Site } from '../../core/models/site';
import { AssetService } from '../../core/services/asset.service';
import { AuthService } from '../../core/services/auth.service';
import { SiteService } from '../../core/services/site.service';

@Component({
  selector: 'app-asset-list',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div style="display:flex; align-items:center; margin-bottom:16px;">
      <h1 style="margin:0;">Assets</h1>
      <span class="spacer"></span>
      @if (auth.isAdmin()) {
        <button type="button" (click)="toggleCreate()">
          {{ showCreate() ? 'Close' : '+ New asset' }}
        </button>
      }
    </div>

    @if (showCreate() && auth.isAdmin()) {
      <div class="card" style="margin-bottom:16px;">
        <h3>Create asset</h3>
        @if (createError()) {
          <div class="form-error">{{ createError() }}</div>
        }
        <form class="toolbar" [formGroup]="createForm" (ngSubmit)="submitCreate()">
          <div class="field">
            <label for="c-name">Name</label>
            <input id="c-name" type="text" formControlName="name" />
          </div>
          <div class="field">
            <label for="c-tag">Tag</label>
            <input id="c-tag" type="text" formControlName="tag" />
          </div>
          <div class="field">
            <label for="c-status">Status</label>
            <select id="c-status" formControlName="status">
              @for (s of statuses; track s) {
                <option [value]="s">{{ s }}</option>
              }
            </select>
          </div>
          <div class="field">
            <label for="c-site">Site</label>
            <select id="c-site" formControlName="siteId">
              <option value="">Select…</option>
              @for (site of sites(); track site.id) {
                <option [value]="site.id">{{ site.name }}</option>
              }
            </select>
          </div>
          <button type="submit" [disabled]="creating()">Create</button>
        </form>
      </div>
    }

    <div class="card">
      <form class="toolbar" [formGroup]="filterForm" (ngSubmit)="applyFilters()">
        <div class="field">
          <label for="search">Search</label>
          <input id="search" type="text" formControlName="search" placeholder="name or tag" />
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
          <label for="site">Site</label>
          <select id="site" formControlName="siteId">
            <option value="">All</option>
            @for (site of sites(); track site.id) {
              <option [value]="site.id">{{ site.name }}</option>
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
            <p class="muted">No assets match your filters.</p>
          } @else {
            <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Tag</th>
                <th>Status</th>
                <th>Site</th>
              </tr>
            </thead>
            <tbody>
              @for (asset of data.items; track asset.id) {
                <tr>
                  <td>{{ asset.name }}</td>
                  <td>{{ asset.tag }}</td>
                  <td><span class="badge status-{{ asset.status }}">{{ asset.status }}</span></td>
                  <td>{{ asset.siteName }}</td>
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
export class AssetListComponent implements OnInit {
  private readonly assetService = inject(AssetService);
  private readonly siteService = inject(SiteService);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);

  readonly result = signal<PagedResult<Asset> | null>(null);
  readonly sites = signal<Site[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly showCreate = signal(false);
  readonly creating = signal(false);
  readonly createError = signal<string | null>(null);

  readonly statuses = ASSET_STATUSES;

  private page = 1;
  private readonly pageSize = 10;

  readonly filterForm = this.fb.nonNullable.group({
    search: '',
    status: '' as '' | AssetStatus,
    siteId: ''
  });

  readonly createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    tag: ['', [Validators.required, Validators.maxLength(100)]],
    status: ['Operational' as AssetStatus, [Validators.required]],
    siteId: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadSites();
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

  toggleCreate(): void {
    this.showCreate.update((value) => !value);
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creating.set(true);
    this.createError.set(null);

    this.assetService.create(this.createForm.getRawValue()).subscribe({
      next: () => {
        this.creating.set(false);
        this.showCreate.set(false);
        this.createForm.reset({ name: '', tag: '', status: 'Operational', siteId: '' });
        this.page = 1;
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.creating.set(false);
        this.createError.set(this.extractMessage(err));
      }
    });
  }

  private loadSites(): void {
    this.siteService.list({ pageSize: 100, sortBy: 'name', sortDir: 'asc' }).subscribe({
      next: (result) => this.sites.set(result.items),
      error: () => this.error.set('Failed to load sites.')
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    const filters = this.filterForm.getRawValue();
    const query: AssetQuery = {
      page: this.page,
      pageSize: this.pageSize,
      sortBy: 'name',
      sortDir: 'asc',
      search: filters.search || undefined,
      status: filters.status || undefined,
      siteId: filters.siteId || undefined
    };

    this.assetService.list(query).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load assets.');
        this.loading.set(false);
      }
    });
  }

  private extractMessage(error: HttpErrorResponse): string {
    const problem = error.error;
    if (problem?.errors) {
      const firstGroup = Object.values(problem.errors)[0] as string[] | undefined;
      if (firstGroup && firstGroup.length > 0) {
        return firstGroup[0];
      }
    }
    return problem?.detail ?? 'Failed to create the asset.';
  }
}
