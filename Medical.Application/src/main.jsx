import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import { LanguageProvider, useLanguage } from './contexts/LanguageContext.jsx';
import { ThemeProvider, useTheme } from './contexts/ThemeContext.jsx';
import './index.css';
import App from './App.jsx';

// 内部组件，用于访问主题上下文
function ThemedApp() {
  const { primaryColor } = useTheme();
  
  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: primaryColor,
          borderRadius: 6
        }
      }}
    >
      <App />
    </ConfigProvider>
  );
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    {/* BrowserRouter 负责前端路由管理 */}
    <BrowserRouter
      future={{
        v7_startTransition: true,
        v7_relativeSplatPath: true
      }}
    >
      {/* LanguageProvider 提供语言上下文 */}
      <LanguageProvider>
        {/* ThemeProvider 提供主题上下文 */}
        <ThemeProvider>
          {/* ThemedApp 使用主题颜色配置 ConfigProvider */}
          <ThemedApp />
        </ThemeProvider>
      </LanguageProvider>
    </BrowserRouter>
  </StrictMode>
);
