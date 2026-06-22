import {
  ApartmentOutlined,
  ArrowRightOutlined,
  FileSearchOutlined,
  IdcardOutlined,
  MenuFoldOutlined,
  SafetyCertificateOutlined,
  TeamOutlined,
} from '@ant-design/icons';
import { App as AntApp, Button, Empty, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { dashboardService } from '../../services/dashboard';
import type { DashboardStats } from '../../types/dashboard';
import type { LoginLogItem, OperationLogItem } from '../../types/log';

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function DashboardPage() {
  const { message } = AntApp.useApp();
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(false);

  const loadStats = async () => {
    setLoading(true);
    try {
      setStats(await dashboardService.stats());
    } catch (error) {
      message.error(error instanceof Error ? error.message : '仪表盘数据加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadStats();
  }, []);

  const operationColumns: ColumnsType<OperationLogItem> = [
    { title: '账号', dataIndex: 'userName', width: 120 },
    { title: '请求', dataIndex: 'path', ellipsis: true },
    {
      title: '结果',
      dataIndex: 'succeeded',
      width: 90,
      render: (value) => <Tag color={value ? 'green' : 'red'}>{value ? '成功' : '失败'}</Tag>,
    },
    { title: '时间', dataIndex: 'createdAt', width: 170, render: formatDateTime },
  ];

  const loginColumns: ColumnsType<LoginLogItem> = [
    { title: '账号', dataIndex: 'account', width: 140 },
    {
      title: '结果',
      dataIndex: 'succeeded',
      width: 90,
      render: (value) => <Tag color={value ? 'green' : 'red'}>{value ? '成功' : '失败'}</Tag>,
    },
    { title: '说明', dataIndex: 'message', ellipsis: true },
    { title: '时间', dataIndex: 'createdAt', width: 170, render: formatDateTime },
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
          <p>查看账号、组织、权限、审计和会话相关的核心运行状态。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{stats?.userCount ?? 0}</strong>
            用户
          </span>
          <span>
            <strong>{stats?.roleCount ?? 0}</strong>
            角色
          </span>
          <span>
            <strong>{stats?.menuCount ?? 0}</strong>
            菜单
          </span>
        </div>
      </div>
      <div className="dashboard-workbench">
        <div className="metric-strip">
          <span>
            <strong>{stats?.enabledUserCount ?? 0}</strong>
            启用用户
          </span>
          <span>
            <strong>{stats?.departmentCount ?? 0}</strong>
            部门
          </span>
          <span>
            <strong>{stats?.positionCount ?? 0}</strong>
            岗位
          </span>
          <span>
            <strong>{stats?.operationCountToday ?? 0}</strong>
            今日操作
          </span>
          <span>
            <strong>{stats?.failedLoginCountToday ?? 0}</strong>
            今日失败登录
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
          <Button icon={<FileSearchOutlined />} onClick={() => navigate('/logs')}>
            审计日志
          </Button>
        </div>
        <div className="dashboard-grid">
          <div className="dashboard-table">
            <div className="section-head">
              <h2>最近操作</h2>
              <Button type="link" onClick={() => navigate('/logs')}>
                查看全部 <ArrowRightOutlined />
              </Button>
            </div>
            <Table
              rowKey="id"
              loading={loading}
              columns={operationColumns}
              dataSource={stats?.recentOperations ?? []}
              pagination={false}
              locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="暂无操作日志" /> }}
            />
          </div>
          <div className="dashboard-table">
            <div className="section-head">
              <h2>最近登录</h2>
              <Button type="link" onClick={() => navigate('/logs')}>
                查看全部 <ArrowRightOutlined />
              </Button>
            </div>
            <Table
              rowKey="id"
              loading={loading}
              columns={loginColumns}
              dataSource={stats?.recentLogins ?? []}
              pagination={false}
              locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="暂无登录日志" /> }}
            />
          </div>
        </div>
      </div>
    </section>
  );
}
