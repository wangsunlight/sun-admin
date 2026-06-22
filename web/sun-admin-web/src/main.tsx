import React from 'react';
import ReactDOM from 'react-dom/client';
import { ConfigProvider } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import App from './app/App';
import './app/styles.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ConfigProvider
      locale={zhCN}
      theme={{
        token: {
          colorPrimary: '#0f766e',
          colorSuccess: '#16a34a',
          colorWarning: '#d97706',
          colorError: '#dc2626',
          colorText: '#152033',
          colorTextSecondary: '#667085',
          colorBorder: '#d8e2ec',
          colorBgLayout: '#eef3f8',
          borderRadius: 6,
          fontSize: 14,
          fontFamily:
            'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif',
        },
        components: {
          Button: {
            controlHeight: 36,
            primaryShadow: 'none',
          },
          Drawer: {
            paddingLG: 24,
          },
          Form: {
            itemMarginBottom: 18,
          },
          Layout: {
            siderBg: '#0b1220',
            headerBg: '#ffffff',
          },
          Menu: {
            darkItemBg: '#0b1220',
            darkSubMenuItemBg: '#0b1220',
            darkItemSelectedBg: '#0f766e',
            itemBorderRadius: 6,
          },
          Table: {
            headerBg: '#f8fafc',
            headerColor: '#475467',
            rowHoverBg: '#f8fbff',
          },
        },
      }}
    >
      <App />
    </ConfigProvider>
  </React.StrictMode>,
);
