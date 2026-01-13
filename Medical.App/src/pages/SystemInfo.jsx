import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function SystemInfo() {
  usePageStyles('system-info.css');
  const navigate = useNavigate();
  const { t } = useLanguage();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="system-info-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('system-info')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="system-info-content">
        {/* 应用信息卡片 */}
        <div className="info-card">
          <div className="app-icon-section">
            <div className="app-icon">
              <span className="icon-text">医</span>
            </div>
            <div className="app-name">{t('medical-consultation-system')}</div>
            <div className="app-version">{t('version')} 10.8.6</div>
          </div>
        </div>

        {/* 系统介绍 */}
        <div className="info-section">
          <div className="section-title">{t('system-introduction')}</div>
          <div className="section-content">
            <p className="info-text">
              {t('system-intro-text')}
            </p>
          </div>
        </div>

        {/* 核心功能 */}
        <div className="info-section">
          <div className="section-title">{t('core-features')}</div>
          <div className="function-list">
            <div className="function-item">
              <div className="function-icon">👨‍⚕️</div>
              <div className="function-content">
                <div className="function-title">{t('online-consultation')}</div>
                <div className="function-desc">{t('online-consultation-desc')}</div>
              </div>
            </div>
            <div className="function-item">
              <div className="function-icon">📚</div>
              <div className="function-content">
                <div className="function-title">{t('health-science-knowledge')}</div>
                <div className="function-desc">{t('health-science-desc')}</div>
              </div>
            </div>
            <div className="function-item">
              <div className="function-icon">👥</div>
              <div className="function-content">
                <div className="function-title">{t('patient-community')}</div>
                <div className="function-desc">{t('patient-community-desc')}</div>
              </div>
            </div>
            <div className="function-item">
              <div className="function-icon">📋</div>
              <div className="function-content">
                <div className="function-title">{t('health-records')}</div>
                <div className="function-desc">{t('health-records-desc')}</div>
              </div>
            </div>
          </div>
        </div>

        {/* 联系方式 */}
        <div className="info-section">
          <div className="section-title">{t('contact-us')}</div>
          <div className="contact-list">
            <div className="contact-item">
              <span className="contact-label">{t('customer-service')}</span>
              <span className="contact-value">400-888-8888</span>
            </div>
            <div className="contact-item">
              <span className="contact-label">{t('service-hours')}</span>
              <span className="contact-value">{t('service-hours-value')}</span>
            </div>
            <div className="contact-item">
              <span className="contact-label">{t('official-email')}</span>
              <span className="contact-value">service@medical.com</span>
            </div>
          </div>
        </div>

        {/* 版权信息 */}
        <div className="copyright-section">
          <div className="copyright-text">© 2024 {t('medical-consultation-system')}</div>
          <div className="copyright-text">{t('all-rights-reserved')}</div>
        </div>
      </div>
    </PageLayout>
  );
}

