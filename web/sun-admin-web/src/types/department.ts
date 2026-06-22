import type { AuditableEntity, EntityStatus } from './api';

export interface DepartmentItem extends AuditableEntity {
  parentId?: number | null;
  code: string;
  name: string;
  leader?: string;
  phone?: string;
  email?: string;
  sortOrder: number;
  status: EntityStatus;
  isBuiltIn?: boolean;
  children?: DepartmentItem[];
}

export interface DepartmentUpsertRequest {
  parentId?: number | null;
  code: string;
  name: string;
  leader?: string;
  phone?: string;
  email?: string;
  sortOrder: number;
  status?: EntityStatus;
}
