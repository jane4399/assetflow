import { HttpParams } from '@angular/common/http';

/**
 * Builds an HttpParams instance from a plain object, skipping null/undefined/empty
 * values so optional filters do not appear on the query string when unset.
 */
export function toHttpParams(query: Record<string, unknown>): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}
