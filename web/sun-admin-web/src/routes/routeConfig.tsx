import {
  ApartmentOutlined,
  DashboardOutlined,
  IdcardOutlined,
  MenuFoldOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import type { ReactNode } from 'react';
import DashboardPage from '../features/dashboard/DashboardPage';
import DepartmentManagementPage from '../features/departments/DepartmentManagementPage';
import PositionManagementPage from '../features/positions/PositionManagementPage';
import UserManagementPage from '../features/users/UserManagementPage';
import RoleManagementPage from '../features/roles/RoleManagementPage';
import MenuManagementPage from '../features/menus/MenuManagementPage';

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
];

export const defaultAuthedPath = '/dashboard';
