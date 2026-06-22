import type { AuditableEntity, EntityStatus } from './api';

export interface UserItem extends AuditableEntity {
  userName: string;
  displayName?: string;
  email?: string;
  status: EntityStatus;
  isBuiltIn?: boolean;
  roles?: string[];
  lastLoginAt?: string;
}

export interface UserUpsertRequest {
  userName: string;
  displayName?: string;
  email?: string;
  password?: string;
  status: EntityStatus;
}

export interface UpdateUserRolesRequest {
  roleIds: number[];
}
