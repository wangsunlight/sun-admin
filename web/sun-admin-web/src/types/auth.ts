import type { MenuItem } from './menu';

export interface LoginRequest {
  account: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt?: string;
  user?: CurrentUser;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface CurrentUser {
  id: number;
  userName: string;
  displayName?: string;
  email?: string;
  departmentId?: number | null;
  departmentName?: string | null;
  positionId?: number | null;
  positionName?: string | null;
  mustChangePassword?: boolean;
  roles: string[];
  permissions: string[];
  menus: MenuItem[];
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UpdateProfileRequest {
  displayName: string;
  email: string;
  departmentId?: number | null;
  positionId?: number | null;
}
