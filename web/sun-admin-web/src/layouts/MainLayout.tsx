import {
  AppstoreOutlined,
  IdcardOutlined,
  KeyOutlined,
  LockOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  UserOutlined,
} from '@ant-design/icons';
import {
  App as AntApp,
  Avatar,
  Button,
  Descriptions,
  Dropdown,
  Form,
  Input,
  Layout,
  Menu,
  Modal,
  Space,
  Tag,
  Typography,
} from 'antd';
import type { MenuProps } from 'antd';
import { useMemo, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { defaultAuthedPath, staticRoutes } from '../routes/routeConfig';
import { authService } from '../services/auth';
import { useAuth } from '../stores/authStore';
import type { ChangePasswordRequest, CurrentUser } from '../types/auth';
import type { MenuItem } from '../types/menu';

const { Header, Sider, Content } = Layout;

interface ChangePasswordFormValues extends ChangePasswordRequest {
  confirmPassword: string;
}

function collectVisiblePaths(menus: MenuItem[]) {
  const paths = new Set<string>();

  const walk = (items: MenuItem[]) => {
    items.forEach((item) => {
      if (item.status === 'Enabled' && item.type === 'Page' && item.routePath) {
        paths.add(item.routePath);
      }
      if (item.children?.length) {
        walk(item.children);
      }
    });
  };

  walk(menus);
  return paths;
}

export default function MainLayout() {
  const { message } = AntApp.useApp();
  const [passwordForm] = Form.useForm<ChangePasswordFormValues>();
  const [collapsed, setCollapsed] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [profileLoading, setProfileLoading] = useState(false);
  const [profileUser, setProfileUser] = useState<CurrentUser | null>(null);
  const [passwordOpen, setPasswordOpen] = useState(false);
  const [passwordSubmitting, setPasswordSubmitting] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, menus, refreshMe, user } = useAuth();

  const menuItems = useMemo<MenuProps['items']>(() => {
    const visiblePaths = collectVisiblePaths(menus);
    const hasMatchedRoute = staticRoutes.some((route) => visiblePaths.has(route.path));
    const allowAll = visiblePaths.size === 0 || !hasMatchedRoute;

    return staticRoutes
      .filter((route) => allowAll || visiblePaths.has(route.path))
      .map((route) => ({
        key: route.path,
        icon: route.icon,
        label: route.title,
      }));
  }, [menus]);

  const activePath = staticRoutes.find((route) =>
    location.pathname.startsWith(route.path),
  )?.path;

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const openProfile = async () => {
    setProfileOpen(true);
    setProfileLoading(true);
    try {
      const latestUser = await authService.me();
      setProfileUser(latestUser);
    } catch (error) {
      setProfileUser(user);
      message.error(error instanceof Error ? error.message : '个人资料加载失败');
    } finally {
      setProfileLoading(false);
    }
  };

  const openPasswordModal = () => {
    passwordForm.resetFields();
    setPasswordOpen(true);
  };

  const handleChangePassword = async () => {
    const values = await passwordForm.validateFields();
    setPasswordSubmitting(true);
    try {
      await authService.changePassword({
        oldPassword: values.oldPassword,
        newPassword: values.newPassword,
      });
      message.success('密码修改成功');
      setPasswordOpen(false);
      passwordForm.resetFields();
      await refreshMe();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '密码修改失败');
    } finally {
      setPasswordSubmitting(false);
    }
  };

  const userMenu: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <IdcardOutlined />,
      label: '个人资料',
      onClick: () => void openProfile(),
    },
    {
      key: 'change-password',
      icon: <KeyOutlined />,
      label: '修改密码',
      onClick: openPasswordModal,
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: '退出登录',
      onClick: handleLogout,
    },
  ];

  const displayUser = profileUser ?? user;
  const primaryRole = user?.roles?.[0] ?? 'member';

  return (
    <Layout className="app-shell">
      <Sider width={236} collapsed={collapsed} breakpoint="lg" className="app-sider">
        <div className="app-logo">
          <span className="app-logo-mark">S</span>
          {!collapsed && (
            <span className="app-logo-text">
              <strong>sun-admin</strong>
              <small>Access Console</small>
            </span>
          )}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[activePath ?? defaultAuthedPath]}
          items={menuItems}
          onClick={({ key }) => navigate(String(key))}
        />
        {!collapsed && (
          <div className="app-sider-footer">
            <span>当前角色</span>
            <strong>{primaryRole}</strong>
          </div>
        )}
      </Sider>
      <Layout className="app-main">
        <Header className="app-header">
          <Space>
            <Button
              className="icon-action"
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={() => setCollapsed((value) => !value)}
            />
            <div className="header-title">
              <Typography.Text type="secondary">
                <AppstoreOutlined /> 控制台
              </Typography.Text>
              <Typography.Title level={4}>
                {staticRoutes.find((route) => route.path === activePath)?.title ??
                  '后台管理'}
              </Typography.Title>
            </div>
          </Space>
          <Dropdown menu={{ items: userMenu }} placement="bottomRight">
            <Space className="account-trigger">
              <Avatar size={32} icon={<UserOutlined />} />
              <span className="account-copy">
                <Typography.Text>
                  {user?.displayName || user?.userName || '管理员'}
                </Typography.Text>
                <Typography.Text type="secondary">{primaryRole}</Typography.Text>
              </span>
            </Space>
          </Dropdown>
        </Header>
        <Content className="app-content">
          <Outlet />
        </Content>
      </Layout>

      <Modal
        title="个人资料"
        open={profileOpen}
        footer={null}
        loading={profileLoading}
        onCancel={() => setProfileOpen(false)}
      >
        {displayUser && (
          <div className="profile-modal">
            <Space className="profile-modal-head" size={12}>
              <Avatar size={48} icon={<UserOutlined />} />
              <div>
                <Typography.Title level={5}>
                  {displayUser.displayName || displayUser.userName}
                </Typography.Title>
                <Typography.Text type="secondary">{displayUser.userName}</Typography.Text>
              </div>
            </Space>
            <Descriptions column={1} size="middle" bordered>
              <Descriptions.Item label="用户 ID">{displayUser.id}</Descriptions.Item>
              <Descriptions.Item label="用户名">{displayUser.userName}</Descriptions.Item>
              <Descriptions.Item label="显示名">
                {displayUser.displayName || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="邮箱">{displayUser.email || '-'}</Descriptions.Item>
              <Descriptions.Item label="角色">
                {displayUser.roles.length ? (
                  <Space size={[4, 4]} wrap>
                    {displayUser.roles.map((role) => (
                      <Tag key={role} color="blue">
                        {role}
                      </Tag>
                    ))}
                  </Space>
                ) : (
                  '-'
                )}
              </Descriptions.Item>
              <Descriptions.Item label="权限数量">
                {displayUser.permissions.length}
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </Modal>

      <Modal
        title="修改密码"
        open={passwordOpen}
        okText="保存"
        cancelText="取消"
        confirmLoading={passwordSubmitting}
        onCancel={() => setPasswordOpen(false)}
        onOk={() => void handleChangePassword()}
      >
        <Form form={passwordForm} layout="vertical" requiredMark={false}>
          <Form.Item
            name="oldPassword"
            label="当前密码"
            rules={[{ required: true, message: '请输入当前密码' }]}
          >
            <Input.Password
              autoComplete="current-password"
              prefix={<LockOutlined />}
              placeholder="输入当前密码"
            />
          </Form.Item>
          <Form.Item
            name="newPassword"
            label="新密码"
            rules={[{ required: true, min: 8, message: '新密码至少 8 位' }]}
          >
            <Input.Password
              autoComplete="new-password"
              prefix={<KeyOutlined />}
              placeholder="输入新密码"
            />
          </Form.Item>
          <Form.Item
            name="confirmPassword"
            label="确认新密码"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: '请再次输入新密码' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('两次输入的密码不一致'));
                },
              }),
            ]}
          >
            <Input.Password
              autoComplete="new-password"
              prefix={<KeyOutlined />}
              placeholder="再次输入新密码"
            />
          </Form.Item>
        </Form>
      </Modal>
    </Layout>
  );
}
