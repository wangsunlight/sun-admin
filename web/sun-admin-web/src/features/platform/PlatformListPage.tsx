import { ReloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Empty, Input, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import type { TableProps } from 'antd';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import type { PageResult } from '../../types/api';
import type { PlatformQuery } from '../../types/platform';

interface PlatformListPageProps<T extends { id: number }> {
  title: string;
  kicker: string;
  description: string;
  permission?: string;
  columns: ColumnsType<T>;
  load: (query: PlatformQuery) => Promise<PageResult<T>>;
  toolbarExtra?: (helpers: PlatformListHelpers) => ReactNode;
  actions?: (record: T, helpers: PlatformListHelpers) => ReactNode;
  expandable?: TableProps<T>['expandable'];
}

const defaultQuery: PlatformQuery = { pageIndex: 1, pageSize: 20 };

export interface PlatformListHelpers {
  reload: () => Promise<void>;
}

export function PlatformListPage<T extends { id: number }>({
  title,
  kicker,
  description,
  columns,
  load,
  toolbarExtra,
  actions,
  expandable,
}: PlatformListPageProps<T>) {
  const { message } = AntApp.useApp();
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [items, setItems] = useState<T[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  const buildQuery = (pageIndex = 1, pageSize = query.pageSize ?? 20): PlatformQuery => ({
    pageIndex,
    pageSize,
    keyword,
  });

  const loadItems = async (nextQuery = query) => {
    setLoading(true);
    try {
      const result = await load(nextQuery);
      setItems(result.items);
      setTotal(result.total);
      setQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : `${title}加载失败`);
    } finally {
      setLoading(false);
    }
  };

  const helpers = useMemo<PlatformListHelpers>(() => ({
    reload: () => loadItems(),
  }), [query, keyword]);

  const tableColumns = useMemo<ColumnsType<T>>(() => {
    if (!actions) {
      return columns;
    }

    return [
      ...columns,
      {
        title: '操作',
        width: 180,
        fixed: 'right',
        render: (_, record) => actions(record, helpers),
      },
    ];
  }, [actions, columns, helpers]);

  useEffect(() => {
    void loadItems(defaultQuery);
  }, []);

  return (
    <section className="page-surface">
      <div className="page-heading">
        <div>
          <div className="page-kicker">{kicker}</div>
          <h1>{title}</h1>
          <p>{description}</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{total}</strong>
            条记录
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索关键字"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => {
              setKeyword(value);
              void loadItems({ ...buildQuery(), keyword: value });
            }}
            style={{ width: 300 }}
          />
          <Button icon={<ReloadOutlined />} onClick={() => void loadItems()}>
            刷新
          </Button>
        </div>
        {toolbarExtra?.(helpers)}
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={tableColumns}
        dataSource={items}
        scroll={{ x: 1000 }}
        expandable={expandable}
        locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="暂无数据" /> }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) => void loadItems(buildQuery(pageIndex, pageSize)),
        }}
      />
    </section>
  );
}

export function statusTag(status?: string) {
  return <Tag color={status === 'Enabled' ? 'green' : 'default'}>{status === 'Enabled' ? '启用' : '禁用'}</Tag>;
}

export function formatDateTime(value?: string | null) {
  return value ? new Date(value).toLocaleString() : '-';
}
