import {
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import {
  App as AntApp,
  Descriptions,
  Drawer,
  Form,
  Input,
  Modal,
  Select,
  Table,
  Tag,
  Tree,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useMemo, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { menuService } from '../../services/menus';
import { roleService } from '../../services/roles';
import { useAuth } from '../../stores/authStore';
import type { PageQuery } from '../../types/api';
import type { MenuItem } from '../../types/menu';
import type { RoleItem, RoleUpsertRequest } from '../../types/role';
import { flattenMenus, toTreeData } from '../../utils/menuTree';

const defaultQuery: PageQuery = { pageIndex: 1, pageSize: 20 };

const dataScopeLabels = {
  All: '全部数据',
  OwnDepartment: '本部门数据',
};

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function RoleManagementPage() {
  const { message } = AntApp.useApp();
  const { refreshMe } = useAuth();
  const [form] = Form.useForm<RoleUpsertRequest>();
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [menus, setMenus] = useState<MenuItem[]>([]);
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<RoleItem | null>(null);
  const [detailTarget, setDetailTarget] = useState<RoleItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [menuTarget, setMenuTarget] = useState<RoleItem | null>(null);
  const [checkedMenuIds, setCheckedMenuIds] = useState<React.Key[]>([]);

  const flatMenus = useMemo(() => flattenMenus(menus), [menus]);
  const treeData = useMemo(() => toTreeData(menus), [menus]);
  const menuNameById = useMemo(
    () => new Map(flatMenus.map((menu) => [menu.id, menu.name])),
    [flatMenus],
  );

  const loadRoles = async (nextQuery = query) => {
    setLoading(true);
    try {
      const result = await roleService.list(nextQuery);
      setRoles(result.items);
      setTotal(result.total);
      setQuery(nextQuery);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '角色加载失败');
    } finally {
      setLoading(false);
    }
  };

  const loadMenus = async () => {
    try {
      setMenus(await menuService.tree());
    } catch {
      setMenus([]);
    }
  };

  useEffect(() => {
    void loadRoles(defaultQuery);
    void loadMenus();
  }, []);

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({ status: 'Enabled', dataScope: 'All' });
    setDrawerOpen(true);
  };

  const openEdit = (record: RoleItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(record);
    setDrawerOpen(true);
  };

  const submitRole = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await roleService.update(editing.id, values);
      } else {
        await roleService.create(values);
      }
      message.success('保存成功');
      setDrawerOpen(false);
      await loadRoles();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeRole = (record: RoleItem) => {
    Modal.confirm({
      title: '删除角色',
      content: `确认删除角色 ${record.name}？已分配给用户的角色后端会拒绝删除。`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await roleService.remove(record.id);
        message.success('删除成功');
        await loadRoles();
      },
    });
  };

  const toggleStatus = (record: RoleItem) => {
    const nextStatus = record.status === 'Enabled' ? 'Disabled' : 'Enabled';
    Modal.confirm({
      title: nextStatus === 'Enabled' ? '启用角色' : '禁用角色',
      content: `确认${nextStatus === 'Enabled' ? '启用' : '禁用'}角色 ${record.name}？`,
      okText: nextStatus === 'Enabled' ? '启用' : '禁用',
      cancelText: '取消',
      onOk: async () => {
        await roleService.update(record.id, { ...record, status: nextStatus });
        message.success('状态已更新');
        await loadRoles();
      },
    });
  };

  const columns: ColumnsType<RoleItem> = [
    { title: '角色名称', dataIndex: 'name', width: 180 },
    { title: '角色编码', dataIndex: 'code', width: 180 },
    { title: '描述', dataIndex: 'description', render: (value) => value || '-' },
    {
      title: '数据范围',
      dataIndex: 'dataScope',
      width: 120,
      render: (value: keyof typeof dataScopeLabels) => dataScopeLabels[value] ?? value,
    },
    { title: '用户数', dataIndex: 'userCount', width: 90, render: (value) => value ?? 0 },
    {
      title: '权限数',
      dataIndex: 'menuIds',
      width: 90,
      render: (value?: number[]) => value?.length ?? 0,
    },
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
      width: 310,
      fixed: 'right',
      render: (_, record) => (
        <div className="table-actions table-actions-wide">
          <PermissionButton permission="role:view" type="link" icon={<EyeOutlined />} onClick={() => setDetailTarget(record)}>
            详情
          </PermissionButton>
          <PermissionButton permission="role:update" type="link" onClick={() => openEdit(record)}>
            编辑
          </PermissionButton>
          <PermissionButton
            permission="role:update"
            type="link"
            disabled={record.isBuiltIn && record.code === 'super_admin'}
            title={record.isBuiltIn && record.code === 'super_admin' ? 'super_admin 内置角色不可编辑授权' : undefined}
            onClick={() => {
              setMenuTarget(record);
              setCheckedMenuIds(record.menuIds ?? []);
            }}
          >
            授权
          </PermissionButton>
          <PermissionButton
            permission="role:update"
            type="link"
            disabled={record.isBuiltIn}
            onClick={() => toggleStatus(record)}
          >
            {record.status === 'Enabled' ? '禁用' : '启用'}
          </PermissionButton>
          <PermissionButton
            permission="role:delete"
            type="link"
            danger
            disabled={record.isBuiltIn}
            onClick={() => removeRole(record)}
          >
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
            <SafetyCertificateOutlined />
            角色权限
          </div>
          <h1>角色管理</h1>
          <p>管理角色编码、启停状态和可访问菜单权限。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{total}</strong>
            角色
          </span>
          <span>
            <strong>{roles.filter((item) => item.status === 'Enabled').length}</strong>
            启用
          </span>
          <span>
            <strong>{roles.filter((item) => item.isBuiltIn).length}</strong>
            内置
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索角色名称、编码"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => void loadRoles({ ...defaultQuery, keyword: value })}
            style={{ width: 280 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="role:view" onClick={() => void loadRoles()}>
            刷新
          </PermissionButton>
        </div>
        <PermissionButton type="primary" icon={<PlusOutlined />} permission="role:create" onClick={openCreate}>
          新建角色
        </PermissionButton>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={roles}
        scroll={{ x: 1200 }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) =>
            void loadRoles({ pageIndex, pageSize, keyword }),
        }}
      />
      <Drawer
        title={editing ? '编辑角色' : '新建角色'}
        width={460}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <PermissionButton type="primary" permission={editing ? 'role:update' : 'role:create'} onClick={() => void submitRole()}>
            保存
          </PermissionButton>
        }
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Form.Item name="name" label="角色名称" rules={[{ required: true, message: '请输入角色名称' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="code" label="角色编码" rules={[{ required: true, message: '请输入角色编码' }]}>
            <Input disabled={Boolean(editing)} placeholder="例如 finance_admin" />
          </Form.Item>
          <Form.Item name="description" label="描述">
            <Input.TextArea rows={3} placeholder="说明该角色的职责范围" />
          </Form.Item>
          <Form.Item name="dataScope" label="数据范围" rules={[{ required: true, message: '请选择数据范围' }]}>
            <Select
              options={[
                { label: '全部数据', value: 'All' },
                { label: '本部门数据', value: 'OwnDepartment' },
              ]}
            />
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
      <Drawer
        title="角色详情"
        width={520}
        open={Boolean(detailTarget)}
        onClose={() => setDetailTarget(null)}
      >
        {detailTarget && (
          <Descriptions column={1} size="middle" bordered>
            <Descriptions.Item label="角色名称">{detailTarget.name}</Descriptions.Item>
            <Descriptions.Item label="角色编码">{detailTarget.code}</Descriptions.Item>
            <Descriptions.Item label="描述">{detailTarget.description || '-'}</Descriptions.Item>
            <Descriptions.Item label="数据范围">{dataScopeLabels[detailTarget.dataScope]}</Descriptions.Item>
            <Descriptions.Item label="已分配用户">{detailTarget.userCount ?? 0}</Descriptions.Item>
            <Descriptions.Item label="状态">{detailTarget.status === 'Enabled' ? '启用' : '禁用'}</Descriptions.Item>
            <Descriptions.Item label="内置角色">{detailTarget.isBuiltIn ? '是' : '否'}</Descriptions.Item>
            <Descriptions.Item label="创建时间">{formatDateTime(detailTarget.createdAt)}</Descriptions.Item>
            <Descriptions.Item label="已授权">
              {detailTarget.menuIds?.length
                ? detailTarget.menuIds.map((id) => menuNameById.get(id) ?? id).join(', ')
                : '未授权'}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
      <Modal
        title={`角色授权${menuTarget ? ` - ${menuTarget.name}` : ''}`}
        open={Boolean(menuTarget)}
        width={620}
        onCancel={() => setMenuTarget(null)}
        onOk={async () => {
          if (!menuTarget) return;
          await roleService.updateMenus(menuTarget.id, {
            menuIds: checkedMenuIds.map(Number),
          });
          message.success('授权已更新');
          setMenuTarget(null);
          await Promise.all([loadRoles(), refreshMe()]);
        }}
      >
        <div className="modal-hint">
          页面权限控制左侧菜单显示；按钮/API 权限控制页面按钮可见性和后端接口访问。
        </div>
        <Tree
          className="permission-tree"
          checkable
          defaultExpandAll
          treeData={treeData}
          checkedKeys={checkedMenuIds}
          onCheck={(keys) =>
            setCheckedMenuIds(Array.isArray(keys) ? keys : keys.checked)
          }
        />
      </Modal>
    </section>
  );
}
