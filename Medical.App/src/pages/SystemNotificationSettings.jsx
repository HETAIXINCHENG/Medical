import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function SystemNotificationSettings() {
  usePageStyles('system-notification-settings.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [allNotificationsEnabled, setAllNotificationsEnabled] = useState(false);
  const [lockScreenEnabled, setLockScreenEnabled] = useState(false);

  // 模拟检查系统通知权限
  useEffect(() => {
    // 这里可以添加实际的权限检查逻辑
    // 目前模拟为未开启状态
    setAllNotificationsEnabled(false);
  }, []);

  const handleToggleAllNotifications = () => {
    const newValue = !allNotificationsEnabled;
    setAllNotificationsEnabled(newValue);
    if (newValue) {
      setLockScreenEnabled(true);
    }
  };

  const handleToggleLockScreen = () => {
    setLockScreenEnabled(!lockScreenEnabled);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="system-notification-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('notification-management')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="system-notification-content">
        {/* 应用信息 */}
        <div className="app-info-section">
          <div className="app-icon">
            <div className="icon-background">
              <span className="icon-thumb">👍</span>
            </div>
          </div>
          <div className="app-name">{t('app-name')}</div>
        </div>

        {/* 所有通知开关 */}
        <div className="notification-card">
          <div className="card-content">
            <span className="card-label">{t('all-notifications')}</span>
            <div className={`toggle-switch ${allNotificationsEnabled ? 'enabled' : ''}`} onClick={handleToggleAllNotifications}>
              <div className={`toggle-slider ${allNotificationsEnabled ? 'enabled' : ''}`}></div>
            </div>
          </div>
        </div>

        {/* 提示信息 */}
        {!allNotificationsEnabled && (
          <div className="hint-text">
            {t('not-allowed-notification')}
          </div>
        )}

        {/* 类别 */}
        {allNotificationsEnabled && (
          <>
            <div className="category-label">{t('categories')}</div>
            <div className="notification-card">
              <div className="card-content">
                <span className="card-label disabled">{t('no-notifications-published')}</span>
                <span className="item-arrow">›</span>
              </div>
            </div>
          </>
        )}

        {/* 锁屏通知 */}
        {allNotificationsEnabled && (
          <div className="notification-card">
            <div className="card-content">
              <span className="card-label">{t('allow-lock-screen-notification')}</span>
              <div className={`toggle-switch ${lockScreenEnabled ? 'enabled' : ''}`} onClick={handleToggleLockScreen}>
                <div className={`toggle-slider ${lockScreenEnabled ? 'enabled' : ''}`}></div>
              </div>
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
}

