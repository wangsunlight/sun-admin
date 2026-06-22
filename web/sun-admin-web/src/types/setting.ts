export interface SettingItem {
  id: number;
  key: string;
  value: string;
  name: string;
  description?: string | null;
  updatedAt: string;
}
