import { PlusOutlined } from '@ant-design/icons';
import { App as AntApp, Form, Input, Modal } from 'antd';
import { Tag } from 'antd';
import { useRef, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { exportTaskService } from '../../services/platform';
import type { ExportTaskCreateRequest, ExportTaskItem } from '../../types/platform';
import { formatDateTime, PlatformListPage, type PlatformListHelpers } from './PlatformListPage';

const statusColors: Record<string, string> = {
  Pending: 'default',
  Running: 'blue',
  Succeeded: 'green',
  Failed: 'red',
};

export default function ExportTaskPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<ExportTaskCreateRequest>();
  const [open, setOpen] = useState(false);
  const reloadRef = useRef<PlatformListHelpers['reload']>(async () => {});

  const openCreate = () => {
    form.resetFields();
    form.setFieldsValue({ parametersJson: '{}' });
    setOpen(true);
  };

  const submit = async () => {
    const values = await form.validateFields();
    try {
      await exportTaskService.create({
        ...values,
        parametersJson: values.parametersJson?.trim() || null,
      });
      message.success('导出任务已创建');
      setOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '创建失败');
    }
  };

  return (
    <>
      <PlatformListPage<ExportTaskItem>
        title="导出中心"
        kicker="异步任务"
        description="查看导出任务状态，为大数据量异步导出和文件下载预留统一队列。"
        load={exportTaskService.list}
        toolbarExtra={(helpers) => {
          reloadRef.current = helpers.reload;
          return (
            <PermissionButton type="primary" icon={<PlusOutlined />} permission="export:create" onClick={openCreate}>
              创建导出
            </PermissionButton>
          );
        }}
        columns={[
          { title: '任务名', dataIndex: 'taskName', width: 220 },
          { title: '类型', dataIndex: 'exportType', width: 140 },
          { title: '状态', dataIndex: 'status', width: 110, render: (value) => <Tag color={statusColors[value] ?? 'default'}>{value}</Tag> },
          { title: '发起人', dataIndex: 'createdByUserName', width: 150 },
          { title: '文件', dataIndex: 'filePath', ellipsis: true, render: (value) => value || '-' },
          { title: '错误', dataIndex: 'errorMessage', ellipsis: true, render: (value) => value || '-' },
          { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
        ]}
      />
      <Modal
        title="创建导出"
        open={open}
        onCancel={() => setOpen(false)}
        onOk={() => void submit()}
        okText="创建"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item name="taskName" label="任务名" rules={[{ required: true, message: '请输入任务名' }]}>
            <Input maxLength={128} />
          </Form.Item>
          <Form.Item name="exportType" label="导出类型" rules={[{ required: true, message: '请输入导出类型' }]}>
            <Input maxLength={64} placeholder="users" />
          </Form.Item>
          <Form.Item name="parametersJson" label="参数 JSON">
            <Input.TextArea rows={6} maxLength={8000} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
