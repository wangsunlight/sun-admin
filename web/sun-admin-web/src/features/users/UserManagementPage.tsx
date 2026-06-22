import {
  CheckCircleOutlined,
  DeleteOutlined,
  EyeOutlined,
  KeyOutlined,
  PlusOutlined,
  ReloadOutlined,
  StopOutlined,
  TeamOutlined,
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
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useMemo, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { departmentService } from '../../services/departments';
import { positionService } from '../../services/positions';
import { roleService } from '../../services/roles';
import { userService } from '../../services/users';
import type { EntityStatus } from '../../types/api';
import type { DepartmentItem } from '../../types/department';
import type { PositionItem } from '../../types/position';
import type { RoleItem } from '../../types/role';
import type { UserItem, UserQuery, UserUpsertRequest } from '../../types/user';

const defaultQuery: UserQuery = { pageIndex: 1, pageSize: 20 };

interface UserFormValues extends UserUpsertRequest {
  roleIds?: number[];
}

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

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

export default function UserManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<UserFormValues>();
  const [roleForm] = Form.useForm<{ roleIds: number[] }>();
  const [passwordForm] = Form.useForm<{ newPassword: string; confirmPassword: string }>();
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [statusFilter, setStatusFilter] = useState<EntityStatus | undefined>();
  const [roleFilter, setRoleFilter] = useState<number | undefined>();
  const [departmentFilter, setDepartmentFilter] = useState<number | undefined>();
  const [positionFilter, setPositionFilter] = useState<number | undefined>();
  const [users, setUsers] = useState<UserItem[]>([]);
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [departments, setDepartments] = useState<DepartmentItem[]>([]);
  const [positions, setPositions] = useState<PositionItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [editing, setEditing] = useState<UserItem | null>(null);
  const [detailTarget, setDetailTarget] = useState<UserItem | null>(null);
  const [roleTarget, setRoleTarget] = useState<UserItem | null>(null);
  const [passwordTarget, setPasswordTarget] = useState<UserItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const roleOptions = useMemo(
    () => roles.map((role) => ({ label: `${role.name} (${role.code})`, value: role.id })),
    [roles],
  );
  const departmentOptions = useMemo(
    () => flattenDepartments(departments).map((department) => ({ label: `${department.name} (${department.code})`, value: department.id })),
    [departments],
  );
  const positionOptions = useMemo(
    () => positions.map((position) => ({ label: `${position.name} (${position.code})`, value: position.id })),
    [positions],
  );

  const getRoleIds = (record: UserItem) =>
    roles.filter((role) => record.roles?.includes(role.code)).map((role) => role.id);

  const loadUsers = async (nextQuery = query) => {
    setLoading(true);
    try {
      const result = await userService.list(nextQuery);
      setUsers(result.items);
      setTotal(result.total);
      setQuery(nextQuery);
      setSelectedRowKeys([]);
    } catch (error) {
      message.error(error instanceof Error ? error.message : '用户加载失败');
    } finally {
      setLoading(false);
    }
  };

  const loadRoles = async () => {
    try {
      const result = await roleService.list({ pageIndex: 1, pageSize: 100 });
      setRoles(result.items);
    } catch {
      setRoles([]);
    }
  };

  const loadOrganizations = async () => {
    const [departmentResult, positionResult] = await Promise.allSettled([
      departmentService.tree(),
      positionService.list({ pageIndex: 1, pageSize: 100 }),
    ]);
    if (departmentResult.status === 'fulfilled') {
      setDepartments(departmentResult.value);
    }
    if (positionResult.status === 'fulfilled') {
      setPositions(positionResult.value.items);
    }
  };

  useEffect(() => {
    void loadUsers(defaultQuery);
    void loadRoles();
    void loadOrganizations();
  }, []);

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({ status: 'Enabled', roleIds: [] });
    setDrawerOpen(true);
  };

  const openEdit = (record: UserItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue({
      userName: record.userName,
      displayName: record.displayName,
      email: record.email,
      departmentId: record.departmentId ?? null,
      positionId: record.positionId ?? null,
      status: record.status,
    });
    setDrawerOpen(true);
  };

  const submitUser = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await userService.update(editing.id, values);
      } else {
        await userService.create(values);
      }

      message.success('保存成功');
      setDrawerOpen(false);
      await loadUsers();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeUser = (record: UserItem) => {
    Modal.confirm({
      title: '删除用户',
      content: `确认删除用户 ${record.userName}？删除后该账号将无法登录。`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await userService.remove(record.id);
        message.success('删除成功');
        await loadUsers();
      },
    });
  };

  const toggleStatus = (record: UserItem) => {
    const nextEnabled = record.status !== 'Enabled';
    Modal.confirm({
      title: nextEnabled ? '启用用户' : '禁用用户',
      content: nextEnabled
        ? `确认启用用户 ${record.userName}？`
        : `确认禁用用户 ${record.userName}？禁用后该用户无法登录。`,
      okText: nextEnabled ? '启用' : '禁用',
      cancelText: '取消',
      onOk: async () => {
        if (nextEnabled) {
          await userService.enable(record.id);
        } else {
          await userService.disable(record.id);
        }
        message.success('状态已更新');
        await loadUsers();
      },
    });
  };

  const resetPassword = async () => {
    if (!passwordTarget) return;
    const values = await passwordForm.validateFields();
    await userService.resetPassword(passwordTarget.id, values.newPassword);
    message.success('密码已重置，用户下次登录必须修改密码');
    setPasswordTarget(null);
    passwordForm.resetFields();
  };

  const buildQuery = (pageIndex = 1, pageSize = query.pageSize ?? 20): UserQuery => ({
    pageIndex,
    pageSize,
    keyword,
    status: statusFilter,
    roleId: roleFilter,
    departmentId: departmentFilter,
    positionId: positionFilter,
  });

  const batchOperate = (type: 'enable' | 'disable' | 'delete') => {
    const userIds = selectedRowKeys.map(Number);
    if (!userIds.length) {
      message.warning('请先选择用户');
      return;
    }

    const titleMap = {
      enable: '批量启用用户',
      disable: '批量禁用用户',
      delete: '批量删除用户',
    };
    Modal.confirm({
      title: titleMap[type],
      content: `确认处理已选择的 ${userIds.length} 个用户？内置账号会由后端拒绝危险操作。`,
      okText: '确认',
      cancelText: '取消',
      okType: type === 'delete' ? 'danger' : 'primary',
      onOk: async () => {
        if (type === 'enable') {
          await userService.batchEnable({ userIds });
        } else if (type === 'disable') {
          await userService.batchDisable({ userIds });
        } else {
          await userService.batchDelete({ userIds });
        }
        message.success('批量操作已完成');
        await loadUsers();
      },
    });
  };

  const columns: ColumnsType<UserItem> = [
    { title: '用户名', dataIndex: 'userName', width: 150 },
    { title: '显示名', dataIndex: 'displayName', width: 150, render: (value) => value || '-' },
    { title: '邮箱', dataIndex: 'email', width: 220, render: (value) => value || '-' },
    { title: '部门', dataIndex: 'departmentName', width: 140, render: (value) => value || '-' },
    { title: '岗位', dataIndex: 'positionName', width: 140, render: (value) => value || '-' },
    {
      title: '角色',
      dataIndex: 'roles',
      width: 220,
      render: (values?: string[]) =>
        values?.length ? values.map((item) => <Tag key={item}>{item}</Tag>) : <Tag>未分配</Tag>,
    },
    {
      title: '状态',
      dataIndex: 'status',
      width: 110,
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
      title: '改密',
      dataIndex: 'mustChangePassword',
      width: 90,
      render: (value) => (value ? <Tag color="orange">待改密</Tag> : <Tag>正常</Tag>),
    },
    { title: '最后登录', dataIndex: 'lastLoginAt', width: 180, render: formatDateTime },
    {
      title: '操作',
      width: 360,
      fixed: 'right',
      render: (_, record) => (
        <div className="table-actions table-actions-wide">
          <PermissionButton permission="user:view" type="link" icon={<EyeOutlined />} onClick={() => setDetailTarget(record)}>
            详情
          </PermissionButton>
          <PermissionButton permission="user:update" type="link" onClick={() => openEdit(record)}>
            编辑
          </PermissionButton>
          <PermissionButton
            permission="user:update"
            type="link"
            onClick={() => {
              setRoleTarget(record);
              roleForm.setFieldsValue({ roleIds: getRoleIds(record) });
            }}
          >
            角色
          </PermissionButton>
          <PermissionButton
            permission="user:update"
            type="link"
            icon={<KeyOutlined />}
            onClick={() => {
              passwordForm.resetFields();
              setPasswordTarget(record);
            }}
          >
            密码
          </PermissionButton>
          <PermissionButton
            permission="user:update"
            type="link"
            disabled={record.isBuiltIn}
            onClick={() => toggleStatus(record)}
          >
            {record.status === 'Enabled' ? '禁用' : '启用'}
          </PermissionButton>
          <PermissionButton
            permission="user:delete"
            type="link"
            danger
            disabled={record.isBuiltIn}
            onClick={() => removeUser(record)}
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
            <TeamOutlined />
            账号与访问
          </div>
          <h1>用户管理</h1>
          <p>维护系统账号、启停状态、密码安全和角色授权关系。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{total}</strong>
            用户
          </span>
          <span>
            <strong>{users.filter((item) => item.status === 'Enabled').length}</strong>
            启用
          </span>
          <span>
            <strong>{roles.length}</strong>
            角色
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Input.Search
            allowClear
            placeholder="搜索用户名、显示名、邮箱"
            value={keyword}
            onChange={(event) => setKeyword(event.target.value)}
            onSearch={(value) => {
              setKeyword(value);
              void loadUsers({ ...buildQuery(1, defaultQuery.pageSize ?? 20), keyword: value });
            }}
            style={{ width: 300 }}
          />
          <Select
            allowClear
            placeholder="状态"
            value={statusFilter}
            onChange={(value) => {
              setStatusFilter(value);
              void loadUsers({ ...buildQuery(), status: value });
            }}
            options={[
              { label: '启用', value: 'Enabled' },
              { label: '禁用', value: 'Disabled' },
            ]}
            style={{ width: 120 }}
          />
          <Select
            allowClear
            showSearch
            placeholder="角色"
            value={roleFilter}
            optionFilterProp="label"
            onChange={(value) => {
              setRoleFilter(value);
              void loadUsers({ ...buildQuery(), roleId: value });
            }}
            options={roleOptions}
            style={{ width: 180 }}
          />
          <Select
            allowClear
            showSearch
            placeholder="部门"
            value={departmentFilter}
            optionFilterProp="label"
            onChange={(value) => {
              setDepartmentFilter(value);
              void loadUsers({ ...buildQuery(), departmentId: value });
            }}
            options={departmentOptions}
            style={{ width: 180 }}
          />
          <Select
            allowClear
            showSearch
            placeholder="岗位"
            value={positionFilter}
            optionFilterProp="label"
            onChange={(value) => {
              setPositionFilter(value);
              void loadUsers({ ...buildQuery(), positionId: value });
            }}
            options={positionOptions}
            style={{ width: 180 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="user:view" onClick={() => void loadUsers()}>
            刷新
          </PermissionButton>
        </div>
        <div className="page-toolbar-actions">
          <PermissionButton icon={<CheckCircleOutlined />} permission="user:update" disabled={!selectedRowKeys.length} onClick={() => batchOperate('enable')}>
            批量启用
          </PermissionButton>
          <PermissionButton icon={<StopOutlined />} permission="user:update" disabled={!selectedRowKeys.length} onClick={() => batchOperate('disable')}>
            批量禁用
          </PermissionButton>
          <PermissionButton danger icon={<DeleteOutlined />} permission="user:delete" disabled={!selectedRowKeys.length} onClick={() => batchOperate('delete')}>
            批量删除
          </PermissionButton>
          <PermissionButton type="primary" icon={<PlusOutlined />} permission="user:create" onClick={openCreate}>
            新建用户
          </PermissionButton>
        </div>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={users}
        rowSelection={{
          selectedRowKeys,
          onChange: setSelectedRowKeys,
        }}
        scroll={{ x: 1750 }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) =>
            void loadUsers(buildQuery(pageIndex, pageSize)),
        }}
      />
      <Drawer
        title={editing ? '编辑用户' : '新建用户'}
        width={480}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <PermissionButton type="primary" permission={editing ? 'user:update' : 'user:create'} onClick={() => void submitUser()}>
            保存
          </PermissionButton>
        }
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Form.Item name="userName" label="用户名" rules={[{ required: true, message: '请输入用户名' }]}>
            <Input disabled={Boolean(editing)} placeholder="用于登录，创建后不可修改" />
          </Form.Item>
          <Form.Item name="displayName" label="显示名" rules={[{ required: true, message: '请输入显示名' }]}>
            <Input placeholder="页面展示名称" />
          </Form.Item>
          <Form.Item name="email" label="邮箱" rules={[{ type: 'email', message: '请输入正确的邮箱地址' }]}>
            <Input placeholder="user@example.com" />
          </Form.Item>
          <Form.Item name="departmentId" label="部门">
            <Select allowClear showSearch optionFilterProp="label" options={departmentOptions} placeholder="选择部门" />
          </Form.Item>
          <Form.Item name="positionId" label="岗位">
            <Select allowClear showSearch optionFilterProp="label" options={positionOptions} placeholder="选择岗位" />
          </Form.Item>
          {!editing && (
            <>
              <Form.Item name="password" label="初始密码" rules={[{ required: true, min: 8, message: '至少 8 位密码' }]}>
                <Input.Password placeholder="至少 8 位" />
              </Form.Item>
              <Form.Item name="roleIds" label="初始角色">
                <Select mode="multiple" allowClear options={roleOptions} placeholder="可稍后再分配" />
              </Form.Item>
            </>
          )}
          <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
            <Select
              suffixIcon={<CheckCircleOutlined />}
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
        title="用户详情"
        width={460}
        open={Boolean(detailTarget)}
        onClose={() => setDetailTarget(null)}
      >
        {detailTarget && (
          <Descriptions column={1} size="middle" bordered>
            <Descriptions.Item label="用户名">{detailTarget.userName}</Descriptions.Item>
            <Descriptions.Item label="显示名">{detailTarget.displayName || '-'}</Descriptions.Item>
            <Descriptions.Item label="邮箱">{detailTarget.email || '-'}</Descriptions.Item>
            <Descriptions.Item label="部门">{detailTarget.departmentName || '-'}</Descriptions.Item>
            <Descriptions.Item label="岗位">{detailTarget.positionName || '-'}</Descriptions.Item>
            <Descriptions.Item label="状态">{detailTarget.status === 'Enabled' ? '启用' : '禁用'}</Descriptions.Item>
            <Descriptions.Item label="内置账号">{detailTarget.isBuiltIn ? '是' : '否'}</Descriptions.Item>
            <Descriptions.Item label="强制改密">{detailTarget.mustChangePassword ? '是' : '否'}</Descriptions.Item>
            <Descriptions.Item label="角色">
              {detailTarget.roles?.length ? detailTarget.roles.join(', ') : '未分配'}
            </Descriptions.Item>
            <Descriptions.Item label="创建时间">{formatDateTime(detailTarget.createdAt)}</Descriptions.Item>
            <Descriptions.Item label="最后登录">{formatDateTime(detailTarget.lastLoginAt)}</Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
      <Modal
        title={`分配角色${roleTarget ? ` - ${roleTarget.userName}` : ''}`}
        open={Boolean(roleTarget)}
        onCancel={() => setRoleTarget(null)}
        onOk={async () => {
          if (!roleTarget) return;
          const values = await roleForm.validateFields();
          await userService.updateRoles(roleTarget.id, values);
          message.success('角色已更新');
          setRoleTarget(null);
          await loadUsers();
        }}
      >
        <Form form={roleForm} layout="vertical">
          <Form.Item name="roleIds" label="角色">
            <Select mode="multiple" allowClear options={roleOptions} placeholder="选择角色" />
          </Form.Item>
        </Form>
      </Modal>
      <Modal
        title={`重置密码${passwordTarget ? ` - ${passwordTarget.userName}` : ''}`}
        open={Boolean(passwordTarget)}
        onCancel={() => {
          setPasswordTarget(null);
          passwordForm.resetFields();
        }}
        onOk={() => void resetPassword()}
        okText="重置"
      >
        <Form form={passwordForm} layout="vertical">
          <Form.Item name="newPassword" label="新密码" rules={[{ required: true, min: 8, message: '至少 8 位密码' }]}>
            <Input.Password placeholder="输入新密码" />
          </Form.Item>
          <Form.Item
            name="confirmPassword"
            label="确认密码"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: '请再次输入密码' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  return !value || getFieldValue('newPassword') === value
                    ? Promise.resolve()
                    : Promise.reject(new Error('两次输入的密码不一致'));
                },
              }),
            ]}
          >
            <Input.Password placeholder="再次输入新密码" />
          </Form.Item>
        </Form>
      </Modal>
    </section>
  );
}
