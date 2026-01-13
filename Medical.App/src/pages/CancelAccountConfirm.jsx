import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function CancelAccountConfirm() {
  usePageStyles('cancel-account-confirm.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
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

  // 获取当前账号
  const getCurrentAccount = () => {
    if (user?.username) return user.username;
    if (user?.phoneNumber) return user.phoneNumber;
    return phone || '18800066509';
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

  const handleConfirm = async () => {
    if (!code || code.length !== 6) {
      setError(t('enter-6-digit-code'));
      return;
    }

    // TODO: 调用API确认注销
    // await medicalApi.cancelAccount({ phone, code });
    
    // 模拟API调用
    if (window.confirm(t('confirm-cancel-account'))) {
      // 清除本地存储
      localStorage.removeItem('medical-jwt');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      alert(t('account-cancelled'));
      navigate('/login');
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="cancel-confirm-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('cancel-account')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="cancel-confirm-content">
        <p className="warning-text">{t('cancelling-account')}</p>
        <p className="account-number">{getCurrentAccount()}</p>

        {/* 手机号输入（只读） */}
        <div className="form-group">
          <input
            type="text"
            className="form-input readonly"
            value={formatPhone(phone)}
            readOnly
          />
        </div>

        {/* 验证码输入 */}
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

        {/* 确认注销按钮 */}
        <button
          type="button"
          className="confirm-btn"
          onClick={handleConfirm}
          disabled={!code || code.length !== 6}
        >
          {t('confirm-cancel')}
        </button>
      </div>
    </PageLayout>
  );
}

