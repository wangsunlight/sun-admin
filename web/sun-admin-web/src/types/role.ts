import type { AuditableEntity, EntityStatus } from './api';

export type RoleDataScope = 'All' | 'OwnDepartment';

export interface RoleItem extends AuditableEntity {
  name: string;
  code: string;
  description?: string;
  dataScope: RoleDataScope;
  status: EntityStatus;
  isBuiltIn?: boolean;
  userCount?: number;
  menuIds?: number[];
}

export interface RoleUpsertRequest {
  name: string;
  code: string;
  description?: string;
  dataScope: RoleDataScope;
  status: EntityStatus;
}

export interface UpdateRoleMenusRequest {
  menuIds: number[];
}
