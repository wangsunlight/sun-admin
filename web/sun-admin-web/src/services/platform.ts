import request from './request';
import type { PageResult } from '../types/api';
import type {
  CodeGenerationTemplateItem,
  DictionaryItemGroup,
  EntityChangeLogItem,
  ExportTaskItem,
  FileResourceItem,
  NotificationItem,
  PlatformQuery,
} from '../types/platform';

export const dictionaryService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<DictionaryItemGroup>, PageResult<DictionaryItemGroup>>('/api/dictionaries', { params });
  },
};

export const notificationService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<NotificationItem>, PageResult<NotificationItem>>('/api/notifications', { params });
  },
};

export const fileResourceService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<FileResourceItem>, PageResult<FileResourceItem>>('/api/files', { params });
  },
};

export const exportTaskService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<ExportTaskItem>, PageResult<ExportTaskItem>>('/api/exports', { params });
  },
};

export const codeGenerationService = {
  templates(params: PlatformQuery) {
    return request.get<PageResult<CodeGenerationTemplateItem>, PageResult<CodeGenerationTemplateItem>>('/api/code-generation/templates', { params });
  },
};

export const entityChangeLogService = {
  list(params: PlatformQuery) {
    return request.get<PageResult<EntityChangeLogItem>, PageResult<EntityChangeLogItem>>('/api/entity-change-logs', { params });
  },
};
