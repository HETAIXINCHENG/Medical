import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function ResetPassword() {
  usePageStyles('reset-password.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useLanguage();
  const phone = searchParams.get('phone');
  const code = searchParams.get('code');
  const from = searchParams.get('from'); // 区分是从忘记密码还是修改密码进入
  
  const [user, setUser] = useState(null);
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // 如果是修改密码流程，不需要验证 phone 和 code
    if (from !== 'modify' && (!phone || !code)) {
      navigate('/forgot-password');
    }

    // 加载用户信息
    const loadUser = async () => {
      try {
        const savedUser = localStorage.getItem('user');
        if (savedUser) {
          const userData = JSON.parse(savedUser);
          setUser(userData);
        }
      } catch (err) {
        console.error('加载用户信息失败:', err);
      }
    };
    loadUser();
  }, [phone, code, from, navigate]);

  // 格式化手机号显示（中间4位用*代替）
  const formatPhone = (phoneNumber) => {
    if (!phoneNumber) return '188****6509';
    if (phoneNumber.length === 11) {
      return phoneNumber.substring(0, 3) + '****' + phoneNumber.substring(7);
    }
    return phoneNumber;
  };

  // 获取当前账号（用户名或手机号）
  const getCurrentAccount = () => {
    if (user?.username) return user.username;
    if (user?.phoneNumber) return user.phoneNumber;
    return phone || '18800066509';
  };

  // 获取绑定手机
  const getBoundPhone = () => {
    if (user?.phoneNumber) return formatPhone(user.phoneNumber);
    return formatPhone(phone) || '188****6509';
  };

  const validatePassword = (pwd) => {
    // 6-20位字母或数字
    if (pwd.length < 6 || pwd.length > 20) {
      return t('password-length-error');
    }
    if (!/^[a-zA-Z0-9]+$/.test(pwd)) {
      return t('password-format-error');
    }
    return '';
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const passwordError = validatePassword(password);
    if (passwordError) {
      setError(passwordError);
      return;
    }

    if (password !== confirmPassword) {
      setError(t('password-mismatch'));
      return;
    }

    setLoading(true);
    try {
      // TODO: 调用API重置密码
      // await medicalApi.resetPassword({ phone, code, password });
      
      // 模拟API调用
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      alert(t('password-reset-success'));
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message || t('password-reset-failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="password-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">{t('reset-password')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="password-content">
        <form className="password-form" onSubmit={handleSubmit}>
          <div className="account-info">
            <p className="account-text">{t('current-account-label')}: {getCurrentAccount()}</p>
            <p className="account-text">{t('bound-phone-label')}: {getBoundPhone()}</p>
          </div>
          <p className="form-hint">{t('enter-new-password-hint')}</p>

          <div className="form-group">
            <input
              type={showPassword ? 'text' : 'password'}
              className="form-input"
              placeholder={t('enter-new-password')}
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                setError('');
              }}
              maxLength={20}
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              <span className="eye-icon">{showPassword ? '👁️' : '👁️‍🗨️'}</span>
            </button>
          </div>

          <div className="form-group">
            <input
              type={showConfirmPassword ? 'text' : 'password'}
              className="form-input"
              placeholder={t('enter-new-password-again')}
              value={confirmPassword}
              onChange={(e) => {
                setConfirmPassword(e.target.value);
                setError('');
              }}
              maxLength={20}
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
            >
              <span className="eye-icon">{showConfirmPassword ? '👁️' : '👁️‍🗨️'}</span>
            </button>
          </div>

          {error && <div className="error-message">{error}</div>}

          <button
            type="submit"
            className="submit-btn"
            disabled={loading || !password || !confirmPassword}
          >
            {loading ? t('setting') : t('set-new-password')}
          </button>
        </form>
      </div>
    </PageLayout>
  );
}

