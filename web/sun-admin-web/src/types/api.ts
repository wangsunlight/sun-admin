export interface ApiResponse<T> {
  code: string;
  message: string;
  data: T;
}

export interface ApiErrorResponse {
  code: string;
  message: string;
  errors?: Record<string, string[]>;
}

export interface PageResult<T> {
  items: T[];
  total: number;
  pageIndex: number;
  pageSize: number;
}

export interface PageQuery {
  pageIndex: number;
  pageSize: number;
  keyword?: string;
}

export type EntityStatus = 'Enabled' | 'Disabled';

export interface AuditableEntity {
  id: number;
  createdAt?: string;
  updatedAt?: string;
}
