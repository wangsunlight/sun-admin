import { Alert, Button } from 'antd';
import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from 'react';

interface AppErrorBoundaryState {
  error: Error | null;
}

export default class AppErrorBoundary extends Component<
  PropsWithChildren,
  AppErrorBoundaryState
> {
  state: AppErrorBoundaryState = {
    error: null,
  };

  static getDerivedStateFromError(error: Error): AppErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Route render failed', error, info);
  }

  handleRetry = () => {
    this.setState({ error: null });
  };

  render(): ReactNode {
    if (this.state.error) {
      return (
        <div className="route-error">
          <Alert
            type="error"
            showIcon
            message="页面加载失败"
            description={this.state.error.message || '请刷新页面或稍后重试'}
            action={
              <Button size="small" onClick={this.handleRetry}>
                重试
              </Button>
            }
          />
        </div>
      );
    }

    return this.props.children;
  }
}
