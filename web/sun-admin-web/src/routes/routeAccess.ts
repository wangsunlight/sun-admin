import type { MenuItem } from '../types/menu';

export function collectVisiblePaths(menus: MenuItem[]) {
  const paths = new Set<string>();

  const walk = (items: MenuItem[]) => {
    items.forEach((item) => {
      if (item.status === 'Enabled' && item.type === 'Page' && item.routePath) {
        paths.add(item.routePath);
      }

      if (item.children?.length) {
        walk(item.children);
      }
    });
  };

  walk(menus);
  return paths;
}

export function canAccessPath(
  path: string,
  menus: MenuItem[],
  knownPaths: string[],
) {
  const visiblePaths = collectVisiblePaths(menus);
  const hasMatchedRoute = knownPaths.some((knownPath) => visiblePaths.has(knownPath));
  const allowAll = visiblePaths.size === 0 || !hasMatchedRoute;

  return allowAll || visiblePaths.has(path);
}
