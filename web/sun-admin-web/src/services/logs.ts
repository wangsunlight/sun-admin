import request from './request';
import type { PageResult } from '../types/api';
import type { LogQuery, LoginLogItem, OperationLogItem } from '../types/log';

export const logService = {
  operations(params: LogQuery) {
    return request.get<PageResult<OperationLogItem>, PageResult<OperationLogItem>>('/api/logs/operations', {
      params,
    });
  },
  logins(params: LogQuery) {
    return request.get<PageResult<LoginLogItem>, PageResult<LoginLogItem>>('/api/logs/logins', {
      params,
    });
  },
};
