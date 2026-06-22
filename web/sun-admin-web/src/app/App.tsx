import { App as AntApp } from 'antd';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '../stores/authStore';
import AppRoutes from '../routes/AppRoutes';

export default function App() {
  return (
    <AntApp>
      <BrowserRouter>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </BrowserRouter>
    </AntApp>
  );
}
