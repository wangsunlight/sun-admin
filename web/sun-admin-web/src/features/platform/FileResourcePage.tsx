import { fileResourceService } from '../../services/platform';
import type { FileResourceItem } from '../../types/platform';
import { formatDateTime, PlatformListPage } from './PlatformListPage';

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
  return (
    <PlatformListPage<FileResourceItem>
      title="文件资源"
      kicker="文件存储"
      description="登记文件元数据，为后续上传、下载和多存储适配预留统一入口。"
      load={fileResourceService.list}
      columns={[
        { title: '原文件名', dataIndex: 'originalFileName', width: 220 },
        { title: '类型', dataIndex: 'contentType', width: 160 },
        { title: '大小', dataIndex: 'sizeBytes', width: 120, render: formatSize },
        { title: '存储', dataIndex: 'storageProvider', width: 120 },
        { title: '路径', dataIndex: 'storagePath', ellipsis: true },
        { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
      ]}
    />
  );
}
