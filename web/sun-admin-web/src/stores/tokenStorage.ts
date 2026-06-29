const TOKEN_KEY = 'sun_admin_access_token';
const REFRESH_TOKEN_KEY = 'sun_admin_refresh_token';

export function getToken() {
  return window.localStorage.getItem(TOKEN_KEY);
}

export function getRefreshToken() {
  return window.localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setToken(token: string) {
  window.localStorage.setItem(TOKEN_KEY, token);
}

export function setRefreshToken(token: string) {
  window.localStorage.setItem(REFRESH_TOKEN_KEY, token);
}

export function setAuthTokens(accessToken: string, refreshToken: string) {
  setToken(accessToken);
  setRefreshToken(refreshToken);
}

export function clearToken() {
  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(REFRESH_TOKEN_KEY);
}
