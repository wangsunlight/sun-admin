import request from './request';
import type { PageQuery, PageResult } from '../types/api';
import type { PositionItem, PositionUpsertRequest } from '../types/position';

export const positionService = {
  list(params: Partial<PageQuery>) {
    return request.get<PageResult<PositionItem>, PageResult<PositionItem>>('/api/positions', {
      params,
    });
  },
  detail(id: number) {
    return request.get<PositionItem, PositionItem>(`/api/positions/${id}`);
  },
  create(payload: PositionUpsertRequest) {
    return request.post<PositionItem, PositionItem>('/api/positions', payload);
  },
  update(id: number, payload: PositionUpsertRequest) {
    return request.put<PositionItem, PositionItem>(`/api/positions/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/positions/${id}`);
  },
};
