import { PlusOutlined } from '@ant-design/icons';
import { App as AntApp, Form, Input, InputNumber, Modal, Space } from 'antd';
import { useRef, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { fileResourceService } from '../../services/platform';
import type { FileResourceCreateRequest, FileResourceItem } from '../../types/platform';
import { formatDateTime, PlatformListPage, type PlatformListHelpers } from './PlatformListPage';

function formatSize(value: number) {
  if (value >= 1024 * 1024) {
    return `${(value / 1024 / 1024).toFixed(2)} MB`;
  }
  if (value >= 1024) {
    return `${(value / 1024).toFixed(2)} KB`;
  }
  return `${value} B`;
}

export default function FileResourcePage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<FileResourceCreateRequest>();
  const [open, setOpen] = useState(false);
  const reloadRef = useRef<PlatformListHelpers['reload']>(async () => {});

  const openCreate = () => {
    form.resetFields();
    form.setFieldsValue({ storageProvider: 'local' });
    setOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    try {
      await fileResourceService.create(values);
      message.success('登记成功');
      setOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '登记失败');
    }
  };

  const remove = (record: FileResourceItem, helpers: PlatformListHelpers) => {
    Modal.confirm({
      title: '删除文件记录',
      content: `确认删除文件记录 ${record.originalFileName}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await fileResourceService.remove(record.id);
        message.success('删除成功');
        await helpers.reload();
      },
    });
  };

  return (
    <>
      <PlatformListPage<FileResourceItem>
        title="文件资源"
        kicker="文件存储"
        description="登记文件元数据，为后续上传、下载和多存储适配预留统一入口。"
        load={fileResourceService.list}
        toolbarExtra={(helpers) => {
          reloadRef.current = helpers.reload;
          return (
            <PermissionButton type="primary" icon={<PlusOutlined />} permission="file:create" onClick={openCreate}>
              登记文件
            </PermissionButton>
          );
        }}
        actions={(record, helpers) => (
          <Space>
            <PermissionButton type="link" danger permission="file:delete" onClick={() => remove(record, helpers)}>
              删除
            </PermissionButton>
          </Space>
        )}
        columns={[
          { title: '原文件名', dataIndex: 'originalFileName', width: 220 },
          { title: '类型', dataIndex: 'contentType', width: 160 },
          { title: '大小', dataIndex: 'sizeBytes', width: 120, render: formatSize },
          { title: '存储', dataIndex: 'storageProvider', width: 120 },
          { title: '路径', dataIndex: 'storagePath', ellipsis: true },
          { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
        ]}
      />
      <Modal
        title="登记文件"
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => void submit()}
        okText="保存"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item name="originalFileName" label="原文件名" rules={[{ required: true, message: '请输入原文件名' }]}>
            <Input maxLength={256} />
          </Form.Item>
          <Form.Item name="contentType" label="内容类型" rules={[{ required: true, message: '请输入内容类型' }]}>
            <Input maxLength={128} placeholder="application/pdf" />
          </Form.Item>
          <Form.Item name="sizeBytes" label="文件大小" rules={[{ required: true, message: '请输入文件大小' }]}>
            <InputNumber min={1} max={500 * 1024 * 1024} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="storageProvider" label="存储类型">
            <Input maxLength={64} placeholder="local" />
          </Form.Item>
          <Form.Item name="storagePath" label="存储路径" rules={[{ required: true, message: '请输入存储路径' }]}>
            <Input maxLength={1024} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
