import type { AuditableEntity, EntityStatus } from './api';

export interface PositionItem extends AuditableEntity {
  code: string;
  name: string;
  description?: string;
  sortOrder: number;
  status: EntityStatus;
  isBuiltIn?: boolean;
}

export interface PositionUpsertRequest {
  code: string;
  name: string;
  description?: string;
  sortOrder: number;
  status?: EntityStatus;
}
