import type { CurrentUser } from '../types/auth';

export function hasPermission(
  user: CurrentUser | null,
  permissionCode: string,
) {
  if (!user) {
    return false;
  }

  return (
    user.roles.includes('super_admin') ||
    user.permissions.includes(permissionCode)
  );
}
