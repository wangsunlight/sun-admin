import { Suspense, useMemo } from 'react';
import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import { Spin } from 'antd';
import AppErrorBoundary from '../components/AppErrorBoundary';
import RouteFallback from '../components/RouteFallback';
import ForbiddenPage from '../features/errors/ForbiddenPage';
import LoginPage from '../features/auth/LoginPage';
import MainLayout from '../layouts/MainLayout';
import { useAuth } from '../stores/authStore';
import { defaultAuthedPath, staticRoutes, type StaticRouteItem } from './routeConfig';
import { canAccessPath } from './routeAccess';

function ProtectedRoute() {
  const { loading, isAuthenticated } = useAuth();

  if (loading) {
    return (
      <div className="login-page">
        <Spin size="large" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}

function PageRoute({ route }: { route: StaticRouteItem }) {
  const { hasPermission, menus } = useAuth();
  const knownPaths = useMemo(() => staticRoutes.map((item) => item.path), []);
  const canAccessByMenu = canAccessPath(route.path, menus, knownPaths);
  const canAccessByPermission = route.permissionCode
    ? hasPermission(route.permissionCode)
    : false;

  if (!canAccessByMenu && !canAccessByPermission) {
    return <ForbiddenPage />;
  }

  return (
    <AppErrorBoundary>
      <Suspense fallback={<RouteFallback />}>{route.element}</Suspense>
    </AppErrorBoundary>
  );
}

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<Navigate to={defaultAuthedPath} replace />} />
          {staticRoutes.map((route) => (
            <Route
              key={route.path}
              path={route.path.slice(1)}
              element={<PageRoute route={route} />}
            />
          ))}
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={defaultAuthedPath} replace />} />
    </Routes>
  );
}
