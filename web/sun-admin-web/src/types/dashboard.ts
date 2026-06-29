import type { LoginLogItem, OperationLogItem } from './log';

export interface DashboardStats {
  userCount: number;
  enabledUserCount: number;
  roleCount: number;
  departmentCount: number;
  positionCount: number;
  menuCount: number;
  operationCountToday: number;
  failedLoginCountToday: number;
  apiErrorCountToday: number;
  activeSessionCount: number;
  pendingExportCount: number;
  recentOperations: OperationLogItem[];
  recentLogins: LoginLogItem[];
}
