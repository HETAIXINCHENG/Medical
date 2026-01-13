import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import ModeSwitchModal from '../components/ModeSwitchModal.jsx';
import { useTheme } from '../contexts/ThemeContext.jsx';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Settings() {
  usePageStyles('settings.css');
  const navigate = useNavigate();
  const { mode } = useTheme();
  const { t } = useLanguage();
  const [showModeModal, setShowModeModal] = useState(false);

  const handleLogout = () => {
    if (window.confirm(t('confirm-logout'))) {
      localStorage.removeItem('medical-jwt');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      navigate('/login');
    }
  };

  const handleSwitchAccount = () => {
    if (window.confirm(t('confirm-switch-account'))) {
      localStorage.removeItem('medical-jwt');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      navigate('/login');
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="settings-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">{t('settings')}</h1>
      </div>

      {/* 设置内容 */}
      <div className="settings-content">
        {/* 账号与安全 */}
        <div className="settings-section">
          <div className="settings-item" onClick={() => navigate('/account-security')}>
            <div className="settings-item-left">
              <div className="settings-icon account-icon">👤</div>
              <span className="settings-label">{t('account-security')}</span>
            </div>
            <span className="settings-arrow">›</span>
          </div>
          <div className="settings-item">
            <div className="settings-item-left">
              <div className="settings-icon payment-icon">¥</div>
              <span className="settings-label">{t('payment-settings')}</span>
            </div>
            <span className="settings-arrow">›</span>
          </div>
        </div>

        {/* 模式切换 */}
        <div className="settings-section">
          <div className="settings-item" onClick={() => setShowModeModal(true)}>
            <div className="settings-item-left">
              <div className="settings-icon mode-icon">💙</div>
              <span className="settings-label">{t('mode-switch')}</span>
            </div>
            <span className="settings-arrow">›</span>
          </div>
        </div>

        {/* 通知 */}
        <div className="settings-section">
          <div className="settings-item" onClick={() => navigate('/notification-settings')}>
            <div className="settings-item-left">
              <div className="settings-icon notification-icon">🔔</div>
              <span className="settings-label">{t('notification-settings')}</span>
            </div>
            <span className="settings-arrow">›</span>
          </div>
        </div>

        {/* 反馈与关于 */}
        <div className="settings-section">
          <div className="settings-item" onClick={() => navigate('/feedback')}>
            <div className="settings-item-left">
              <div className="settings-icon feedback-icon">✏️</div>
              <span className="settings-label">{t('feedback')}</span>
            </div>
            <span className="settings-arrow">›</span>
          </div>
          <div className="settings-item" onClick={() => navigate('/system-info')}>
            <div className="settings-item-left">
              <div className="settings-icon about-icon">ℹ️</div>
              <div className="settings-item-text">
                <span className="settings-label">{t('about')}</span>
                <span className="settings-subtitle">{t('version')} 10.8.6</span>
              </div>
            </div>
            <span className="settings-arrow">›</span>
          </div>
        </div>

        {/* 底部按钮 */}
        <div className="settings-buttons">
          <button className="settings-btn switch-account-btn" onClick={handleSwitchAccount}>
            {t('switch-account')}
          </button>
          <button className="settings-btn logout-btn" onClick={handleLogout}>
            {t('logout')}
          </button>
        </div>
      </div>

      {/* 模式切换模态框 */}
      <ModeSwitchModal 
        visible={showModeModal} 
        onClose={() => setShowModeModal(false)} 
      />
    </PageLayout>
  );
}

