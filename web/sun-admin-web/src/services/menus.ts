import request from './request';
import type { MenuItem, MenuUpsertRequest } from '../types/menu';

export const menuService = {
  tree() {
    return request.get<MenuItem[], MenuItem[]>('/api/menus/tree');
  },
  detail(id: number) {
    return request.get<MenuItem, MenuItem>(`/api/menus/${id}`);
  },
  create(payload: MenuUpsertRequest) {
    return request.post<MenuItem, MenuItem>('/api/menus', payload);
  },
  update(id: number, payload: MenuUpsertRequest) {
    return request.put<MenuItem, MenuItem>(`/api/menus/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/menus/${id}`);
  },
};
