import {
  ApartmentOutlined,
  DashboardOutlined,
  FileSearchOutlined,
  IdcardOutlined,
  MenuFoldOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import type { ReactNode } from 'react';
import DashboardPage from '../features/dashboard/DashboardPage';
import DepartmentManagementPage from '../features/departments/DepartmentManagementPage';
import PositionManagementPage from '../features/positions/PositionManagementPage';
import UserManagementPage from '../features/users/UserManagementPage';
import RoleManagementPage from '../features/roles/RoleManagementPage';
import MenuManagementPage from '../features/menus/MenuManagementPage';
import LogManagementPage from '../features/logs/LogManagementPage';
import SessionManagementPage from '../features/sessions/SessionManagementPage';
import SettingManagementPage from '../features/settings/SettingManagementPage';

export interface StaticRouteItem {
  path: string;
  title: string;
  element: ReactNode;
  icon?: ReactNode;
}

export const staticRoutes: StaticRouteItem[] = [
  {
    path: '/dashboard',
    title: '工作台',
    element: <DashboardPage />,
    icon: <DashboardOutlined />,
  },
  {
    path: '/users',
    title: '用户管理',
    element: <UserManagementPage />,
    icon: <TeamOutlined />,
  },
  {
    path: '/roles',
    title: '角色管理',
    element: <RoleManagementPage />,
    icon: <SafetyCertificateOutlined />,
  },
  {
    path: '/departments',
    title: '部门管理',
    element: <DepartmentManagementPage />,
    icon: <ApartmentOutlined />,
  },
  {
    path: '/positions',
    title: '岗位管理',
    element: <PositionManagementPage />,
    icon: <IdcardOutlined />,
  },
  {
    path: '/menus',
    title: '菜单管理',
    element: <MenuManagementPage />,
    icon: <MenuFoldOutlined />,
  },
  {
    path: '/logs',
    title: '日志审计',
    element: <LogManagementPage />,
    icon: <FileSearchOutlined />,
  },
  {
    path: '/sessions',
    title: '在线会话',
    element: <SessionManagementPage />,
    icon: <ThunderboltOutlined />,
  },
  {
    path: '/settings',
    title: '系统配置',
    element: <SettingManagementPage />,
    icon: <SettingOutlined />,
  },
];

export const defaultAuthedPath = '/dashboard';
