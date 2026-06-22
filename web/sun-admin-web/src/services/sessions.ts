import request from './request';
import type { PageResult } from '../types/api';
import type { SessionItem, SessionQuery } from '../types/session';

export const sessionService = {
  list(params: SessionQuery) {
    return request.get<PageResult<SessionItem>, PageResult<SessionItem>>('/api/sessions', {
      params,
    });
  },
  revoke(sessionId: string) {
    return request.post<void, void>(`/api/sessions/${sessionId}/revoke`);
  },
};
