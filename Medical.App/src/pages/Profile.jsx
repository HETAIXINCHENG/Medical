import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { buildImageUrl } from '../utils/imageUtils.js';

export default function Profile() {
  usePageStyles('profile.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadUser = async () => {
      try {
        // 先尝试从 localStorage 获取
        const savedUser = localStorage.getItem('user');
        if (savedUser) {
          try {
            const userData = JSON.parse(savedUser);
            setUser(userData);
            setLoading(false);
          } catch (e) {
            // JSON解析失败，忽略
          }
        }

        // 然后从 API 获取最新信息（静默失败，不显示错误）
        const token = localStorage.getItem('medical-jwt') || localStorage.getItem('token');
        if (token) {
          try {
            const currentUser = await medicalApi.getCurrentUser();
            if (currentUser) {
              setUser(currentUser);
              localStorage.setItem('user', JSON.stringify(currentUser));
            }
          } catch (err) {
            // 静默失败，使用 localStorage 中的数据
            // 不显示错误，避免控制台噪音
          }
        }
      } catch (err) {
        // 静默处理所有错误
      } finally {
        setLoading(false);
      }
    };

    loadUser();
  }, []);

  // 获取用户名的首字母作为头像
  const getAvatarLetter = () => {
    if (!user?.username) return 'U';
    return user.username.charAt(0).toUpperCase();
  };

  // 处理头像加载错误
  const handleAvatarError = (e) => {
    // 如果图片加载失败，隐藏img并显示首字母
    e.target.style.display = 'none';
    const parent = e.target.parentNode;
    if (!parent.querySelector('.avatar-circle')) {
      const letterDiv = document.createElement('div');
      letterDiv.className = 'avatar-circle';
      letterDiv.textContent = getAvatarLetter();
      parent.appendChild(letterDiv);
    }
  };

  return (
    <PageLayout>
      {/* 页面标题 */}
      <div className="header">
        <h1 className="page-title">{t('profile')}</h1>
        <div className="header-icons">
          <Link to="/settings">
            <img src="/Img/setting.png" alt={t('settings')} className="settings-icon" />
          </Link>
        </div>
      </div>

      {/* 用户资料 */}
      <div className="user-profile">
        <div className="avatar">
          {user?.avatarUrl ? (
            <img 
              src={buildImageUrl(user.avatarUrl)} 
              alt={t('avatar')}
              className="avatar-img"
              onError={handleAvatarError}
            />
          ) : (
            <div className="avatar-circle">{getAvatarLetter()}</div>
          )}
        </div>
        <div className="user-info">
          {user ? (
            <>
              <div className="username">{user.username}</div>
              <div className="user-prompt">{user.bio || t('fill-bio')}</div>
            </>
          ) : (
            <>
              <div 
                className="username clickable" 
                onClick={() => navigate('/login?from=/profile')}
                style={{ cursor: 'pointer' }}
              >
                {t('not-logged-in')}
              </div>
              <div className="user-prompt">{t('fill-bio')}</div>
            </>
          )}
        </div>
      </div>

      {/* 信息卡片区域 */}
      <div className="info-cards-section">
        <div className="info-cards">
          <div className="info-card">
            <div className="order-type-icon">
              <img src="/Img/test-50.png" alt={t('pending-payment')} />
            </div>
            <div className="info-card-label">{t('todo')}</div>
            <div className="info-card-count">(1)</div>
          </div>
          <div className="info-card">
            <div className="order-type-icon">
              <img src="/Img/medical-48.png" alt={t('pending-payment')} />
            </div>
            <div className="info-card-label">{t('my-consultation')}</div>
            <div className="info-card-count">(26)</div>
          </div>
          <Link to="/merchant-services" className="info-card">
            <div className="order-type-icon">
              <img src="/Img/website-48.png" alt={t('merchant-service')} />
            </div>
            <div className="info-card-label">{t('merchant-service')}</div>
            <div className="info-card-count">(4)</div>
          </Link>
        </div>
      </div>

      {/* 我的订单 */}
      <div className="orders-section">
        <h2 className="section-title">{t('my-orders')}</h2>
        <div className="order-types">
          <Link to="/my-orders?status=pending" className="order-type-item">
            <div className="order-type-icon">
              <img src="/Img/payment-50.png" alt={t('pending-payment')} />
            </div>
            <span className="order-type-label">{t('pending-payment')}</span>
          </Link>
          <Link to="/my-orders?status=in-progress" className="order-type-item">
            <div className="order-type-icon">
              <img src="/Img/data-quality-50.png" alt={t('pending-payment')} />
            </div>
            <span className="order-type-label">{t('in-progress')}</span>
          </Link>
          <Link to="/my-orders?status=completed" className="order-type-item">
            <div className="order-type-icon">
              <img src="/Img/documents-folder-50.png" alt={t('pending-payment')} />
            </div>
            <span className="order-type-label">{t('completed')}</span>
          </Link>
          <Link to="/my-orders?status=cancelled" className="order-type-item">
            <div className="order-type-icon">
              <img src="/Img/icons8-return-64.png" alt={t('pending-payment')} />
            </div>
            <span className="order-type-label">{t('cancelled')}</span>
          </Link>
        </div>
      </div>

      {/* 我的服务 */}
      <div className="section">
        <h2 className="section-title">{t('my-services')}</h2>
        <div className="service-grid">
          <Link to="/my-prescriptions" className="service-item">
            <div className="order-type-icon">
              <img src="/Img/choice-50.png" alt={t('pending-payment')} />
            </div>
            <span className="service-label">{t('my-prescription')}</span>
          </Link>
          <Link to="/my-reviews" className="service-item">
            <div className="order-type-icon">
              <img src="/Img/like-50.png" alt={t('pending-payment')} />
            </div>
            <span className="service-label">{t('my-reviews')}</span>
          </Link>
          <Link to="/my-doctors" className="service-item">
            <div className="order-type-icon">
              <img src="/Img/doctor-50.png" alt={t('my-doctors')} />
            </div>
            <span className="service-label">{t('my-doctors')}</span>
          </Link>
          <Link to="/medical-record-list" className="service-item">
            <div className="order-type-icon">
              <img src="/Img/case-history-64.png" alt={t('my-medical-records')} />
            </div>
            <span className="service-label">{t('my-medical-records')}</span>
          </Link>
        </div>
      </div>
    </PageLayout>
  );
}
