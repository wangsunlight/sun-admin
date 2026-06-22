import {
  EyeOutlined,
  MenuFoldOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import {
  App as AntApp,
  Alert,
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
import { menuService } from '../../services/menus';
import type { MenuItem, MenuType, MenuUpsertRequest } from '../../types/menu';
import { flattenMenus, toParentOptions } from '../../utils/menuTree';

const menuTypeLabels: Record<MenuType, string> = {
  Directory: '目录',
  Page: '页面',
  Button: '按钮',
};

const menuTypeDescriptions: Record<MenuType, string> = {
  Directory: '目录只组织菜单层级，不需要路由路径、组件标识或权限码。',
  Page: '页面必须填写已有静态路由路径；这里只控制菜单是否显示，不会动态创建页面。',
  Button: '按钮用于操作权限，必须填写全局唯一的权限码。',
};

function filterMenuTree(items: MenuItem[], keyword: string): MenuItem[] {
  const value = keyword.trim().toLowerCase();
  if (!value) return items;

  const result: MenuItem[] = [];
  items.forEach((item) => {
    const children = item.children ? filterMenuTree(item.children, value) : [];
    const matched = [
      item.name,
      item.routePath,
      item.component,
      item.permissionCode,
    ].some((field) => field?.toLowerCase().includes(value));

    if (matched || children.length) {
      result.push({ ...item, children });
    }
  });

  return result;
}

function toMenuPayload(record: MenuItem, status = record.status): MenuUpsertRequest {
  return {
    parentId: record.parentId ?? null,
    name: record.name,
    type: record.type,
    routePath: record.routePath,
    component: record.component,
    icon: record.icon,
    permissionCode: record.permissionCode,
    sortOrder: record.sortOrder,
    status,
  };
}

function hasChildren(record: MenuItem) {
  return Boolean(record.children?.length);
}

function normalizeMenuPayload(values: MenuUpsertRequest): MenuUpsertRequest {
  const trimText = (value?: string) => {
    const trimmed = value?.trim();
    return trimmed || undefined;
  };

  const base = {
    parentId: values.parentId ?? null,
    name: values.name.trim(),
    type: values.type,
    icon: trimText(values.icon),
    sortOrder: values.sortOrder,
    status: values.status,
  };

  if (values.type === 'Page') {
    return {
      ...base,
      routePath: trimText(values.routePath),
      component: trimText(values.component),
      permissionCode: undefined,
    };
  }

  if (values.type === 'Button') {
    return {
      ...base,
      routePath: undefined,
      component: undefined,
      permissionCode: trimText(values.permissionCode),
    };
  }

  return {
    ...base,
    routePath: undefined,
    component: undefined,
    permissionCode: undefined,
  };
}

export default function MenuManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<MenuUpsertRequest>();
  const selectedType = Form.useWatch('type', form) ?? 'Page';
  const [menus, setMenus] = useState<MenuItem[]>([]);
  const [keyword, setKeyword] = useState('');
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<MenuItem | null>(null);
  const [detailTarget, setDetailTarget] = useState<MenuItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const flatMenus = useMemo(() => flattenMenus(menus), [menus]);
  const filteredMenus = useMemo(() => filterMenuTree(menus, keyword), [menus, keyword]);
  const parentOptions = useMemo(() => {
    const blockedIds = editing
      ? new Set([editing.id, ...flattenMenus(editing.children ?? []).map((item) => item.id)])
      : new Set<number>();
    return toParentOptions(menus).filter((option) => !blockedIds.has(option.value));
  }, [editing, menus]);
  const parentNameById = useMemo(
    () => new Map(flatMenus.map((menu) => [menu.id, menu.name])),
    [flatMenus],
  );

  const loadMenus = async () => {
    setLoading(true);
    try {
      setMenus(await menuService.tree());
    } catch (error) {
      message.error(error instanceof Error ? error.message : '菜单加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadMenus();
  }, []);

  const openCreate = (parent?: MenuItem) => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({
      parentId: parent?.id ?? null,
      type: parent?.type === 'Page' ? 'Button' : 'Page',
      status: 'Enabled',
      sortOrder: 0,
    });
    setDrawerOpen(true);
  };

  const openEdit = (record: MenuItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(toMenuPayload(record));
    setDrawerOpen(true);
  };

  const submitMenu = async () => {
    const values = await form.validateFields();
    const payload = normalizeMenuPayload(values);
    try {
      if (editing) {
        await menuService.update(editing.id, payload);
      } else {
        await menuService.create(payload);
      }
      message.success('保存成功');
      setDrawerOpen(false);
      await loadMenus();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeMenu = (record: MenuItem) => {
    if (record.isBuiltIn) {
      message.warning('内置菜单不可删除');
      return;
    }

    if (hasChildren(record)) {
      message.warning('有子节点的菜单不可删除');
      return;
    }

    Modal.confirm({
      title: '删除菜单',
      content: `确认删除 ${record.name}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await menuService.remove(record.id);
        message.success('删除成功');
        await loadMenus();
      },
    });
  };

  const toggleStatus = (record: MenuItem) => {
    if (record.isBuiltIn) {
      message.warning('内置菜单不可禁用');
      return;
    }

    const nextStatus = record.status === 'Enabled' ? 'Disabled' : 'Enabled';
    Modal.confirm({
      title: nextStatus === 'Enabled' ? '启用菜单' : '禁用菜单',
      content: `确认${nextStatus === 'Enabled' ? '启用' : '禁用'} ${record.name}？`,
      okText: nextStatus === 'Enabled' ? '启用' : '禁用',
      cancelText: '取消',
      onOk: async () => {
        await menuService.update(record.id, toMenuPayload(record, nextStatus));
        message.success('状态已更新');
        await loadMenus();
      },
    });
  };

  const columns: ColumnsType<MenuItem> = [
    { title: '名称', dataIndex: 'name', width: 220 },
    {
      title: '类型',
      dataIndex: 'type',
      width: 90,
      render: (value: MenuType) => <Tag>{menuTypeLabels[value]}</Tag>,
    },
    { title: '路由', dataIndex: 'routePath', width: 170, render: (value) => value || '-' },
    { title: '组件', dataIndex: 'component', width: 190, render: (value) => value || '-' },
    { title: '权限码', dataIndex: 'permissionCode', width: 190, render: (value) => value || '-' },
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
      width: 350,
      fixed: 'right',
      render: (_, record) => (
        <div className="table-actions table-actions-wide">
          <PermissionButton permission="menu:view" type="link" icon={<EyeOutlined />} onClick={() => setDetailTarget(record)}>
            详情
          </PermissionButton>
          {record.type !== 'Button' && (
            <PermissionButton permission="menu:create" type="link" onClick={() => openCreate(record)}>
              新增子级
            </PermissionButton>
          )}
          <PermissionButton permission="menu:update" type="link" onClick={() => openEdit(record)}>
            编辑
          </PermissionButton>
          <PermissionButton
            permission="menu:update"
            type="link"
            disabled={record.isBuiltIn}
            onClick={() => toggleStatus(record)}
          >
            {record.status === 'Enabled' ? '禁用' : '启用'}
          </PermissionButton>
          <PermissionButton
            permission="menu:delete"
            type="link"
            danger
            disabled={record.isBuiltIn || hasChildren(record)}
            onClick={() => removeMenu(record)}
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
            <MenuFoldOutlined />
            导航结构
          </div>
          <h1>菜单管理</h1>
          <p>维护目录、静态路由菜单显示和按钮权限，页面不会由菜单配置动态创建。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{flatMenus.length}</strong>
            节点
          </span>
          <span>
            <strong>{flatMenus.filter((item) => item.status === 'Enabled').length}</strong>
            启用
          </span>
          <span>
            <strong>{flatMenus.filter((item) => item.type === 'Button').length}</strong>
            权限
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索名称、路由、权限码"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            style={{ width: 300 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="menu:view" onClick={() => void loadMenus()}>
            刷新
          </PermissionButton>
        </div>
        <PermissionButton type="primary" icon={<PlusOutlined />} permission="menu:create" onClick={() => openCreate()}>
          新建菜单
        </PermissionButton>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={filteredMenus}
        pagination={false}
        scroll={{ x: 1400 }}
      />
      <Drawer
        title={editing ? '编辑菜单' : '新建菜单'}
        width={500}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <PermissionButton type="primary" permission={editing ? 'menu:update' : 'menu:create'} onClick={() => void submitMenu()}>
            保存
          </PermissionButton>
        }
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Alert className="menu-rule-alert" type="info" showIcon message={menuTypeDescriptions[selectedType]} />
          <Form.Item name="parentId" label="父级菜单">
            <Select allowClear options={parentOptions} placeholder="根节点" />
          </Form.Item>
          <Form.Item name="name" label="名称" rules={[{ required: true, message: '请输入菜单名称' }]}>
            <Input />
          </Form.Item>
          <Form.Item name="type" label="类型" rules={[{ required: true, message: '请选择类型' }]}>
            <Select
              onChange={(value: MenuType) => {
                form.setFieldsValue({
                  routePath: value === 'Page' ? form.getFieldValue('routePath') : undefined,
                  component: value === 'Page' ? form.getFieldValue('component') : undefined,
                  permissionCode: value === 'Button' ? form.getFieldValue('permissionCode') : undefined,
                });
              }}
              options={[
                { label: '目录', value: 'Directory' },
                { label: '页面', value: 'Page' },
                { label: '按钮', value: 'Button' },
              ]}
            />
          </Form.Item>
          <Form.Item
            name="routePath"
            label="路由路径"
            extra={selectedType === 'Page' ? '填写已经在前端代码中注册的静态路由，例如 /users。' : '当前类型不使用路由路径。'}
            rules={[{ required: selectedType === 'Page', message: '页面必须填写路由路径' }]}
          >
            <Input disabled={selectedType !== 'Page'} placeholder="/users" />
          </Form.Item>
          <Form.Item
            name="component"
            label="组件标识"
            extra={selectedType === 'Page' ? '仅作为备注或兼容字段，不会触发动态组件加载。' : '当前类型不使用组件标识。'}
          >
            <Input disabled={selectedType !== 'Page'} placeholder="users/UserManagementPage" />
          </Form.Item>
          <Form.Item name="icon" label="图标">
            <Input placeholder="TeamOutlined" />
          </Form.Item>
          <Form.Item
            name="permissionCode"
            label="权限码"
            extra={selectedType === 'Button' ? '权限码必须全局唯一，通常使用 resource:action 格式。' : '目录和页面不需要权限码。'}
            rules={[{ required: selectedType === 'Button', message: '按钮必须填写权限码' }]}
          >
            <Input disabled={selectedType !== 'Button'} placeholder="user:create" />
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
      <Drawer
        title="菜单详情"
        width={500}
        open={Boolean(detailTarget)}
        onClose={() => setDetailTarget(null)}
      >
        {detailTarget && (
          <Descriptions column={1} size="middle" bordered>
            <Descriptions.Item label="名称">{detailTarget.name}</Descriptions.Item>
            <Descriptions.Item label="类型">{menuTypeLabels[detailTarget.type]}</Descriptions.Item>
            <Descriptions.Item label="父级">
              {detailTarget.parentId ? parentNameById.get(detailTarget.parentId) ?? detailTarget.parentId : '根节点'}
            </Descriptions.Item>
            <Descriptions.Item label="路由">{detailTarget.routePath || '-'}</Descriptions.Item>
            <Descriptions.Item label="组件">{detailTarget.component || '-'}</Descriptions.Item>
            <Descriptions.Item label="图标">{detailTarget.icon || '-'}</Descriptions.Item>
            <Descriptions.Item label="权限码">{detailTarget.permissionCode || '-'}</Descriptions.Item>
            <Descriptions.Item label="排序">{detailTarget.sortOrder}</Descriptions.Item>
            <Descriptions.Item label="状态">{detailTarget.status === 'Enabled' ? '启用' : '禁用'}</Descriptions.Item>
            <Descriptions.Item label="内置菜单">{detailTarget.isBuiltIn ? '是' : '否'}</Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
    </section>
  );
}
