import request from './request';
import type {
  ChangePasswordRequest,
  CurrentUser,
  LoginRequest,
  LoginResponse,
} from '../types/auth';

export const authService = {
  login(payload: LoginRequest) {
    return request.post<LoginResponse, LoginResponse>('/api/auth/login', payload);
  },
  logout() {
    return request.post<void, void>('/api/auth/logout');
  },
  me() {
    return request.get<CurrentUser, CurrentUser>('/api/auth/me');
  },
  changePassword(payload: ChangePasswordRequest) {
    return request.post<void, void>('/api/auth/change-password', payload);
  },
};
