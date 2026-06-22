import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import { Spin } from 'antd';
import LoginPage from '../features/auth/LoginPage';
import MainLayout from '../layouts/MainLayout';
import { useAuth } from '../stores/authStore';
import { defaultAuthedPath, staticRoutes } from './routeConfig';

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
              element={route.element}
            />
          ))}
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={defaultAuthedPath} replace />} />
    </Routes>
  );
}
