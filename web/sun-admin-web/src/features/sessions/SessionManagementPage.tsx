import { ReloadOutlined, ThunderboltOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Input, Modal, Select, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { sessionService } from '../../services/sessions';
import type { SessionItem, SessionQuery } from '../../types/session';

const defaultQuery: SessionQuery = { pageIndex: 1, pageSize: 20, activeOnly: true };

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function getSessionStatus(record: SessionItem) {
  if (record.revokedAt) {
    return <Tag color="red">已下线</Tag>;
  }
  if (new Date(record.expiresAt).getTime() <= Date.now()) {
    return <Tag>已过期</Tag>;
  }
  return <Tag color="green">在线</Tag>;
}

export default function SessionManagementPage() {
  const { message } = AntApp.useApp();
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [activeOnly, setActiveOnly] = useState(true);
  const [sessions, setSessions] = useState<SessionItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  const buildQuery = (pageIndex = 1, pageSize = query.pageSize ?? 20): SessionQuery => ({
    pageIndex,
    pageSize,
    keyword,
    activeOnly,
  });

  const loadSessions = async (nextQuery = query) => {
    setLoading(true);
    try {
      const result = await sessionService.list(nextQuery);
      setSessions(result.items);
      setTotal(result.total);
      setQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '会话加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadSessions(defaultQuery);
  }, []);

  const revokeSession = (record: SessionItem) => {
    Modal.confirm({
      title: '强制下线',
      content: `确认强制下线用户 ${record.userName} 的当前会话？`,
      okText: '下线',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        await sessionService.revoke(record.sessionId);
        message.success('会话已下线');
        await loadSessions();
      },
    });
  };

  const columns: ColumnsType<SessionItem> = [
    { title: '用户', dataIndex: 'userName', width: 150 },
    { title: '会话 ID', dataIndex: 'sessionId', ellipsis: true },
    { title: 'IP', dataIndex: 'ipAddress', width: 150, render: (value) => value || '-' },
    { title: '状态', width: 100, render: (_, record) => getSessionStatus(record) },
    { title: '登录时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
    { title: '最后活跃', dataIndex: 'lastSeenAt', width: 180, render: formatDateTime },
    { title: '过期时间', dataIndex: 'expiresAt', width: 180, render: formatDateTime },
    {
      title: '操作',
      width: 120,
      fixed: 'right',
      render: (_, record) => (
        <PermissionButton
          danger
          type="link"
          permission="session:revoke"
          disabled={Boolean(record.revokedAt) || new Date(record.expiresAt).getTime() <= Date.now()}
          onClick={() => revokeSession(record)}
        >
          下线
        </PermissionButton>
      ),
    },
  ];

  return (
    <section className="page-surface">
      <div className="page-heading">
        <div>
          <div className="page-kicker">
            <ThunderboltOutlined />
            登录会话
          </div>
          <h1>在线会话</h1>
          <p>查看当前有效登录，并在账号异常时强制下线指定会话。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{total}</strong>
            会话
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索用户、会话 ID、IP"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => {
              setKeyword(value);
              void loadSessions({ ...buildQuery(), keyword: value });
            }}
            style={{ width: 300 }}
          />
          <Select
            value={activeOnly}
            onChange={(value) => {
              setActiveOnly(value);
              void loadSessions({ ...buildQuery(), activeOnly: value });
            }}
            options={[
              { label: '仅在线', value: true },
              { label: '全部会话', value: false },
            ]}
            style={{ width: 130 }}
          />
          <Button icon={<ReloadOutlined />} onClick={() => void loadSessions()}>
            刷新
          </Button>
        </div>
      </div>
      <Table
        rowKey="sessionId"
        loading={loading}
        columns={columns}
        dataSource={sessions}
        scroll={{ x: 1100 }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) => void loadSessions(buildQuery(pageIndex, pageSize)),
        }}
      />
    </section>
  );
}
