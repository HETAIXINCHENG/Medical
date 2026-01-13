import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function VerifyPhone() {
  usePageStyles('verify-phone.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [phone, setPhone] = useState('');
  const [code, setCode] = useState('');
  const [countdown, setCountdown] = useState(0);
  const [error, setError] = useState('');

  const handleGetCode = async () => {
    if (!phone || phone.length !== 11) {
      setError(t('please-enter-correct-phone'));
      return;
    }

    // TODO: 调用API发送验证码
    // await medicalApi.sendVerificationCode({ phone });
    
    // 开始倒计时
    setCountdown(60);
    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const handleNext = () => {
    if (!phone || phone.length !== 11) {
      setError(t('please-enter-correct-phone'));
      return;
    }
    if (!code || code.length !== 6) {
      setError(t('enter-6-digit-code'));
      return;
    }

    // 跳转到修改手机号页面（或返回）
    navigate(-1);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="verify-phone-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('verify-phone')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="verify-phone-content">
        <p className="verify-hint">{t('verify-current-phone')}</p>

        <div className="form-group">
          <input
            type="tel"
            className="form-input"
            placeholder={t('enter-bound-phone')}
            value={phone}
            onChange={(e) => {
              setPhone(e.target.value);
              setError('');
            }}
            maxLength={11}
          />
        </div>

        <div className="form-group code-group">
          <input
            type="text"
            className="form-input code-input"
            placeholder={t('enter-verification-code')}
            value={code}
            onChange={(e) => {
              setCode(e.target.value);
              setError('');
            }}
            maxLength={6}
          />
          <button
            type="button"
            className="get-code-btn"
            onClick={handleGetCode}
            disabled={countdown > 0}
          >
            {countdown > 0 ? `${countdown}${t('seconds')}` : t('get-code')}
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <button
          type="button"
          className="next-btn"
          onClick={handleNext}
          disabled={!phone || !code}
        >
          {t('next-step')}
        </button>
      </div>
    </PageLayout>
  );
}

