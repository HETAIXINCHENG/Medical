import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function ModifyPassword() {
  usePageStyles('modify-password.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [user, setUser] = useState(null);
  const [phone, setPhone] = useState('');
  const [code, setCode] = useState('');
  const [countdown, setCountdown] = useState(0);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadUser = async () => {
      try {
        const savedUser = localStorage.getItem('user');
        if (savedUser) {
          const userData = JSON.parse(savedUser);
          setUser(userData);
          // 从用户数据中获取手机号
          const phoneNumber = userData.phoneNumber || '18800066509';
          setPhone(phoneNumber);
        }
      } catch (err) {
        console.error('加载用户信息失败:', err);
      }
    };
    loadUser();
  }, []);

  // 格式化手机号显示（中间4位用*代替）
  const formatPhone = (phoneNumber) => {
    if (!phoneNumber) return '188****6509';
    if (phoneNumber.length === 11) {
      return phoneNumber.substring(0, 3) + '****' + phoneNumber.substring(7);
    }
    return phoneNumber;
  };

  const handleGetCode = async () => {
    if (!phone || phone.length !== 11) {
      setError(t('phone-format-incorrect'));
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
    if (!code || code.length !== 6) {
      setError(t('enter-6-digit-code'));
      return;
    }

    // 跳转到重置密码页面
    navigate(`/reset-password?phone=${phone}&code=${code}&from=modify`);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="modify-password-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('modify-password')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="modify-password-content">
        <p className="modify-hint">{t('verify-current-bound-phone')}</p>
        <p className="phone-info">{t('bound-phone-label')}: {formatPhone(phone)}</p>

        <div className="form-group code-group">
          <input
            type="text"
            className="form-input code-input"
            placeholder={t('enter-verification-code')}
            value={code}
            onChange={(e) => {
              setCode(e.target.value.replace(/\D/g, ''));
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
          disabled={!code || code.length !== 6}
        >
          {t('next-step')}
        </button>
      </div>
    </PageLayout>
  );
}

