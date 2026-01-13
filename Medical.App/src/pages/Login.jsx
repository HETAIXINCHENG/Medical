import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Login() {
  usePageStyles('login.css');
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useLanguage();
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [agreed, setAgreed] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // 从 location.state 或 URL 参数获取跳转前的路径
  const urlParams = new URLSearchParams(window.location.search);
  const fromParam = urlParams.get('from');
  const from = location.state?.from || fromParam || '/';

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');

    if (!phone || !password) {
      setError(t('enter-phone-password'));
      return;
    }

    if (!agreed) {
      setError(t('agree-terms'));
      return;
    }

    setLoading(true);
    try {
      const response = await medicalApi.login({
        username: phone,
        password: password
      });

      // 保存 token
      if (response.token) {
        localStorage.setItem('medical-jwt', response.token);
        localStorage.setItem('token', response.token); // 兼容旧代码
        localStorage.setItem('user', JSON.stringify(response.user));
      }

      // 跳转到之前的页面或首页
      navigate(from, { replace: true });
    } catch (err) {
      setError(err.response?.data?.message || t('login-failed'));
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = () => {
    navigate('/register', { state: { from } });
  };

  return (
    <PageLayout>
      <div className="login-container">
        {/* 顶部帮助 */}
        <div className="login-header">
          <div></div>
          <Link to="/login-help" className="help-link">{t('login-problem')}</Link>
        </div>

        {/* Logo和标题 */}
        <div className="logo-section">
          <div className="app-logo">
            <div className="logo-icon">👨‍⚕️</div>
          </div>
          <h1 className="app-name">{t('app-name')}</h1>
        </div>

        {/* 登录表单 */}
        <form className="login-form" onSubmit={handleLogin}>
          <div className="form-group">
            <input
              type="text"
              className="form-input"
              placeholder={t('phone-placeholder')}
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              maxLength={11}
            />
          </div>

          <div className="form-group">
            <input
              type={showPassword ? 'text' : 'password'}
              className="form-input"
              placeholder={t('password-placeholder')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <button
              type="button"
              className="password-toggle"
              onClick={() => setShowPassword(!showPassword)}
            >
              <span className="eye-icon">{showPassword ? '👁️' : '👁️‍🗨️'}</span>
            </button>
          </div>

          <div className="form-options">
            <Link to="/forgot-password" className="forgot-password">{t('forgot-password')}</Link>
          </div>

          {error && <div className="error-message">{error}</div>}

          <button type="submit" className="login-btn" disabled={loading}>
            {loading ? t('logging') : t('login')}
          </button>

          {/* 协议同意 */}
          <div className="agreement-section">
            <label className="agreement-checkbox">
              <input
                type="checkbox"
                checked={agreed}
                onChange={(e) => setAgreed(e.target.checked)}
              />
              <span className="checkbox-label">
                {t('read-agreed')}
                <Link to="/service-agreement" className="agreement-link">{t('service-agreement')}</Link>
                {t('and')}
                <Link to="/privacy-policy" className="agreement-link">{t('privacy-policy')}</Link>
              </span>
            </label>
          </div>
        </form>

      </div>
    </PageLayout>
  );
}

