import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/pagination';
import {
  CreateWorkOrderRequest,
  UpdateWorkOrderRequest,
  WorkOrder,
  WorkOrderQuery
} from '../models/work-order';
import { toHttpParams } from './http-params.util';

@Injectable({ providedIn: 'root' })
export class WorkOrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/workorders`;

  list(query: WorkOrderQuery = {}): Observable<PagedResult<WorkOrder>> {
    return this.http.get<PagedResult<WorkOrder>>(this.baseUrl, {
      params: toHttpParams(query as Record<string, unknown>)
    });
  }

  get(id: string): Observable<WorkOrder> {
    return this.http.get<WorkOrder>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateWorkOrderRequest): Observable<WorkOrder> {
    return this.http.post<WorkOrder>(this.baseUrl, request);
  }

  update(id: string, request: UpdateWorkOrderRequest): Observable<WorkOrder> {
    return this.http.put<WorkOrder>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
