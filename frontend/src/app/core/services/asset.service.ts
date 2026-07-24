import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Asset, AssetQuery, CreateAssetRequest, UpdateAssetRequest } from '../models/asset';
import { PagedResult } from '../models/pagination';
import { toHttpParams } from './http-params.util';

@Injectable({ providedIn: 'root' })
export class AssetService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/assets`;

  list(query: AssetQuery = {}): Observable<PagedResult<Asset>> {
    return this.http.get<PagedResult<Asset>>(this.baseUrl, {
      params: toHttpParams(query as Record<string, unknown>)
    });
  }

  get(id: string): Observable<Asset> {
    return this.http.get<Asset>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateAssetRequest): Observable<Asset> {
    return this.http.post<Asset>(this.baseUrl, request);
  }

  update(id: string, request: UpdateAssetRequest): Observable<Asset> {
    return this.http.put<Asset>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
