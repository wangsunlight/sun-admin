import axios, { AxiosError } from 'axios';
import { clearToken, getToken } from '../stores/tokenStorage';
import type { ApiErrorResponse, ApiResponse } from '../types/api';

export const unauthorizedEventName = 'sun-admin:unauthorized';

function resolveErrorMessage(error: AxiosError<ApiErrorResponse>) {
  if (error.response?.status === 401) {
    return '登录状态已失效，请重新登录';
  }

  if (error.response?.status === 403) {
    return '无权限执行该操作';
  }

  if (error.response?.data?.errors) {
    const firstError = Object.values(error.response.data.errors).flat()[0];
    if (firstError) {
      return firstError;
    }
  }

  return error.response?.data?.message || error.message || '请求失败，请稍后重试';
}

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  timeout: 15000,
});

request.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

request.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse<unknown> | unknown;
    if (body && typeof body === 'object' && 'code' in body && 'data' in body) {
      return (body as ApiResponse<unknown>).data as never;
    }
    return body as never;
  },
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      clearToken();
      window.dispatchEvent(new Event(unauthorizedEventName));
    }

    return Promise.reject(new Error(resolveErrorMessage(error)));
  },
);

export default request;
