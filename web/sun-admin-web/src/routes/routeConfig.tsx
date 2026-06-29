import {
  ApartmentOutlined,
  BellOutlined,
  CodeOutlined,
  DashboardOutlined,
  DatabaseOutlined,
  DownloadOutlined,
  FileSearchOutlined,
  FileTextOutlined,
  FolderOpenOutlined,
  IdcardOutlined,
  MenuFoldOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  ThunderboltOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import { lazy, type ReactNode } from 'react';

const DashboardPage = lazy(() => import('../features/dashboard/DashboardPage'));
const DepartmentManagementPage = lazy(
  () => import('../features/departments/DepartmentManagementPage'),
);
const PositionManagementPage = lazy(
  () => import('../features/positions/PositionManagementPage'),
);
const UserManagementPage = lazy(() => import('../features/users/UserManagementPage'));
const RoleManagementPage = lazy(() => import('../features/roles/RoleManagementPage'));
const MenuManagementPage = lazy(() => import('../features/menus/MenuManagementPage'));
const LogManagementPage = lazy(() => import('../features/logs/LogManagementPage'));
const SessionManagementPage = lazy(
  () => import('../features/sessions/SessionManagementPage'),
);
const SettingManagementPage = lazy(
  () => import('../features/settings/SettingManagementPage'),
);
const DictionaryManagementPage = lazy(
  () => import('../features/platform/DictionaryManagementPage'),
);
const NotificationManagementPage = lazy(
  () => import('../features/platform/NotificationManagementPage'),
);
const FileResourcePage = lazy(() => import('../features/platform/FileResourcePage'));
const ExportTaskPage = lazy(() => import('../features/platform/ExportTaskPage'));
const CodeGenerationPage = lazy(
  () => import('../features/platform/CodeGenerationPage'),
);
const EntityChangeLogPage = lazy(
  () => import('../features/platform/EntityChangeLogPage'),
);

export interface StaticRouteItem {
  path: string;
  title: string;
  element: ReactNode;
  icon?: ReactNode;
  permissionCode?: string;
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
    permissionCode: 'user:view',
  },
  {
    path: '/roles',
    title: '角色管理',
    element: <RoleManagementPage />,
    icon: <SafetyCertificateOutlined />,
    permissionCode: 'role:view',
  },
  {
    path: '/departments',
    title: '部门管理',
    element: <DepartmentManagementPage />,
    icon: <ApartmentOutlined />,
    permissionCode: 'department:view',
  },
  {
    path: '/positions',
    title: '岗位管理',
    element: <PositionManagementPage />,
    icon: <IdcardOutlined />,
    permissionCode: 'position:view',
  },
  {
    path: '/menus',
    title: '菜单管理',
    element: <MenuManagementPage />,
    icon: <MenuFoldOutlined />,
    permissionCode: 'menu:view',
  },
  {
    path: '/logs',
    title: '日志审计',
    element: <LogManagementPage />,
    icon: <FileSearchOutlined />,
    permissionCode: 'operation-log:view',
  },
  {
    path: '/sessions',
    title: '在线会话',
    element: <SessionManagementPage />,
    icon: <ThunderboltOutlined />,
    permissionCode: 'session:view',
  },
  {
    path: '/settings',
    title: '系统配置',
    element: <SettingManagementPage />,
    icon: <SettingOutlined />,
    permissionCode: 'setting:view',
  },
  {
    path: '/dictionaries',
    title: '数据字典',
    element: <DictionaryManagementPage />,
    icon: <DatabaseOutlined />,
    permissionCode: 'dictionary:view',
  },
  {
    path: '/notifications',
    title: '通知公告',
    element: <NotificationManagementPage />,
    icon: <BellOutlined />,
    permissionCode: 'notification:view',
  },
  {
    path: '/files',
    title: '文件资源',
    element: <FileResourcePage />,
    icon: <FolderOpenOutlined />,
    permissionCode: 'file:view',
  },
  {
    path: '/exports',
    title: '导出中心',
    element: <ExportTaskPage />,
    icon: <DownloadOutlined />,
    permissionCode: 'export:view',
  },
  {
    path: '/code-generation',
    title: '代码生成',
    element: <CodeGenerationPage />,
    icon: <CodeOutlined />,
    permissionCode: 'code-generation:view',
  },
  {
    path: '/entity-change-logs',
    title: '变更审计',
    element: <EntityChangeLogPage />,
    icon: <FileTextOutlined />,
    permissionCode: 'entity-change-log:view',
  },
];

export const defaultAuthedPath = '/dashboard';
