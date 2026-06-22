import type { MenuItem } from './menu';

export interface LoginRequest {
  account: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt?: string;
}

export interface CurrentUser {
  id: number;
  userName: string;
  displayName?: string;
  email?: string;
  roles: string[];
  permissions: string[];
  menus: MenuItem[];
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}
