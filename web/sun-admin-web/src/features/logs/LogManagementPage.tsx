import { FileSearchOutlined, ReloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Input, Select, Table, Tabs, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useState } from 'react';
import { logService } from '../../services/logs';
import type { LoginLogItem, LogQuery, OperationLogItem } from '../../types/log';

const defaultQuery: LogQuery = { pageIndex: 1, pageSize: 20 };

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function LogManagementPage() {
  const { message } = AntApp.useApp();
  const [activeTab, setActiveTab] = useState('operations');
  const [keyword, setKeyword] = useState('');
  const [succeeded, setSucceeded] = useState<boolean | undefined>();
  const [operationQuery, setOperationQuery] = useState(defaultQuery);
  const [loginQuery, setLoginQuery] = useState(defaultQuery);
  const [operations, setOperations] = useState<OperationLogItem[]>([]);
  const [logins, setLogins] = useState<LoginLogItem[]>([]);
  const [operationTotal, setOperationTotal] = useState(0);
  const [loginTotal, setLoginTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  const buildQuery = (pageIndex = 1, pageSize = 20): LogQuery => ({
    pageIndex,
    pageSize,
    keyword,
    succeeded,
  });

  const loadOperations = async (nextQuery = operationQuery) => {
    setLoading(true);
    try {
      const result = await logService.operations(nextQuery);
      setOperations(result.items);
      setOperationTotal(result.total);
      setOperationQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '操作日志加载失败');
    } finally {
      setLoading(false);
    }
  };

  const loadLogins = async (nextQuery = loginQuery) => {
    setLoading(true);
    try {
      const result = await logService.logins(nextQuery);
      setLogins(result.items);
      setLoginTotal(result.total);
      setLoginQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '登录日志加载失败');
    } finally {
      setLoading(false);
    }
  };

  const reloadActive = () => {
    const nextQuery = buildQuery();
    if (activeTab === 'operations') {
      void loadOperations(nextQuery);
    } else {
      void loadLogins(nextQuery);
    }
  };

  useEffect(() => {
    void loadOperations(defaultQuery);
  }, []);

  const operationColumns: ColumnsType<OperationLogItem> = [
    { title: '用户', dataIndex: 'userName', width: 130 },
    { title: '方法', dataIndex: 'method', width: 90, render: (value) => <Tag>{value}</Tag> },
    { title: '路径', dataIndex: 'path', ellipsis: true },
    { title: '状态码', dataIndex: 'statusCode', width: 90 },
    {
      title: '结果',
      dataIndex: 'succeeded',
      width: 90,
      render: (value) => <Tag color={value ? 'green' : 'red'}>{value ? '成功' : '失败'}</Tag>,
    },
    { title: '耗时', dataIndex: 'durationMs', width: 100, render: (value) => `${value} ms` },
    { title: 'IP', dataIndex: 'ipAddress', width: 150, render: (value) => value || '-' },
    { title: '时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
  ];

  const loginColumns: ColumnsType<LoginLogItem> = [
    { title: '账号', dataIndex: 'account', width: 160 },
    { title: '用户名', dataIndex: 'userName', width: 140, render: (value) => value || '-' },
    {
      title: '结果',
      dataIndex: 'succeeded',
      width: 90,
      render: (value) => <Tag color={value ? 'green' : 'red'}>{value ? '成功' : '失败'}</Tag>,
    },
    { title: '说明', dataIndex: 'message', ellipsis: true },
    { title: 'IP', dataIndex: 'ipAddress', width: 150, render: (value) => value || '-' },
    { title: '时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
  ];

  return (
    <section className="page-surface">
      <div className="page-heading">
        <div>
          <div className="page-kicker">
            <FileSearchOutlined />
            审计追踪
          </div>
          <h1>日志审计</h1>
          <p>查看关键写操作和登录结果，便于定位权限、账号和系统使用问题。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{operationTotal}</strong>
            操作日志
          </span>
          <span>
            <strong>{loginTotal}</strong>
            登录日志
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            value={keyword}
            placeholder={activeTab === 'operations' ? '搜索用户、路径、方法' : '搜索账号、用户名、说明'}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => {
              setKeyword(value);
              const nextQuery = { ...buildQuery(), keyword: value };
              if (activeTab === 'operations') {
                void loadOperations(nextQuery);
              } else {
                void loadLogins(nextQuery);
              }
            }}
            style={{ width: 300 }}
          />
          <Select
            allowClear
            placeholder="结果"
            value={succeeded}
            onChange={(value) => {
              setSucceeded(value);
              const nextQuery = { ...buildQuery(), succeeded: value };
              if (activeTab === 'operations') {
                void loadOperations(nextQuery);
              } else {
                void loadLogins(nextQuery);
              }
            }}
            options={[
              { label: '成功', value: true },
              { label: '失败', value: false },
            ]}
            style={{ width: 120 }}
          />
          <Button icon={<ReloadOutlined />} onClick={reloadActive}>
            刷新
          </Button>
        </div>
      </div>
      <Tabs
        className="content-tabs"
        activeKey={activeTab}
        onChange={(key) => {
          setActiveTab(key);
          if (key === 'logins' && !logins.length) {
            void loadLogins(defaultQuery);
          }
        }}
        items={[
          {
            key: 'operations',
            label: '操作日志',
            children: (
              <Table
                rowKey="id"
                loading={loading}
                columns={operationColumns}
                dataSource={operations}
                scroll={{ x: 1200 }}
                pagination={{
                  current: operationQuery.pageIndex,
                  pageSize: operationQuery.pageSize,
                  total: operationTotal,
                  showSizeChanger: true,
                  showTotal: (value) => `共 ${value} 条`,
                  onChange: (pageIndex, pageSize) => void loadOperations(buildQuery(pageIndex, pageSize)),
                }}
              />
            ),
          },
          {
            key: 'logins',
            label: '登录日志',
            children: (
              <Table
                rowKey="id"
                loading={loading}
                columns={loginColumns}
                dataSource={logins}
                scroll={{ x: 1050 }}
                pagination={{
                  current: loginQuery.pageIndex,
                  pageSize: loginQuery.pageSize,
                  total: loginTotal,
                  showSizeChanger: true,
                  showTotal: (value) => `共 ${value} 条`,
                  onChange: (pageIndex, pageSize) => void loadLogins(buildQuery(pageIndex, pageSize)),
                }}
              />
            ),
          },
        ]}
      />
    </section>
  );
}
