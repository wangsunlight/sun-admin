import { ReloadOutlined, SettingOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Form, Input, Modal, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useEffect, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { settingService } from '../../services/settings';
import type { SettingItem } from '../../types/setting';

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

export default function SettingManagementPage() {
  const { message } = AntApp.useApp();
  const [form] = Form.useForm<{ value: string }>();
  const [settings, setSettings] = useState<SettingItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState<SettingItem | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const loadSettings = async () => {
    setLoading(true);
    try {
      setSettings(await settingService.list());
    } catch (error) {
      message.error(error instanceof Error ? error.message : '系统配置加载失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadSettings();
  }, []);

  const openEdit = (record: SettingItem) => {
    setEditing(record);
    form.setFieldsValue({ value: record.value });
  };

  const submitSetting = async () => {
    if (!editing) return;
    const values = await form.validateFields();
    setSubmitting(true);
    try {
      await settingService.update(editing.key, values.value);
      message.success('配置已保存');
      setEditing(null);
      await loadSettings();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '配置保存失败');
    } finally {
      setSubmitting(false);
    }
  };

  const columns: ColumnsType<SettingItem> = [
    { title: '名称', dataIndex: 'name', width: 180 },
    { title: '配置键', dataIndex: 'key', width: 260 },
    { title: '配置值', dataIndex: 'value', width: 180 },
    { title: '说明', dataIndex: 'description', render: (value) => value || '-' },
    { title: '更新时间', dataIndex: 'updatedAt', width: 180, render: formatDateTime },
    {
      title: '操作',
      width: 100,
      fixed: 'right',
      render: (_, record) => (
        <PermissionButton type="link" permission="setting:update" onClick={() => openEdit(record)}>
          编辑
        </PermissionButton>
      ),
    },
  ];

  return (
    <section className="page-surface">
      <div className="page-heading">
        <div>
          <div className="page-kicker">
            <SettingOutlined />
            系统参数
          </div>
          <h1>系统配置</h1>
          <p>维护后台运行所需的基础配置，当前提供密码策略和系统名称配置。</p>
        </div>
        <div className="page-summary">
          <span>
            <strong>{settings.length}</strong>
            配置项
          </span>
        </div>
      </div>
      <div className="page-toolbar">
        <div className="page-toolbar-search">
          <Button icon={<ReloadOutlined />} onClick={() => void loadSettings()}>
            刷新
          </Button>
        </div>
      </div>
      <Table
        rowKey="key"
        loading={loading}
        columns={columns}
        dataSource={settings}
        scroll={{ x: 1100 }}
        pagination={false}
      />
      <Modal
        title={`编辑配置${editing ? ` - ${editing.name}` : ''}`}
        open={Boolean(editing)}
        okText="保存"
        cancelText="取消"
        confirmLoading={submitting}
        onCancel={() => setEditing(null)}
        onOk={() => void submitSetting()}
      >
        <Form form={form} layout="vertical" requiredMark={false}>
          <Form.Item name="value" label="配置值" rules={[{ required: true, message: '请输入配置值' }]}>
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </section>
  );
}
