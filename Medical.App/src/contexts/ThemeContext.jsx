import { createContext, useContext, useState, useEffect } from 'react';

const ThemeContext = createContext();

// 字体倍数配置（可调整）
export const FONT_SCALE_CONFIG = {
  standard: 1,      // 标准模式：1倍
  elder: 1.2        // 长辈模式：1.2倍（可调整）
};

export function ThemeProvider({ children }) {
  const [mode, setMode] = useState(() => {
    // 从localStorage读取保存的模式
    const savedMode = localStorage.getItem('app-mode');
    return savedMode || 'standard';
  });

  // 初始化时立即设置模式（不等待useEffect）
  useEffect(() => {
    const initializeMode = () => {
      try {
        const savedMode = localStorage.getItem('app-mode') || 'standard';
        const scale = FONT_SCALE_CONFIG[savedMode] || FONT_SCALE_CONFIG.standard;
        
        // 设置CSS变量
        document.documentElement.style.setProperty('--font-scale', scale);
        
        // 直接设置zoom属性
        if (savedMode === 'elder') {
          document.documentElement.style.zoom = scale.toString();
        } else {
          document.documentElement.style.zoom = '1';
        }
        
        // 添加模式类名
        const html = document.documentElement;
        const body = document.body;
        
        if (html && body) {
          html.className = html.className.replace(/mode-\w+/g, '').trim();
          body.className = body.className.replace(/mode-\w+/g, '').trim();
          html.classList.add(`mode-${savedMode}`);
          body.classList.add(`mode-${savedMode}`);
        }
      } catch (error) {
        console.error('初始化主题模式失败:', error);
      }
    };
    
    initializeMode();
  }, []); // 只在组件挂载时执行一次

  // 模式改变时更新
  useEffect(() => {
    try {
      // 保存模式到localStorage
      localStorage.setItem('app-mode', mode);
      
      // 设置CSS变量
      const scale = FONT_SCALE_CONFIG[mode] || FONT_SCALE_CONFIG.standard;
      document.documentElement.style.setProperty('--font-scale', scale);
      
      // 直接设置zoom属性（更可靠）
      if (mode === 'elder') {
        document.documentElement.style.zoom = scale.toString();
      } else {
        document.documentElement.style.zoom = '1';
      }
      
      // 添加模式类名到html和body
      const html = document.documentElement;
      const body = document.body;
      
      if (html && body) {
        // 清除旧的模式类名
        html.className = html.className.replace(/mode-\w+/g, '').trim();
        body.className = body.className.replace(/mode-\w+/g, '').trim();
        
        // 添加新的模式类名
        html.classList.add(`mode-${mode}`);
        body.classList.add(`mode-${mode}`);
      }
    } catch (error) {
      console.error('设置主题模式失败:', error);
    }
  }, [mode]);

  return (
    <ThemeContext.Provider value={{ mode, setMode }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}

