import request from './request';
import type { SettingItem } from '../types/setting';

export const settingService = {
  list() {
    return request.get<SettingItem[], SettingItem[]>('/api/settings');
  },
  update(key: string, value: string) {
    return request.put<SettingItem, SettingItem>(`/api/settings/${encodeURIComponent(key)}`, {
      value,
    });
  },
};
