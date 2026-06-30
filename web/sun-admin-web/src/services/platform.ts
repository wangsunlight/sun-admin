import request from './request';
import type { PageResult } from '../types/api';
import type {
  CodeGenerationTemplateCreateRequest,
  CodeGenerationTemplateItem,
  CodeGenerationTemplateUpdateRequest,
  DictionaryCreateRequest,
  DictionaryItem,
  DictionaryItemGroup,
  DictionaryItemUpsertRequest,
  DictionaryUpdateRequest,
  EntityChangeLogItem,
  ExportTaskCreateRequest,
  ExportTaskItem,
  FileResourceCreateRequest,
  FileResourceItem,
  NotificationCreateRequest,
  NotificationItem,
  NotificationUpdateRequest,
  PlatformQuery,
} from '../types/platform';

export const dictionaryService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<DictionaryItemGroup>, PageResult<DictionaryItemGroup>>('/api/dictionaries', { params });
  },
  create(payload: DictionaryCreateRequest) {
    return request.post<DictionaryItemGroup, DictionaryItemGroup>('/api/dictionaries', payload);
  },
  update(id: number, payload: DictionaryUpdateRequest) {
    return request.put<DictionaryItemGroup, DictionaryItemGroup>(`/api/dictionaries/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/dictionaries/${id}`);
  },
  createItem(dictionaryId: number, payload: DictionaryItemUpsertRequest) {
    return request.post<DictionaryItem, DictionaryItem>(`/api/dictionaries/${dictionaryId}/items`, payload);
  },
  updateItem(dictionaryId: number, itemId: number, payload: DictionaryItemUpsertRequest) {
    return request.put<DictionaryItem, DictionaryItem>(`/api/dictionaries/${dictionaryId}/items/${itemId}`, payload);
  },
  removeItem(dictionaryId: number, itemId: number) {
    return request.delete<void, void>(`/api/dictionaries/${dictionaryId}/items/${itemId}`);
  },
};

export const notificationService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<NotificationItem>, PageResult<NotificationItem>>('/api/notifications', { params });
  },
  create(payload: NotificationCreateRequest) {
    return request.post<NotificationItem, NotificationItem>('/api/notifications', payload);
  },
  update(id: number, payload: NotificationUpdateRequest) {
    return request.put<NotificationItem, NotificationItem>(`/api/notifications/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/notifications/${id}`);
  },
};

export const fileResourceService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<FileResourceItem>, PageResult<FileResourceItem>>('/api/files', { params });
  },
  create(payload: FileResourceCreateRequest) {
    return request.post<FileResourceItem, FileResourceItem>('/api/files', payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/files/${id}`);
  },
};

export const exportTaskService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<ExportTaskItem>, PageResult<ExportTaskItem>>('/api/exports', { params });
  },
  create(payload: ExportTaskCreateRequest) {
    return request.post<ExportTaskItem, ExportTaskItem>('/api/exports', payload);
  },
};

export const codeGenerationService = {
  templates(params: PlatformQuery) {
    return request.get<PageResult<CodeGenerationTemplateItem>, PageResult<CodeGenerationTemplateItem>>('/api/code-generation/templates', { params });
  },
  create(payload: CodeGenerationTemplateCreateRequest) {
    return request.post<CodeGenerationTemplateItem, CodeGenerationTemplateItem>('/api/code-generation/templates', payload);
  },
  update(id: number, payload: CodeGenerationTemplateUpdateRequest) {
    return request.put<CodeGenerationTemplateItem, CodeGenerationTemplateItem>(`/api/code-generation/templates/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/code-generation/templates/${id}`);
  },
};

export const entityChangeLogService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<EntityChangeLogItem>, PageResult<EntityChangeLogItem>>('/api/entity-change-logs', { params });
  },
};
