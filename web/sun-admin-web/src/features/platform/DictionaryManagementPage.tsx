import { Tag } from 'antd';
import { dictionaryService } from '../../services/platform';
import type { DictionaryItemGroup } from '../../types/platform';
import { formatDateTime, PlatformListPage, statusTag } from './PlatformListPage';

export default function DictionaryManagementPage() {
  return (
    <PlatformListPage<DictionaryItemGroup>
      title="数据字典"
      kicker="配置中心"
      description="管理通用枚举、下拉选项和基础字典数据。"
      load={dictionaryService.list}
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
  );
}
