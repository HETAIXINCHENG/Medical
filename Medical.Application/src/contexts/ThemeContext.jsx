import { createContext, useContext, useState, useEffect } from 'react';

const ThemeContext = createContext();

const STORAGE_KEY = 'medical_app_theme_color';
const DEFAULT_COLOR = '#0d9488'; // 默认主题色（teal-600）

export function ThemeProvider({ children }) {
  const [primaryColor, setPrimaryColor] = useState(() => {
    // 从localStorage加载保存的主题颜色
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved || DEFAULT_COLOR;
  });

  useEffect(() => {
    // 保存主题颜色到localStorage
    localStorage.setItem(STORAGE_KEY, primaryColor);
    
    // 更新CSS变量（如果需要）
    document.documentElement.style.setProperty('--primary-color', primaryColor);
  }, [primaryColor]);

  const changeColor = (color) => {
    setPrimaryColor(color);
  };

  return (
    <ThemeContext.Provider value={{ primaryColor, changeColor }}>
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

