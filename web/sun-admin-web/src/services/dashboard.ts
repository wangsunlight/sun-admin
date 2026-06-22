import request from './request';
import type { DashboardStats } from '../types/dashboard';

export const dashboardService = {
  stats() {
    return request.get<DashboardStats, DashboardStats>('/api/dashboard/stats');
  },
};
