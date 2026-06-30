import { PlusOutlined } from '@ant-design/icons';
import { App as AntApp, Form, Input, Modal, Select, Space, Tag } from 'antd';
import { useRef, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { codeGenerationService } from '../../services/platform';
import type {
  CodeGenerationTemplateCreateRequest,
  CodeGenerationTemplateItem,
  CodeGenerationTemplateUpdateRequest,
} from '../../types/platform';
import { formatDateTime, PlatformListPage, type PlatformListHelpers, statusTag } from './PlatformListPage';

type TemplateFormValues = CodeGenerationTemplateCreateRequest &
  Partial<Pick<CodeGenerationTemplateUpdateRequest, 'status'>>;

export default function CodeGenerationPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<TemplateFormValues>();
  const [editing, setEditing] = useState<CodeGenerationTemplateItem | null>(null);
  const [open, setOpen] = useState(false);
  const reloadRef = useRef<PlatformListHelpers['reload']>(async () => {});

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({ targetKind: 'backend', status: 'Enabled' });
    setOpen(true);
  };

  const openEdit = (record: CodeGenerationTemplateItem) => {
    setEditing(record);
    form.resetFields();
    form.setFieldsValue(record);
    setOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    try {
      if (editing) {
        await codeGenerationService.update(editing.id, {
          name: values.name,
          targetKind: values.targetKind,
          content: values.content,
          status: values.status ?? editing.status,
        });
      } else {
        await codeGenerationService.create({
          name: values.name,
          templateKey: values.templateKey,
          targetKind: values.targetKind,
          content: values.content,
        });
      }
      message.success('保存成功');
      setOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const remove = (record: CodeGenerationTemplateItem, helpers: PlatformListHelpers) => {
    Modal.confirm({
      title: '删除模板',
      content: `确认删除模板 ${record.name}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await codeGenerationService.remove(record.id);
        message.success('删除成功');
        await helpers.reload();
      },
    });
  };

  return (
    <>
      <PlatformListPage<CodeGenerationTemplateItem>
        title="代码生成"
        kicker="开发工具"
        description="管理代码生成模板，为后续实体、DTO、接口和前端页面生成预留入口。"
        load={codeGenerationService.templates}
        toolbarExtra={(helpers) => {
          reloadRef.current = helpers.reload;
          return (
            <PermissionButton type="primary" icon={<PlusOutlined />} permission="code-generation:create" onClick={openCreate}>
              新建模板
            </PermissionButton>
          );
        }}
        actions={(record, helpers) => (
          <Space>
            <PermissionButton type="link" permission="code-generation:update" onClick={() => openEdit(record)}>
              编辑
            </PermissionButton>
            <PermissionButton type="link" danger disabled={record.isBuiltIn} permission="code-generation:delete" onClick={() => remove(record, helpers)}>
              删除
            </PermissionButton>
          </Space>
        )}
        columns={[
          { title: '模板名', dataIndex: 'name', width: 220 },
          { title: '模板 Key', dataIndex: 'templateKey', width: 180 },
          { title: '目标', dataIndex: 'targetKind', width: 120 },
          { title: '状态', dataIndex: 'status', width: 100, render: statusTag },
          { title: '内置', dataIndex: 'isBuiltIn', width: 90, render: (value) => (value ? <Tag color="blue">内置</Tag> : '-') },
          { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
          { title: '内容', dataIndex: 'content', ellipsis: true },
        ]}
      />
      <Modal
        title={editing ? '编辑模板' : '新建模板'}
        width={760}
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => void submit()}
        okText="保存"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item name="name" label="模板名" rules={[{ required: true, message: '请输入模板名' }]}>
            <Input maxLength={128} />
          </Form.Item>
          {!editing && (
            <Form.Item name="templateKey" label="模板 Key" rules={[{ required: true, message: '请输入模板 Key' }]}>
              <Input maxLength={128} placeholder="backend.dto" />
            </Form.Item>
          )}
          <Form.Item name="targetKind" label="目标类型" rules={[{ required: true, message: '请选择目标类型' }]}>
            <Select
              options={[
                { label: '后端', value: 'backend' },
                { label: '前端', value: 'frontend' },
                { label: '通用', value: 'common' },
              ]}
            />
          </Form.Item>
          {editing && (
            <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
              <Select
                disabled={editing.isBuiltIn}
                options={[
                  { label: '启用', value: 'Enabled' },
                  { label: '禁用', value: 'Disabled' },
                ]}
              />
            </Form.Item>
          )}
          <Form.Item name="content" label="模板内容" rules={[{ required: true, message: '请输入模板内容' }]}>
            <Input.TextArea rows={12} maxLength={20000} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
