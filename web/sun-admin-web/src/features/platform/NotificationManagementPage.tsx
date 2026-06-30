import { PlusOutlined } from '@ant-design/icons';
import { App as AntApp, Checkbox, Form, Input, Modal, Select, Space, Tag } from 'antd';
import { useRef, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { notificationService } from '../../services/platform';
import type {
  NotificationCreateRequest,
  NotificationItem,
  NotificationUpdateRequest,
} from '../../types/platform';
import { formatDateTime, PlatformListPage, type PlatformListHelpers, statusTag } from './PlatformListPage';

const levelColors: Record<string, string> = {
  Info: 'blue',
  Success: 'green',
  Warning: 'orange',
  Error: 'red',
};

export default function NotificationManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<NotificationCreateRequest & Partial<NotificationUpdateRequest>>();
  const [editing, setEditing] = useState<NotificationItem | null>(null);
  const [open, setOpen] = useState(false);
  const reloadRef = useRef<PlatformListHelpers['reload']>(async () => {});

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({ level: 'Info', isPinned: false, status: 'Enabled' });
    setOpen(true);
  };

  const openEdit = (record: NotificationItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(record);
    setOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await notificationService.update(editing.id, {
          title: values.title,
          content: values.content,
          level: values.level,
          publishAt: values.publishAt || null,
          expiresAt: values.expiresAt || null,
          isPinned: values.isPinned,
          status: values.status ?? editing.status,
        });
      } else {
        await notificationService.create({
          title: values.title,
          content: values.content,
          level: values.level,
          publishAt: values.publishAt || null,
          expiresAt: values.expiresAt || null,
          isPinned: values.isPinned,
        });
      }
      message.success('保存成功');
      setOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const remove = (record: NotificationItem, helpers: PlatformListHelpers) => {
    Modal.confirm({
      title: '删除通知',
      content: `确认删除通知 ${record.title}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await notificationService.remove(record.id);
        message.success('删除成功');
        await helpers.reload();
      },
    });
  };

  return (
    <>
      <PlatformListPage<NotificationItem>
        title="通知公告"
        kicker="消息中心"
        description="管理系统通知、公告和面向后台用户的运营提示。"
        load={notificationService.list}
        toolbarExtra={(helpers) => {
          reloadRef.current = helpers.reload;
          return (
            <PermissionButton type="primary" icon={<PlusOutlined />} permission="notification:create" onClick={openCreate}>
              新建通知
            </PermissionButton>
          );
        }}
        actions={(record, helpers) => (
          <Space>
            <PermissionButton type="link" permission="notification:update" onClick={() => openEdit(record)}>
              编辑
            </PermissionButton>
            <PermissionButton type="link" danger permission="notification:delete" onClick={() => remove(record, helpers)}>
              删除
            </PermissionButton>
          </Space>
        )}
        columns={[
          { title: '标题', dataIndex: 'title', width: 220 },
          { title: '级别', dataIndex: 'level', width: 100, render: (value) => <Tag color={levelColors[value] ?? 'default'}>{value}</Tag> },
          { title: '置顶', dataIndex: 'isPinned', width: 90, render: (value) => (value ? <Tag color="gold">置顶</Tag> : '-') },
          { title: '状态', dataIndex: 'status', width: 100, render: statusTag },
          { title: '发布时间', dataIndex: 'publishAt', width: 180, render: formatDateTime },
          { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
          { title: '内容', dataIndex: 'content', ellipsis: true },
        ]}
      />
      <Modal
        title={editing ? '编辑通知' : '新建通知'}
        width={680}
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => void submit()}
        okText="保存"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item name="title" label="标题" rules={[{ required: true, message: '请输入标题' }]}>
            <Input maxLength={128} />
          </Form.Item>
          <Form.Item name="level" label="级别" rules={[{ required: true, message: '请选择级别' }]}>
            <Select
              options={[
                { label: 'Info', value: 'Info' },
                { label: 'Success', value: 'Success' },
                { label: 'Warning', value: 'Warning' },
                { label: 'Error', value: 'Error' },
              ]}
            />
          </Form.Item>
          <Form.Item name="publishAt" label="发布时间">
            <Input placeholder="2026-06-29T10:00:00Z" />
          </Form.Item>
          <Form.Item name="expiresAt" label="过期时间">
            <Input placeholder="2026-07-29T10:00:00Z" />
          </Form.Item>
          {editing && (
            <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
              <Select
                options={[
                  { label: '启用', value: 'Enabled' },
                  { label: '禁用', value: 'Disabled' },
                ]}
              />
            </Form.Item>
          )}
          <Form.Item name="isPinned" valuePropName="checked">
            <Checkbox>置顶</Checkbox>
          </Form.Item>
          <Form.Item name="content" label="内容" rules={[{ required: true, message: '请输入内容' }]}>
            <Input.TextArea rows={6} maxLength={4000} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
