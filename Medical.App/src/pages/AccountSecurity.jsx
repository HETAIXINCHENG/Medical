import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function AccountSecurity() {
  usePageStyles('account-security.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [user, setUser] = useState(null);
  const [phone, setPhone] = useState('');

  useEffect(() => {
    const loadUser = async () => {
      try {
        const savedUser = localStorage.getItem('user');
        if (savedUser) {
          const userData = JSON.parse(savedUser);
          setUser(userData);
          // 从用户数据中获取手机号，如果没有则显示占位符
          setPhone(userData.phoneNumber || '188****6509');
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

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="account-security-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('account-security')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="account-security-content">
        {/* 修改绑定手机 */}
        <div className="security-item" onClick={() => navigate('/verify-phone')}>
          <span className="security-label">{t('modify-phone')}</span>
          <div className="security-right">
            <span className="security-value">{formatPhone(phone)}</span>
            <span className="security-arrow">›</span>
          </div>
        </div>

        {/* 修改密码 */}
        <div className="security-item" onClick={() => navigate('/modify-password')}>
          <span className="security-label">{t('modify-password')}</span>
          <span className="security-arrow">›</span>
        </div>

        {/* 注销账号 */}
        <div className="security-item" onClick={() => navigate('/cancel-account')}>
          <span className="security-label">{t('cancel-account')}</span>
          <span className="security-arrow">›</span>
        </div>
      </div>
    </PageLayout>
  );
}

