import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/pagination';
import { CreateSiteRequest, Site, SiteQuery, UpdateSiteRequest } from '../models/site';
import { toHttpParams } from './http-params.util';

@Injectable({ providedIn: 'root' })
export class SiteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/sites`;

  list(query: SiteQuery = {}): Observable<PagedResult<Site>> {
    return this.http.get<PagedResult<Site>>(this.baseUrl, {
      params: toHttpParams(query as Record<string, unknown>)
    });
  }

  get(id: string): Observable<Site> {
    return this.http.get<Site>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateSiteRequest): Observable<Site> {
    return this.http.post<Site>(this.baseUrl, request);
  }

  update(id: string, request: UpdateSiteRequest): Observable<Site> {
    return this.http.put<Site>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
