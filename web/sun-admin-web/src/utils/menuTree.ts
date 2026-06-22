import {
  ApiOutlined,
  FileTextOutlined,
  FolderOutlined,
} from '@ant-design/icons';
import { Tag } from 'antd';
import type { DataNode } from 'antd/es/tree';
import { createElement } from 'react';
import type { MenuItem } from '../types/menu';

const menuTypeMeta = {
  Directory: {
    icon: FolderOutlined,
    label: '目录',
    color: 'blue',
  },
  Page: {
    icon: FileTextOutlined,
    label: '页面',
    color: 'green',
  },
  Button: {
    icon: ApiOutlined,
    label: '按钮/API',
    color: 'purple',
  },
} as const;

export function flattenMenus(items: MenuItem[]) {
  const result: MenuItem[] = [];

  const walk = (nodes: MenuItem[]) => {
    nodes.forEach((node) => {
      result.push(node);
      if (node.children?.length) {
        walk(node.children);
      }
    });
  };

  walk(items);
  return result;
}

export function toTreeData(items: MenuItem[]): DataNode[] {
  return items.map((item) => {
    const meta = menuTypeMeta[item.type];

    return {
      key: item.id,
      title: createElement(
        'span',
        { className: 'permission-tree-node' },
        createElement(meta.icon, { className: 'permission-tree-node-icon' }),
        createElement(Tag, { color: meta.color, className: 'permission-tree-node-tag' }, meta.label),
        createElement('span', { className: 'permission-tree-node-name' }, item.name),
        item.permissionCode
          ? createElement('code', { className: 'permission-tree-node-code' }, item.permissionCode)
          : null,
      ),
      children: item.children?.length ? toTreeData(item.children) : undefined,
    };
  });
}

export function toParentOptions(items: MenuItem[]) {
  return flattenMenus(items)
    .filter((item) => item.type !== 'Button')
    .map((item) => ({
      label: item.name,
      value: item.id,
    }));
}
