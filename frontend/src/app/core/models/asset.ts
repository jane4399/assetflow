import { PaginationQuery } from './pagination';

export type AssetStatus = 'Operational' | 'Maintenance' | 'Offline' | 'Decommissioned';

export const ASSET_STATUSES: AssetStatus[] = ['Operational', 'Maintenance', 'Offline', 'Decommissioned'];

export interface Asset {
  id: string;
  name: string;
  tag: string;
  status: AssetStatus;
  siteId: string;
  siteName: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateAssetRequest {
  name: string;
  tag: string;
  status: AssetStatus;
  siteId: string;
}

export interface UpdateAssetRequest {
  name: string;
  tag: string;
  status: AssetStatus;
  siteId: string;
}

export interface AssetQuery extends PaginationQuery {
  status?: AssetStatus;
  siteId?: string;
  search?: string;
}
