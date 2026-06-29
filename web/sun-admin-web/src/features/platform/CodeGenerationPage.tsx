import { codeGenerationService } from '../../services/platform';
import type { CodeGenerationTemplateItem } from '../../types/platform';
import { formatDateTime, PlatformListPage, statusTag } from './PlatformListPage';

export default function CodeGenerationPage() {
  return (
    <PlatformListPage<CodeGenerationTemplateItem>
      title="代码生成"
      kicker="开发工具"
      description="管理代码生成模板，为后续实体、DTO、接口和前端页面生成预留入口。"
      load={codeGenerationService.templates}
      columns={[
        { title: '模板名', dataIndex: 'name', width: 220 },
        { title: '模板 Key', dataIndex: 'templateKey', width: 180 },
        { title: '目标', dataIndex: 'targetKind', width: 120 },
        { title: '状态', dataIndex: 'status', width: 100, render: statusTag },
        { title: '创建时间', dataIndex: 'createdAt', width: 180, render: formatDateTime },
        { title: '内容', dataIndex: 'content', ellipsis: true },
      ]}
    />
  );
}
