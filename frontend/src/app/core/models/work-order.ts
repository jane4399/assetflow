import { PaginationQuery } from './pagination';

export type WorkOrderPriority = 'Low' | 'Medium' | 'High' | 'Critical';
export type WorkOrderStatus = 'Open' | 'InProgress' | 'OnHold' | 'Completed' | 'Cancelled';

export const WORK_ORDER_PRIORITIES: WorkOrderPriority[] = ['Low', 'Medium', 'High', 'Critical'];
export const WORK_ORDER_STATUSES: WorkOrderStatus[] = ['Open', 'InProgress', 'OnHold', 'Completed', 'Cancelled'];

export interface WorkOrder {
  id: string;
  title: string;
  description?: string | null;
  priority: WorkOrderPriority;
  status: WorkOrderStatus;
  assetId: string;
  assetName: string;
  assignedTechnicianId?: string | null;
  assignedTechnicianName?: string | null;
  dueDate?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateWorkOrderRequest {
  title: string;
  description?: string | null;
  priority: WorkOrderPriority;
  assetId: string;
  assignedTechnicianId?: string | null;
  dueDate?: string | null;
}

export interface UpdateWorkOrderRequest {
  title: string;
  description?: string | null;
  priority: WorkOrderPriority;
  status: WorkOrderStatus;
  assignedTechnicianId?: string | null;
  dueDate?: string | null;
}

export interface WorkOrderQuery extends PaginationQuery {
  status?: WorkOrderStatus;
  priority?: WorkOrderPriority;
  assetId?: string;
  assignedTechnicianId?: string;
  search?: string;
}
