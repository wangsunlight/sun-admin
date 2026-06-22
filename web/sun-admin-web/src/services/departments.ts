import request from './request';
import type { DepartmentItem, DepartmentUpsertRequest } from '../types/department';

export const departmentService = {
  tree() {
    return request.get<DepartmentItem[], DepartmentItem[]>('/api/departments/tree');
  },
  detail(id: number) {
    return request.get<DepartmentItem, DepartmentItem>(`/api/departments/${id}`);
  },
  create(payload: DepartmentUpsertRequest) {
    return request.post<DepartmentItem, DepartmentItem>('/api/departments', payload);
  },
  update(id: number, payload: DepartmentUpsertRequest) {
    return request.put<DepartmentItem, DepartmentItem>(`/api/departments/${id}`, payload);
  },
  remove(id: number) {
    return request.delete<void, void>(`/api/departments/${id}`);
  },
};
