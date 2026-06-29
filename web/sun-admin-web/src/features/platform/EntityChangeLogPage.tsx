import { entityChangeLogService } from '../../services/platform';
import type { EntityChangeLogItem } from '../../types/platform';
import { formatDateTime, PlatformListPage } from './PlatformListPage';

export default function EntityChangeLogPage() {
  return (
    <PlatformListPage<EntityChangeLogItem>
      title="变更审计"
      kicker="审计日志"
      description="查看实体级新增、修改、删除和授权变更记录。"
      load={entityChangeLogService.list}
      columns={[
        { title: '实体', dataIndex: 'entityName', width: 160 },
        { title: '实体 ID', dataIndex: 'entityId', width: 120 },
        { title: '变更', dataIndex: 'changeType', width: 120 },
        { title: '字段', dataIndex: 'changedFields', width: 180, render: (value) => value || '-' },
        { title: '操作人', dataIndex: 'changedByName', width: 140, render: (value) => value || '-' },
        { title: 'IP', dataIndex: 'ipAddress', width: 150, render: (value) => value || '-' },
        { title: '时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
      ]}
    />
  );
}
