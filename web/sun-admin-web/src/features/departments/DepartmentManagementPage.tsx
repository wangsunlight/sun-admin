import {
  ApartmentOutlined,
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import {
  App as AntApp,
  Descriptions,
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
import { useEffect, useMemo, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { departmentService } from '../../services/departments';
import type { DepartmentItem, DepartmentUpsertRequest } from '../../types/department';

function flattenDepartments(items: DepartmentItem[]) {
  const result: DepartmentItem[] = [];
  const walk = (nodes: DepartmentItem[]) => {
    nodes.forEach((node) => {
      result.push(node);
      if (node.children?.length) {
        walk(node.children);
      }
    });
  };
  walk(items);
  return result;
}

function filterDepartmentTree(items: DepartmentItem[], keyword: string): DepartmentItem[] {
  const value = keyword.trim().toLowerCase();
  if (!value) return items;

  const result: DepartmentItem[] = [];
  items.forEach((item) => {
    const children = item.children ? filterDepartmentTree(item.children, value) : [];
    const matched = [item.code, item.name, item.leader, item.phone, item.email]
      .some((field) => field?.toLowerCase().includes(value));
    if (matched || children.length) {
      result.push({ ...item, children });
    }
  });
  return result;
}

export default function DepartmentManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<DepartmentUpsertRequest>();
  const [departments, setDepartments] = useState<DepartmentItem[]>([]);
  const [keyword, setKeyword] = useState('');
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<DepartmentItem | null>(null);
  const [detailTarget, setDetailTarget] = useState<DepartmentItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const flatDepartments = useMemo(() => flattenDepartments(departments), [departments]);
  const filteredDepartments = useMemo(() => filterDepartmentTree(departments, keyword), [departments, keyword]);
  const parentOptions = useMemo(() => {
    const blockedIds = editing
      ? new Set([editing.id, ...flattenDepartments(editing.children ?? []).map((item) => item.id)])
      : new Set<number>();
    return flatDepartments
      .filter((item) => !blockedIds.has(item.id))
      .map((item) => ({ label: `${item.name} (${item.code})`, value: item.id }));
  }, [editing, flatDepartments]);
  const parentNameById = useMemo(() => new Map(flatDepartments.map((item) => [item.id, item.name])), [flatDepartments]);

  const loadDepartments = async () => {
    setLoading(true);
    try {
      setDepartments(await departmentService.tree());
    } catch (error) {
      message.error(error instanceof Error ? error.message : '部门加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadDepartments();
  }, []);

  const openCreate = (parent?: DepartmentItem) => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({
      parentId: parent?.id ?? null,
      sortOrder: 0,
      status: 'Enabled',
    });
    setDrawerOpen(true);
  };

  const openEdit = (record: DepartmentItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(record);
    setDrawerOpen(true);
  };

  const submitDepartment = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await departmentService.update(editing.id, values);
      } else {
        await departmentService.create(values);
      }
      message.success('保存成功');
      setDrawerOpen(false);
      await loadDepartments();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeDepartment = (record: DepartmentItem) => {
    Modal.confirm({
      title: '删除部门',
      content: `确认删除部门 ${record.name}？存在子部门或内置部门时后端会拒绝删除。`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await departmentService.remove(record.id);
        message.success('删除成功');
        await loadDepartments();
      },
    });
  };

  const toggleStatus = (record: DepartmentItem) => {
    const nextStatus = record.status === 'Enabled' ? 'Disabled' : 'Enabled';
    Modal.confirm({
      title: nextStatus === 'Enabled' ? '启用部门' : '禁用部门',
      content: `确认${nextStatus === 'Enabled' ? '启用' : '禁用'}部门 ${record.name}？`,
      okText: nextStatus === 'Enabled' ? '启用' : '禁用',
      cancelText: '取消',
      onOk: async () => {
        await departmentService.update(record.id, { ...record, status: nextStatus });
        message.success('状态已更新');
        await loadDepartments();
      },
    });
  };

  const columns: ColumnsType<DepartmentItem> = [
    { title: '部门名称', dataIndex: 'name', width: 220 },
    { title: '部门编码', dataIndex: 'code', width: 150 },
    { title: '负责人', dataIndex: 'leader', width: 140, render: (value) => value || '-' },
    { title: '联系电话', dataIndex: 'phone', width: 150, render: (value) => value || '-' },
    { title: '邮箱', dataIndex: 'email', width: 210, render: (value) => value || '-' },
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
      title: '操作',
      width: 340,
      fixed: 'right',
      render: (_, record) => (
        <div className="table-actions table-actions-wide">
          <PermissionButton permission="department:view" type="link" icon={<EyeOutlined />} onClick={() => setDetailTarget(record)}>
            详情
          </PermissionButton>
          <PermissionButton permission="department:create" type="link" onClick={() => openCreate(record)}>
            新增子级
          </PermissionButton>
          <PermissionButton permission="department:update" type="link" onClick={() => openEdit(record)}>
            编辑
          </PermissionButton>
          <PermissionButton permission="department:update" type="link" disabled={record.isBuiltIn} onClick={() => toggleStatus(record)}>
            {record.status === 'Enabled' ? '禁用' : '启用'}
          </PermissionButton>
          <PermissionButton permission="department:delete" type="link" danger disabled={record.isBuiltIn || Boolean(record.children?.length)} onClick={() => removeDepartment(record)}>
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
            <ApartmentOutlined />
            组织架构
          </div>
          <h1>部门管理</h1>
          <p>维护企业部门层级、负责人和基础联系信息。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{flatDepartments.length}</strong>
            部门
          </span>
          <span>
            <strong>{flatDepartments.filter((item) => item.status === 'Enabled').length}</strong>
            启用
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索名称、编码、负责人"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            style={{ width: 300 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="department:view" onClick={() => void loadDepartments()}>
            刷新
          </PermissionButton>
        </div>
        <PermissionButton type="primary" icon={<PlusOutlined />} permission="department:create" onClick={() => openCreate()}>
          新建部门
        </PermissionButton>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={filteredDepartments}
        pagination={false}
        scroll={{ x: 1350 }}
      />
      <Drawer
        title={editing ? '编辑部门' : '新建部门'}
        width={500}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <PermissionButton type="primary" permission={editing ? 'department:update' : 'department:create'} onClick={() => void submitDepartment()}>
            保存
          </PermissionButton>
        }
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Form.Item name="parentId" label="上级部门">
            <Select allowClear options={parentOptions} placeholder="根部门" />
          </Form.Item>
          <Form.Item name="name" label="部门名称" rules={[{ required: true, message: '请输入部门名称' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="code" label="部门编码" rules={[{ required: true, message: '请输入部门编码' }]}>
            <Input placeholder="例如 TECH" />
          </Form.Item>
          <Form.Item name="leader" label="负责人">
            <Input />
          </Form.Item>
          <Form.Item name="phone" label="联系电话">
            <Input />
          </Form.Item>
          <Form.Item name="email" label="邮箱" rules={[{ type: 'email', message: '请输入正确的邮箱地址' }]}>
            <Input />
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
      <Drawer title="部门详情" width={500} open={Boolean(detailTarget)} onClose={() => setDetailTarget(null)}>
        {detailTarget && (
          <Descriptions column={1} size="middle" bordered>
            <Descriptions.Item label="部门名称">{detailTarget.name}</Descriptions.Item>
            <Descriptions.Item label="部门编码">{detailTarget.code}</Descriptions.Item>
            <Descriptions.Item label="上级部门">
              {detailTarget.parentId ? parentNameById.get(detailTarget.parentId) ?? detailTarget.parentId : '根部门'}
            </Descriptions.Item>
            <Descriptions.Item label="负责人">{detailTarget.leader || '-'}</Descriptions.Item>
            <Descriptions.Item label="联系电话">{detailTarget.phone || '-'}</Descriptions.Item>
            <Descriptions.Item label="邮箱">{detailTarget.email || '-'}</Descriptions.Item>
            <Descriptions.Item label="排序">{detailTarget.sortOrder}</Descriptions.Item>
            <Descriptions.Item label="状态">{detailTarget.status === 'Enabled' ? '启用' : '禁用'}</Descriptions.Item>
            <Descriptions.Item label="内置部门">{detailTarget.isBuiltIn ? '是' : '否'}</Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
    </section>
  );
}
