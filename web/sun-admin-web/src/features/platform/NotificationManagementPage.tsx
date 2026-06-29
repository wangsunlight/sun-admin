import { Tag } from 'antd';
import { notificationService } from '../../services/platform';
import type { NotificationItem } from '../../types/platform';
import { formatDateTime, PlatformListPage, statusTag } from './PlatformListPage';

const levelColors: Record<string, string> = {
  Info: 'blue',
  Success: 'green',
  Warning: 'orange',
  Error: 'red',
};

export default function NotificationManagementPage() {
  return (
    <PlatformListPage<NotificationItem>
      title="通知公告"
      kicker="消息中心"
      description="管理系统通知、公告和面向后台用户的运营提示。"
      load={notificationService.list}
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
  );
}
