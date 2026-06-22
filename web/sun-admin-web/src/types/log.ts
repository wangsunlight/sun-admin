import type { PageQuery } from './api';

export interface LogQuery extends Partial<PageQuery> {
  succeeded?: boolean;
  createdFrom?: string;
  createdTo?: string;
}

export interface OperationLogItem {
  id: number;
  userId?: number | null;
  userName: string;
  method: string;
  path: string;
  statusCode: number;
  succeeded: boolean;
  durationMs: number;
  ipAddress?: string | null;
  userAgent?: string | null;
  errorMessage?: string | null;
  createdAt: string;
}

export interface LoginLogItem {
  id: number;
  userId?: number | null;
  account: string;
  userName?: string | null;
  succeeded: boolean;
  message: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt: string;
}
