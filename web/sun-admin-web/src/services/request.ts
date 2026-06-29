import axios, { AxiosError } from 'axios';
import {
  clearToken,
  getRefreshToken,
  getToken,
  setAuthTokens,
} from '../stores/tokenStorage';
import type { ApiErrorResponse, ApiResponse } from '../types/api';
import type { LoginResponse } from '../types/auth';

export const unauthorizedEventName = 'sun-admin:unauthorized';

interface RetryableConfig {
  _retry?: boolean;
  headers?: Record<string, unknown>;
  url?: string;
}

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

const refreshClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  timeout: 15000,
});

let refreshPromise: Promise<string | null> | null = null;

function unwrapResponseData<T>(body: ApiResponse<T> | T) {
  if (body && typeof body === 'object' && 'code' in body && 'data' in body) {
    return (body as ApiResponse<T>).data;
  }
  return body as T;
}

async function refreshAccessToken() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) {
    return null;
  }

  if (!refreshPromise) {
    refreshPromise = refreshClient
      .post<ApiResponse<LoginResponse> | LoginResponse>('/api/auth/refresh', {
        refreshToken,
      })
      .then((response) => {
        const data = unwrapResponseData<LoginResponse>(response.data);
        setAuthTokens(data.accessToken, data.refreshToken);
        return data.accessToken;
      })
      .catch(() => null)
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

request.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

request.interceptors.response.use(
  (response) => {
    return unwrapResponseData(response.data as ApiResponse<unknown> | unknown) as never;
  },
  async (error: AxiosError<ApiErrorResponse>) => {
    const originalConfig = error.config as (typeof error.config & RetryableConfig) | undefined;
    if (
      error.response?.status === 401 &&
      originalConfig &&
      !originalConfig._retry &&
      !originalConfig.url?.includes('/api/auth/refresh')
    ) {
      originalConfig._retry = true;
      const accessToken = await refreshAccessToken();
      if (accessToken) {
        originalConfig.headers = originalConfig.headers ?? {};
        originalConfig.headers.Authorization = `Bearer ${accessToken}`;
        return request(originalConfig);
      }
    }

    if (error.response?.status === 401) {
      clearToken();
      window.dispatchEvent(new Event(unauthorizedEventName));
    }

    return Promise.reject(new Error(resolveErrorMessage(error)));
  },
);

export default request;
