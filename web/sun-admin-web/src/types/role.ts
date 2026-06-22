import type { AuditableEntity, EntityStatus } from './api';

export interface RoleItem extends AuditableEntity {
  name: string;
  code: string;
  description?: string;
  status: EntityStatus;
  isBuiltIn?: boolean;
  menuIds?: number[];
}

export interface RoleUpsertRequest {
  name: string;
  code: string;
  description?: string;
  status: EntityStatus;
}

export interface UpdateRoleMenusRequest {
  menuIds: number[];
}
