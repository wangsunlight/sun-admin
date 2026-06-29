import { Tag } from 'antd';
import { exportTaskService } from '../../services/platform';
import type { ExportTaskItem } from '../../types/platform';
import { formatDateTime, PlatformListPage } from './PlatformListPage';

const statusColors: Record<string, string> = {
  Pending: 'default',
  Running: 'blue',
  Succeeded: 'green',
  Failed: 'red',
};

export default function ExportTaskPage() {
  return (
    <PlatformListPage<ExportTaskItem>
      title="导出中心"
      kicker="异步任务"
      description="查看导出任务状态，为大数据量异步导出和文件下载预留统一队列。"
      load={exportTaskService.list}
      columns={[
        { title: '任务名', dataIndex: 'taskName', width: 220 },
        { title: '类型', dataIndex: 'exportType', width: 140 },
        { title: '状态', dataIndex: 'status', width: 110, render: (value) => <Tag color={statusColors[value] ?? 'default'}>{value}</Tag> },
        { title: '发起人', dataIndex: 'createdByUserName', width: 150 },
        { title: '文件', dataIndex: 'filePath', ellipsis: true, render: (value) => value || '-' },
        { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
      ]}
    />
  );
}
