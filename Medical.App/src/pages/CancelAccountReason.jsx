import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function CancelAccountReason() {
  usePageStyles('cancel-account-reason.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [selectedReason, setSelectedReason] = useState('');

  const reasons = [
    { value: 'security', label: t('security-privacy') },
    { value: 'new-phone', label: t('new-phone') },
    { value: 'bad-experience', label: t('bad-experience') },
    { value: 'multiple-accounts', label: t('multiple-accounts') },
    { value: 'not-needed', label: t('not-needed') },
    { value: 'other', label: t('other-reason') }
  ];

  const handleNext = () => {
    if (!selectedReason) {
      return;
    }
    navigate(`/cancel-account-confirm?reason=${selectedReason}`);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="cancel-reason-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('cancel-account')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="cancel-reason-content">
        <p className="intro-text">{t('tell-us-reason')}</p>

        {/* 原因列表 */}
        <div className="reason-list">
          {reasons.map((reason) => (
            <div key={reason.value} className="reason-item">
              <label className="reason-label">
                <input
                  type="radio"
                  name="reason"
                  value={reason.value}
                  checked={selectedReason === reason.value}
                  onChange={(e) => setSelectedReason(e.target.value)}
                  className="reason-radio"
                />
                <span className="reason-text">{reason.label}</span>
              </label>
              {reason.value === 'security' && (
                <span className="more-link" onClick={() => {}}>
                  {t('more-privacy')} &gt;
                </span>
              )}
            </div>
          ))}
        </div>

        {/* 下一步按钮 */}
        <button
          className="next-btn"
          onClick={handleNext}
          disabled={!selectedReason}
        >
          {t('next-step')}
        </button>
      </div>
    </PageLayout>
  );
}

