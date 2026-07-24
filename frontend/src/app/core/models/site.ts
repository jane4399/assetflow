import { PaginationQuery } from './pagination';

export interface Site {
  id: string;
  name: string;
  code: string;
  location?: string | null;
  assetCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateSiteRequest {
  name: string;
  code: string;
  location?: string | null;
}

export interface UpdateSiteRequest {
  name: string;
  code: string;
  location?: string | null;
}

export interface SiteQuery extends PaginationQuery {
  search?: string;
}
