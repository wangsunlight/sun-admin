import type { PageQuery } from './api';

export interface SessionQuery extends Partial<PageQuery> {
  activeOnly?: boolean;
}

export interface SessionItem {
  sessionId: string;
  userId: number;
  userName: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  createdAt: string;
  expiresAt: string;
  refreshTokenExpiresAt?: string | null;
  lastSeenAt?: string | null;
  revokedAt?: string | null;
}
