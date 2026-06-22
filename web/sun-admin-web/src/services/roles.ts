import request from './request';
import type { PageQuery, PageResult } from '../types/api';
import type {
  RoleItem,
  RoleUpsertRequest,
  UpdateRoleMenusRequest,
} from '../types/role';

export const roleService = {
  list(params?: Partial<PageQuery>) {
    return request.get<PageResult<RoleItem>, PageResult<RoleItem>>('/api/roles', {
      params,
    });
  },
  detail(id: number) {
    return request.get<RoleItem, RoleItem>(`/api/roles/${id}`);
  },
  create(payload: RoleUpsertRequest) {
    return request.post<RoleItem, RoleItem>('/api/roles', payload);
  },
  update(id: number, payload: RoleUpsertRequest) {
    return request.put<RoleItem, RoleItem>(`/api/roles/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/roles/${id}`);
  },
  updateMenus(id: number, payload: UpdateRoleMenusRequest) {
    return request.put<void, void>(`/api/roles/${id}/menus`, payload);
  },
};
