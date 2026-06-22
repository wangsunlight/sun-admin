import type { AuditableEntity, EntityStatus } from './api';

export type MenuType = 'Directory' | 'Page' | 'Button';

export interface MenuItem extends AuditableEntity {
  parentId?: number | null;
  name: string;
  type: MenuType;
  routePath?: string;
  component?: string;
  icon?: string;
  permissionCode?: string;
  sortOrder: number;
  status: EntityStatus;
  isBuiltIn?: boolean;
  children?: MenuItem[];
}

export interface MenuUpsertRequest {
  parentId?: number | null;
  name: string;
  type: MenuType;
  routePath?: string;
  component?: string;
  icon?: string;
  permissionCode?: string;
  sortOrder: number;
  status?: EntityStatus;
}
