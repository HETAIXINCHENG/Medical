import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function NotificationSettings() {
  usePageStyles('notification-settings.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  
  // 通知开关状态
  const [systemNotificationEnabled, setSystemNotificationEnabled] = useState(false);
  const [doctorReplyEnabled, setDoctorReplyEnabled] = useState(false);
  const [orderMessageEnabled, setOrderMessageEnabled] = useState(false);
  const [platformMessageEnabled, setPlatformMessageEnabled] = useState(false);

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="notification-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('notification-settings')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="notification-content">
        {/* 系统通知 */}
        <div className="notification-section">
          <div className="section-title">{t('system-notification')}</div>
          <div className="notification-item" onClick={() => navigate('/system-notification-settings')}>
            <span className="item-label">{t('receive-system-notification')}</span>
            <div className="item-right">
              <span className={systemNotificationEnabled ? 'status-enabled' : 'status-disabled'}>
                {systemNotificationEnabled ? t('enabled') : t('disabled-go-settings')}
              </span>
              <span className="item-arrow">›</span>
            </div>
          </div>
        </div>

        {/* APP消息通知 */}
        <div className="notification-section">
          <div className="section-title">{t('app-message-notification')}</div>
          <div className="notification-item">
            <span className="item-label">{t('doctor-reply-message')}</span>
            <div className={`toggle-switch ${doctorReplyEnabled ? 'enabled' : ''}`} onClick={() => setDoctorReplyEnabled(!doctorReplyEnabled)}>
              <div className={`toggle-slider ${doctorReplyEnabled ? 'enabled' : ''}`}></div>
            </div>
          </div>
          <div className="notification-item">
            <span className="item-label">{t('order-message')}</span>
            <div className={`toggle-switch ${orderMessageEnabled ? 'enabled' : ''}`} onClick={() => setOrderMessageEnabled(!orderMessageEnabled)}>
              <div className={`toggle-slider ${orderMessageEnabled ? 'enabled' : ''}`}></div>
            </div>
          </div>
          <div className="notification-item">
            <span className="item-label">{t('platform-message')}</span>
            <div className={`toggle-switch ${platformMessageEnabled ? 'enabled' : ''}`} onClick={() => setPlatformMessageEnabled(!platformMessageEnabled)}>
              <div className={`toggle-slider ${platformMessageEnabled ? 'enabled' : ''}`}></div>
            </div>
          </div>
        </div>
      </div>
    </PageLayout>
  );
}

