import type { EntityStatus, PageQuery } from './api';

export interface DictionaryItem {
  id: number;
  dictionaryId: number;
  label: string;
  value: string;
  sortOrder: number;
  status: EntityStatus;
  isBuiltIn: boolean;
}

export interface DictionaryItemGroup {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  status: EntityStatus;
  isBuiltIn: boolean;
  createdAt: string;
  items: DictionaryItem[];
}

export interface DictionaryCreateRequest {
  code: string;
  name: string;
  description?: string | null;
}

export interface DictionaryUpdateRequest {
  name: string;
  description?: string | null;
  status: EntityStatus;
}

export interface DictionaryItemUpsertRequest {
  label: string;
  value: string;
  sortOrder: number;
  status: EntityStatus;
}

export interface NotificationItem {
  id: number;
  title: string;
  content: string;
  level: 'Info' | 'Success' | 'Warning' | 'Error';
  publishAt?: string | null;
  expiresAt?: string | null;
  isPinned: boolean;
  status: EntityStatus;
  createdAt: string;
}

export interface NotificationCreateRequest {
  title: string;
  content: string;
  level: NotificationItem['level'];
  publishAt?: string | null;
  expiresAt?: string | null;
  isPinned: boolean;
}

export interface NotificationUpdateRequest extends NotificationCreateRequest {
  status: EntityStatus;
}

export interface FileResourceItem {
  id: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  storageProvider: string;
  storagePath: string;
  uploadedBy?: number | null;
  createdAt: string;
}

export interface FileResourceCreateRequest {
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  storageProvider: string;
  storagePath: string;
}

export type ExportTaskStatus = 'Pending' | 'Running' | 'Succeeded' | 'Failed';

export interface ExportTaskItem {
  id: number;
  taskName: string;
  exportType: string;
  status: ExportTaskStatus;
  parametersJson?: string | null;
  filePath?: string | null;
  errorMessage?: string | null;
  createdByUserId: number;
  createdByUserName: string;
  createdAt: string;
  startedAt?: string | null;
  finishedAt?: string | null;
}

export interface ExportTaskCreateRequest {
  taskName: string;
  exportType: string;
  parametersJson?: string | null;
}

export interface CodeGenerationTemplateItem {
  id: number;
  name: string;
  templateKey: string;
  targetKind: string;
  content: string;
  status: EntityStatus;
  isBuiltIn: boolean;
  createdAt: string;
}

export interface CodeGenerationTemplateCreateRequest {
  name: string;
  templateKey: string;
  targetKind: string;
  content: string;
}

export interface CodeGenerationTemplateUpdateRequest {
  name: string;
  targetKind: string;
  content: string;
  status: EntityStatus;
}

export interface EntityChangeLogItem {
  id: number;
  entityName: string;
  entityId: string;
  changeType: string;
  changedBy?: number | null;
  changedByName?: string | null;
  changedFields?: string | null;
  beforeJson?: string | null;
  afterJson?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt: string;
}

export interface PlatformQuery extends Partial<PageQuery> {
  status?: EntityStatus;
}
