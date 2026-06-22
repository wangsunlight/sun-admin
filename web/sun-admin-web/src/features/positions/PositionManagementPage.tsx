import {
  IdcardOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import {
  App as AntApp,
  Drawer,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Table,
  Tag,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { positionService } from '../../services/positions';
import type { PageQuery } from '../../types/api';
import type { PositionItem, PositionUpsertRequest } from '../../types/position';

const defaultQuery: PageQuery = { pageIndex: 1, pageSize: 20 };

export default function PositionManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<PositionUpsertRequest>();
  const [positions, setPositions] = useState<PositionItem[]>([]);
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<PositionItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const loadPositions = async (nextQuery = query) => {
    setLoading(true);
    try {
      const result = await positionService.list(nextQuery);
      setPositions(result.items);
      setTotal(result.total);
      setQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '岗位加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadPositions(defaultQuery);
  }, []);

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({ sortOrder: 0, status: 'Enabled' });
    setDrawerOpen(true);
  };

  const openEdit = (record: PositionItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(record);
    setDrawerOpen(true);
  };

  const submitPosition = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await positionService.update(editing.id, values);
      } else {
        await positionService.create(values);
      }
      message.success('保存成功');
      setDrawerOpen(false);
      await loadPositions();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removePosition = (record: PositionItem) => {
    Modal.confirm({
      title: '删除岗位',
      content: `确认删除岗位 ${record.name}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await positionService.remove(record.id);
        message.success('删除成功');
        await loadPositions();
      },
    });
  };

  const toggleStatus = (record: PositionItem) => {
    const nextStatus = record.status === 'Enabled' ? 'Disabled' : 'Enabled';
    Modal.confirm({
      title: nextStatus === 'Enabled' ? '启用岗位' : '禁用岗位',
      content: `确认${nextStatus === 'Enabled' ? '启用' : '禁用'}岗位 ${record.name}？`,
      okText: nextStatus === 'Enabled' ? '启用' : '禁用',
      cancelText: '取消',
      onOk: async () => {
        await positionService.update(record.id, { ...record, status: nextStatus });
        message.success('状态已更新');
        await loadPositions();
      },
    });
  };

  const columns: ColumnsType<PositionItem> = [
    { title: '岗位名称', dataIndex: 'name', width: 180 },
    { title: '岗位编码', dataIndex: 'code', width: 180 },
    { title: '描述', dataIndex: 'description', render: (value) => value || '-' },
    { title: '排序', dataIndex: 'sortOrder', width: 90 },
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
    {
      title: '内置',
      dataIndex: 'isBuiltIn',
      width: 90,
      render: (value) => (value ? <Tag color="blue">是</Tag> : <Tag>否</Tag>),
    },
    {
      title: '操作',
      width: 250,
      fixed: 'right',
      render: (_, record) => (
        <div className="table-actions">
          <PermissionButton permission="position:update" type="link" onClick={() => openEdit(record)}>
            编辑
          </PermissionButton>
          <PermissionButton permission="position:update" type="link" disabled={record.isBuiltIn} onClick={() => toggleStatus(record)}>
            {record.status === 'Enabled' ? '禁用' : '启用'}
          </PermissionButton>
          <PermissionButton permission="position:delete" type="link" danger disabled={record.isBuiltIn} onClick={() => removePosition(record)}>
            删除
          </PermissionButton>
        </div>
      ),
    },
  ];

  return (
    <section className="page-surface">
      <div className="page-heading">
        <div>
          <div className="page-kicker">
            <IdcardOutlined />
            组织岗位
          </div>
          <h1>岗位管理</h1>
          <p>维护组织岗位编码、职责说明和启停状态。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{total}</strong>
            岗位
          </span>
          <span>
            <strong>{positions.filter((item) => item.status === 'Enabled').length}</strong>
            启用
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索名称、编码"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => void loadPositions({ ...defaultQuery, keyword: value })}
            style={{ width: 280 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="position:view" onClick={() => void loadPositions()}>
            刷新
          </PermissionButton>
        </div>
        <PermissionButton type="primary" icon={<PlusOutlined />} permission="position:create" onClick={openCreate}>
          新建岗位
        </PermissionButton>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={positions}
        scroll={{ x: 1000 }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) =>
            void loadPositions({ pageIndex, pageSize, keyword }),
        }}
      />
      <Drawer
        title={editing ? '编辑岗位' : '新建岗位'}
        width={460}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <PermissionButton type="primary" permission={editing ? 'position:update' : 'position:create'} onClick={() => void submitPosition()}>
            保存
          </PermissionButton>
        }
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Form.Item name="name" label="岗位名称" rules={[{ required: true, message: '请输入岗位名称' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="code" label="岗位编码" rules={[{ required: true, message: '请输入岗位编码' }]}>
            <Input placeholder="例如 ENGINEER" />
          </Form.Item>
          <Form.Item name="description" label="描述">
            <Input.TextArea rows={3} placeholder="说明岗位职责范围" />
          </Form.Item>
          <Form.Item name="sortOrder" label="排序" rules={[{ required: true, message: '请输入排序值' }]}>
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
            <Select
              disabled={Boolean(editing?.isBuiltIn)}
              options={[
                { label: '启用', value: 'Enabled' },
                { label: '禁用', value: 'Disabled' },
              ]}
            />
          </Form.Item>
        </Form>
      </Drawer>
    </section>
  );
}
