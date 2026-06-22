import {
  CheckCircleOutlined,
  EyeOutlined,
  KeyOutlined,
  PlusOutlined,
  ReloadOutlined,
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
import { roleService } from '../../services/roles';
import { userService } from '../../services/users';
import type { PageQuery } from '../../types/api';
import type { RoleItem } from '../../types/role';
import type { UserItem, UserUpsertRequest } from '../../types/user';

const defaultQuery: PageQuery = { pageIndex: 1, pageSize: 20 };

interface UserFormValues extends UserUpsertRequest {
  roleIds?: number[];
}

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function UserManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<UserFormValues>();
  const [roleForm] = Form.useForm<{ roleIds: number[] }>();
  const [passwordForm] = Form.useForm<{ newPassword: string; confirmPassword: string }>();
  const [query, setQuery] = useState(defaultQuery);
  const [keyword, setKeyword] = useState('');
  const [users, setUsers] = useState<UserItem[]>([]);
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<UserItem | null>(null);
  const [detailTarget, setDetailTarget] = useState<UserItem | null>(null);
  const [roleTarget, setRoleTarget] = useState<UserItem | null>(null);
  const [passwordTarget, setPasswordTarget] = useState<UserItem | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const roleOptions = useMemo(
    () => roles.map((role) => ({ label: `${role.name} (${role.code})`, value: role.id })),
    [roles],
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

  useEffect(() => {
    void loadUsers(defaultQuery);
    void loadRoles();
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
        const created = await userService.create(values);
        if (values.roleIds?.length) {
          await userService.updateRoles(created.id, { roleIds: values.roleIds });
        }
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
    message.success('密码已重置');
    setPasswordTarget(null);
    passwordForm.resetFields();
  };

  const columns: ColumnsType<UserItem> = [
    { title: '用户名', dataIndex: 'userName', width: 150 },
    { title: '显示名', dataIndex: 'displayName', width: 150, render: (value) => value || '-' },
    { title: '邮箱', dataIndex: 'email', width: 220, render: (value) => value || '-' },
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
            onSearch={(value) => void loadUsers({ ...defaultQuery, keyword: value })}
            style={{ width: 300 }}
          />
          <PermissionButton icon={<ReloadOutlined />} permission="user:view" onClick={() => void loadUsers()}>
            刷新
          </PermissionButton>
        </div>
        <PermissionButton type="primary" icon={<PlusOutlined />} permission="user:create" onClick={openCreate}>
          新建用户
        </PermissionButton>
      </div>
      <Table
        rowKey="id"
        loading={loading}
        columns={columns}
        dataSource={users}
        scroll={{ x: 1450 }}
        pagination={{
          current: query.pageIndex,
          pageSize: query.pageSize,
          total,
          showSizeChanger: true,
          showTotal: (value) => `共 ${value} 条`,
          onChange: (pageIndex, pageSize) =>
            void loadUsers({ pageIndex, pageSize, keyword }),
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
            <Descriptions.Item label="状态">{detailTarget.status === 'Enabled' ? '启用' : '禁用'}</Descriptions.Item>
            <Descriptions.Item label="内置账号">{detailTarget.isBuiltIn ? '是' : '否'}</Descriptions.Item>
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
