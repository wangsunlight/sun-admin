import { PlusOutlined } from '@ant-design/icons';
import { App as AntApp, Form, Input, InputNumber, Modal, Select, Space, Table, Tag } from 'antd';
import { useRef, useState } from 'react';
import PermissionButton from '../../components/PermissionButton';
import { dictionaryService } from '../../services/platform';
import type {
  DictionaryCreateRequest,
  DictionaryItem,
  DictionaryItemGroup,
  DictionaryItemUpsertRequest,
  DictionaryUpdateRequest,
} from '../../types/platform';
import { formatDateTime, PlatformListPage, type PlatformListHelpers, statusTag } from './PlatformListPage';

type DictionaryFormValues = DictionaryCreateRequest & Partial<DictionaryUpdateRequest>;

export default function DictionaryManagementPage() {
  const { message } = AntApp.useApp();
  const [dictionaryForm] = Form.useForm<DictionaryFormValues>();
  const [itemForm] = Form.useForm<DictionaryItemUpsertRequest>();
  const [editing, setEditing] = useState<DictionaryItemGroup | null>(null);
  const [editingItem, setEditingItem] = useState<DictionaryItem | null>(null);
  const [itemDictionary, setItemDictionary] = useState<DictionaryItemGroup | null>(null);
  const [dictionaryOpen, setDictionaryOpen] = useState(false);
  const [itemOpen, setItemOpen] = useState(false);
  const reloadRef = useRef<PlatformListHelpers['reload']>(async () => {});

  const openCreate = () => {
    setEditing(null);
    dictionaryForm.resetFields();
    dictionaryForm.setFieldsValue({ status: 'Enabled' });
    setDictionaryOpen(true);
  };

  const openEdit = (record: DictionaryItemGroup) => {
    setEditing(record);
    dictionaryForm.resetFields();
    dictionaryForm.setFieldsValue(record);
    setDictionaryOpen(true);
  };

  const submitDictionary = async () => {
    const values = await dictionaryForm.validateFields();
    try {
      if (editing) {
        await dictionaryService.update(editing.id, {
          name: values.name,
          description: values.description || null,
          status: values.status ?? editing.status,
        });
      } else {
        await dictionaryService.create({
          code: values.code,
          name: values.name,
          description: values.description || null,
        });
      }
      message.success('保存成功');
      setDictionaryOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeDictionary = (record: DictionaryItemGroup, helpers: PlatformListHelpers) => {
    Modal.confirm({
      title: '删除字典',
      content: `确认删除字典 ${record.name}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await dictionaryService.remove(record.id);
        message.success('删除成功');
        await helpers.reload();
      },
    });
  };

  const openCreateItem = (record: DictionaryItemGroup) => {
    setItemDictionary(record);
    setEditingItem(null);
    itemForm.resetFields();
    itemForm.setFieldsValue({ sortOrder: 0, status: 'Enabled' });
    setItemOpen(true);
  };

  const openEditItem = (record: DictionaryItemGroup, item: DictionaryItem) => {
    setItemDictionary(record);
    setEditingItem(item);
    itemForm.resetFields();
    itemForm.setFieldsValue(item);
    setItemOpen(true);
  };

  const submitItem = async () => {
    if (!itemDictionary) {
      return;
    }

    const values = await itemForm.validateFields();
    try {
      if (editingItem) {
        await dictionaryService.updateItem(itemDictionary.id, editingItem.id, values);
      } else {
        await dictionaryService.createItem(itemDictionary.id, values);
      }
      message.success('保存成功');
      setItemOpen(false);
      await reloadRef.current();
    } catch (error) {
      message.error(error instanceof Error ? error.message : '保存失败');
    }
  };

  const removeItem = (record: DictionaryItemGroup, item: DictionaryItem) => {
    Modal.confirm({
      title: '删除字典项',
      content: `确认删除字典项 ${item.label}？`,
      okType: 'danger',
      okText: '删除',
      cancelText: '取消',
      onOk: async () => {
        await dictionaryService.removeItem(record.id, item.id);
        message.success('删除成功');
        await reloadRef.current();
      },
    });
  };

  return (
    <>
      <PlatformListPage<DictionaryItemGroup>
        title="数据字典"
        kicker="配置中心"
        description="管理通用枚举、下拉选项和基础字典数据。"
        load={dictionaryService.list}
        toolbarExtra={(helpers) => {
          reloadRef.current = helpers.reload;
          return (
            <PermissionButton type="primary" icon={<PlusOutlined />} permission="dictionary:create" onClick={openCreate}>
              新建字典
            </PermissionButton>
          );
        }}
        actions={(record, helpers) => (
          <Space>
            <PermissionButton type="link" permission="dictionary:update" onClick={() => openCreateItem(record)}>
              新增项
            </PermissionButton>
            <PermissionButton type="link" permission="dictionary:update" onClick={() => openEdit(record)}>
              编辑
            </PermissionButton>
            <PermissionButton type="link" danger disabled={record.isBuiltIn} permission="dictionary:delete" onClick={() => removeDictionary(record, helpers)}>
              删除
            </PermissionButton>
          </Space>
        )}
        expandable={{
          expandedRowRender: (record) => (
            <Table<DictionaryItem>
              rowKey="id"
              size="small"
              pagination={false}
              dataSource={record.items}
              columns={[
                { title: '标签', dataIndex: 'label', width: 180 },
                { title: '值', dataIndex: 'value', width: 180 },
                { title: '排序', dataIndex: 'sortOrder', width: 90 },
                { title: '状态', dataIndex: 'status', width: 100, render: statusTag },
                { title: '内置', dataIndex: 'isBuiltIn', width: 90, render: (value) => (value ? <Tag color="blue">内置</Tag> : '-') },
                {
                  title: '操作',
                  width: 180,
                  render: (_, item) => (
                    <Space>
                      <PermissionButton type="link" permission="dictionary:update" onClick={() => openEditItem(record, item)}>
                        编辑
                      </PermissionButton>
                      <PermissionButton type="link" danger disabled={item.isBuiltIn} permission="dictionary:delete" onClick={() => removeItem(record, item)}>
                        删除
                      </PermissionButton>
                    </Space>
                  ),
                },
              ]}
            />
          ),
        }}
        columns={[
          { title: '编码', dataIndex: 'code', width: 180 },
          { title: '名称', dataIndex: 'name', width: 180 },
          { title: '状态', dataIndex: 'status', width: 100, render: statusTag },
          { title: '内置', dataIndex: 'isBuiltIn', width: 90, render: (value) => (value ? <Tag color="blue">内置</Tag> : '-') },
          { title: '项数量', dataIndex: 'items', width: 100, render: (items: unknown[]) => items?.length ?? 0 },
          { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
          { title: '说明', dataIndex: 'description', ellipsis: true, render: (value) => value || '-' },
        ]}
      />
      <Modal
        title={editing ? '编辑字典' : '新建字典'}
        open={dictionaryOpen}
        onCancel={() => setDictionaryOpen(false)}
        onOk={() => void submitDictionary()}
        okText="保存"
        cancelText="取消"
      >
        <Form form={dictionaryForm} layout="vertical">
          {!editing && (
            <Form.Item name="code" label="编码" rules={[{ required: true, message: '请输入编码' }]}>
              <Input maxLength={64} />
            </Form.Item>
          )}
          <Form.Item name="name" label="名称" rules={[{ required: true, message: '请输入名称' }]}>
            <Input maxLength={64} />
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
          <Form.Item name="description" label="说明">
            <Input.TextArea rows={3} maxLength={256} />
          </Form.Item>
        </Form>
      </Modal>
      <Modal
        title={editingItem ? '编辑字典项' : '新增字典项'}
        open={itemOpen}
        onCancel={() => setItemOpen(false)}
        onOk={() => void submitItem()}
        okText="保存"
        cancelText="取消"
      >
        <Form form={itemForm} layout="vertical">
          <Form.Item name="label" label="标签" rules={[{ required: true, message: '请输入标签' }]}>
            <Input maxLength={128} />
          </Form.Item>
          <Form.Item name="value" label="值" rules={[{ required: true, message: '请输入值' }]}>
            <Input maxLength={128} />
          </Form.Item>
          <Form.Item name="sortOrder" label="排序" rules={[{ required: true, message: '请输入排序' }]}>
            <InputNumber min={0} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item name="status" label="状态" rules={[{ required: true, message: '请选择状态' }]}>
            <Select
              options={[
                { label: '启用', value: 'Enabled' },
                { label: '禁用', value: 'Disabled' },
              ]}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
