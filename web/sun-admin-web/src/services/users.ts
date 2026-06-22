import request from './request';
import type { PageQuery, PageResult } from '../types/api';
import type {
  UpdateUserRolesRequest,
  UserItem,
  UserUpsertRequest,
} from '../types/user';

export const userService = {
  list(params: PageQuery) {
    return request.get<PageResult<UserItem>, PageResult<UserItem>>('/api/users', {
      params,
    });
  },
  detail(id: number) {
    return request.get<UserItem, UserItem>(`/api/users/${id}`);
  },
  create(payload: UserUpsertRequest) {
    return request.post<UserItem, UserItem>('/api/users', payload);
  },
  update(id: number, payload: UserUpsertRequest) {
    return request.put<UserItem, UserItem>(`/api/users/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/users/${id}`);
  },
  enable(id: number) {
    return request.post<void, void>(`/api/users/${id}/enable`);
  },
  disable(id: number) {
    return request.post<void, void>(`/api/users/${id}/disable`);
  },
  resetPassword(id: number, newPassword: string) {
    return request.post<void, void>(`/api/users/${id}/reset-password`, {
      newPassword,
    });
  },
  updateRoles(id: number, payload: UpdateUserRolesRequest) {
    return request.put<void, void>(`/api/users/${id}/roles`, payload);
  },
};
