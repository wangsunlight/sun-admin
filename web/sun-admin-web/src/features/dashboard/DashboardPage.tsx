import {
  ArrowRightOutlined,
  MenuFoldOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import { App as AntApp, Button, Empty, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { menuService } from '../../services/menus';
import { roleService } from '../../services/roles';
import { userService } from '../../services/users';
import type { MenuItem } from '../../types/menu';
import type { RoleItem } from '../../types/role';
import type { UserItem } from '../../types/user';
import { flattenMenus } from '../../utils/menuTree';

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function DashboardPage() {
  const { message } = AntApp.useApp();
  const navigate = useNavigate();
  const [users, setUsers] = useState<UserItem[]>([]);
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [menus, setMenus] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(false);

  const flatMenus = useMemo(() => flattenMenus(menus), [menus]);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      const [userResult, roleResult, menuResult] = await Promise.allSettled([
        userService.list({ pageIndex: 1, pageSize: 6 }),
        roleService.list({ pageIndex: 1, pageSize: 100 }),
        menuService.tree(),
      ]);

      if (userResult.status === 'fulfilled') {
        setUsers(userResult.value.items);
      }
      if (roleResult.status === 'fulfilled') {
        setRoles(roleResult.value.items);
      }
      if (menuResult.status === 'fulfilled') {
        setMenus(menuResult.value);
      }
      if ([userResult, roleResult, menuResult].some((item) => item.status === 'rejected')) {
        message.warning('部分统计因权限限制未加载');
      }
      setLoading(false);
    };

    void load();
  }, [message]);

  const columns: ColumnsType<UserItem> = [
    { title: '用户名', dataIndex: 'userName', width: 160 },
    { title: '显示名', dataIndex: 'displayName', render: (value) => value || '-' },
    {
      title: '状态',
      dataIndex: 'status',
      width: 100,
      render: (value) => (
        <Tag color={value === 'Enabled' ? 'green' : 'default'}>
          {value === 'Enabled' ? '启用' : '禁用'}
        </Tag>
      ),
    },
    { title: '最后登录', dataIndex: 'lastLoginAt', width: 180, render: formatDateTime },
  ];

  return (
    <section className="page-surface dashboard-page">
      <div className="page-heading">
        <div>
          <div className="page-kicker">
            <TeamOutlined />
            工作台
          </div>
          <h1>系统概览</h1>
          <p>快速查看账号、角色和权限菜单的当前状态。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{users.length}</strong>
            最近用户
          </span>
          <span>
            <strong>{roles.length}</strong>
            角色
          </span>
          <span>
            <strong>{flatMenus.length}</strong>
            菜单节点
          </span>
        </div>
      </div>
      <div className="dashboard-grid">
        <div className="dashboard-metric">
          <TeamOutlined />
          <span>启用用户</span>
          <strong>{users.filter((item) => item.status === 'Enabled').length}</strong>
        </div>
        <div className="dashboard-metric">
          <SafetyCertificateOutlined />
          <span>启用角色</span>
          <strong>{roles.filter((item) => item.status === 'Enabled').length}</strong>
        </div>
        <div className="dashboard-metric">
          <MenuFoldOutlined />
          <span>按钮权限</span>
          <strong>{flatMenus.filter((item) => item.type === 'Button').length}</strong>
        </div>
      </div>
      <div className="dashboard-body">
        <div className="dashboard-panel dashboard-table">
          <div className="section-head">
            <h2>最近账号</h2>
            <Button type="link" onClick={() => navigate('/users')}>
              查看全部 <ArrowRightOutlined />
            </Button>
          </div>
          <Table
            rowKey="id"
            loading={loading}
            columns={columns}
            dataSource={users}
            pagination={false}
            locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="暂无用户数据" /> }}
          />
        </div>
        <div className="dashboard-panel quick-panel">
          <div className="section-head">
            <h2>常用操作</h2>
          </div>
          <button type="button" className="quick-action" onClick={() => navigate('/users')}>
            <TeamOutlined />
            <span>
              <strong>用户管理</strong>
              <small>创建账号、分配角色、重置密码</small>
            </span>
            <ArrowRightOutlined />
          </button>
          <button type="button" className="quick-action" onClick={() => navigate('/roles')}>
            <SafetyCertificateOutlined />
            <span>
              <strong>角色授权</strong>
              <small>维护 RBAC 角色与菜单权限</small>
            </span>
            <ArrowRightOutlined />
          </button>
          <button type="button" className="quick-action" onClick={() => navigate('/menus')}>
            <MenuFoldOutlined />
            <span>
              <strong>菜单权限</strong>
              <small>配置导航、页面和按钮权限码</small>
            </span>
            <ArrowRightOutlined />
          </button>
        </div>
      </div>
    </section>
  );
}
