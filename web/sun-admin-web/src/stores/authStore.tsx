import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import { authService } from '../services/auth';
import { unauthorizedEventName } from '../services/request';
import { clearToken, getToken, setAuthTokens } from './tokenStorage';
import type { CurrentUser, LoginRequest } from '../types/auth';

interface AuthContextValue {
  user: CurrentUser | null;
  permissions: string[];
  menus: CurrentUser['menus'];
  loading: boolean;
  isAuthenticated: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshMe: () => Promise<void>;
  hasPermission: (permissionCode: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshMe = useCallback(async () => {
    const currentUser = await authService.me();
    setUser(currentUser);
  }, []);

  const login = useCallback(
    async (payload: LoginRequest) => {
      const result = await authService.login(payload);
      setAuthTokens(result.accessToken, result.refreshToken);
      await refreshMe();
    },
    [refreshMe],
  );

  const logout = useCallback(async () => {
    try {
      if (getToken()) {
        await authService.logout();
      }
    } finally {
      clearToken();
      setUser(null);
    }
  }, []);

  useEffect(() => {
    const bootstrap = async () => {
      if (!getToken()) {
        setLoading(false);
        return;
      }

      try {
        await refreshMe();
      } catch {
        clearToken();
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    void bootstrap();
  }, [refreshMe]);

  useEffect(() => {
    const handleUnauthorized = () => setUser(null);
    window.addEventListener(unauthorizedEventName, handleUnauthorized);
    return () =>
      window.removeEventListener(unauthorizedEventName, handleUnauthorized);
  }, []);

  const value = useMemo<AuthContextValue>(() => {
    const permissions = user?.permissions ?? [];
    const isSuperAdmin = user?.roles?.includes('super_admin') ?? false;

    return {
      user,
      permissions,
      menus: user?.menus ?? [],
      loading,
      isAuthenticated: Boolean(user && getToken()),
      login,
      logout,
      refreshMe,
      hasPermission: (permissionCode: string) =>
        isSuperAdmin || permissions.includes(permissionCode),
    };
  }, [loading, login, logout, refreshMe, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider');
  }
  return context;
}
