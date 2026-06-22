import type { AuditableEntity, EntityStatus, PageQuery } from './api';

export interface UserItem extends AuditableEntity {
  userName: string;
  displayName?: string;
  email?: string;
  departmentId?: number | null;
  departmentName?: string | null;
  positionId?: number | null;
  positionName?: string | null;
  status: EntityStatus;
  isBuiltIn?: boolean;
  mustChangePassword?: boolean;
  roles?: string[];
  lastLoginAt?: string;
}

export interface UserUpsertRequest {
  userName?: string;
  displayName?: string;
  email?: string;
  departmentId?: number | null;
  positionId?: number | null;
  password?: string;
  status: EntityStatus;
}

export interface UpdateUserRolesRequest {
  roleIds: number[];
}

export interface UserQuery extends Partial<PageQuery> {
  status?: EntityStatus;
  roleId?: number;
  departmentId?: number;
  positionId?: number;
  createdFrom?: string;
  createdTo?: string;
}

export interface BatchUserRequest {
  userIds: number[];
}
