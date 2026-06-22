import {
  ApiOutlined,
  LockOutlined,
  SafetyCertificateOutlined,
  UserOutlined,
} from '@ant-design/icons';
import { Button, Form, Input, Typography, App as AntApp } from 'antd';
import { useEffect } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { defaultAuthedPath } from '../../routes/routeConfig';
import { useAuth } from '../../stores/authStore';
import type { LoginRequest } from '../../types/auth';

export default function LoginPage() {
  const navigate = useNavigate();
  const { message } = AntApp.useApp();
  const { login, isAuthenticated, loading } = useAuth();
  const [form] = Form.useForm<LoginRequest>();

  useEffect(() => {
    if (isAuthenticated) {
      navigate(defaultAuthedPath, { replace: true });
    }
  }, [isAuthenticated, navigate]);

  if (!loading && isAuthenticated) {
    return <Navigate to={defaultAuthedPath} replace />;
  }

  const handleFinish = async (values: LoginRequest) => {
    try {
      await login(values);
      message.success('登录成功');
      navigate(defaultAuthedPath, { replace: true });
    } catch (error) {
      message.error(error instanceof Error ? error.message : '登录失败');
    }
  };

  return (
    <div className="login-page">
      <section className="login-shell">
        <div className="login-copy">
          <div className="brand-mark">SA</div>
          <Typography.Title level={1} className="login-title">
            sun-admin
          </Typography.Title>
          <p className="login-subtitle">轻量级后台管理系统</p>
          <div className="login-feature-list">
            <span>
              <SafetyCertificateOutlined />
              RBAC 权限闭环
            </span>
            <span>
              <UserOutlined />
              用户角色管理
            </span>
            <span>
              <ApiOutlined />
              MySQL + .NET API
            </span>
          </div>
        </div>
        <div className="login-panel">
          <div className="login-panel-head">
            <Typography.Title level={2}>登录工作台</Typography.Title>
            <p>使用管理员账号进入系统。</p>
          </div>
          <Form form={form} layout="vertical" onFinish={handleFinish} requiredMark={false}>
            <Form.Item
              label="账号"
              name="account"
              rules={[{ required: true, message: '请输入用户名或邮箱' }]}
            >
              <Input
                autoComplete="username"
                prefix={<UserOutlined />}
                placeholder="用户名或邮箱"
                size="large"
              />
            </Form.Item>
            <Form.Item
              label="密码"
              name="password"
              rules={[{ required: true, message: '请输入密码' }]}
            >
              <Input.Password
                autoComplete="current-password"
                prefix={<LockOutlined />}
                placeholder="密码"
                size="large"
              />
            </Form.Item>
            <Button type="primary" htmlType="submit" size="large" block>
              登录
            </Button>
          </Form>
        </div>
      </section>
    </div>
  );
}
