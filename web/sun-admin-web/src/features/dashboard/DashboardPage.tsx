import {
  ApartmentOutlined,
  ArrowRightOutlined,
  IdcardOutlined,
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
      </div>
      <div className="dashboard-workbench">
        <div className="metric-strip">
          <span>
            <strong>{users.filter((item) => item.status === 'Enabled').length}</strong>
            启用用户
          </span>
          <span>
            <strong>{roles.filter((item) => item.status === 'Enabled').length}</strong>
            启用角色
          </span>
          <span>
            <strong>{flatMenus.filter((item) => item.type === 'Button').length}</strong>
            按钮权限
          </span>
        </div>
        <div className="quick-strip">
          <Button icon={<TeamOutlined />} onClick={() => navigate('/users')}>
            用户
          </Button>
          <Button icon={<SafetyCertificateOutlined />} onClick={() => navigate('/roles')}>
            角色
          </Button>
          <Button icon={<ApartmentOutlined />} onClick={() => navigate('/departments')}>
            部门
          </Button>
          <Button icon={<IdcardOutlined />} onClick={() => navigate('/positions')}>
            岗位
          </Button>
          <Button icon={<MenuFoldOutlined />} onClick={() => navigate('/menus')}>
            菜单
          </Button>
        </div>
        <div className="dashboard-table">
          <div className="section-head">
            <h2>最近账号</h2>
            <Button type="link" onClick={() => navigate('/users')}>查看全部 <ArrowRightOutlined /></Button>
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
      </div>
    </section>
  );
}
