import { Button, type ButtonProps } from 'antd';
import { useAuth } from '../stores/authStore';

interface PermissionButtonProps extends ButtonProps {
  permission: string;
}

export default function PermissionButton({
  permission,
  children,
  ...buttonProps
}: PermissionButtonProps) {
  const { hasPermission } = useAuth();

  if (!hasPermission(permission)) {
    return null;
  }

  return <Button {...buttonProps}>{children}</Button>;
}
