import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import apiClient from '../services/apiClient.js';
import { getAuthToken } from '../config/apiConfig.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Discover() {
  usePageStyles('discover.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState('featured');
  const [articles, setArticles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadArticles = async () => {
      setLoading(true);
      setError('');
      try {
        let articleData;
        if (activeTab === 'favorites') {
          // 收藏标签：获取当前用户收藏的健康知识
          articleData = await medicalApi.getMyFavorites({ pageSize: 20 });
        } else if (activeTab === 'subscriptions') {
          // 订阅标签：获取订阅医生发布的健康知识
          articleData = await apiClient.get('/api/subscriptions/health-knowledge', { 
            params: { pageSize: 20 } 
          });
        } else {
          // 精选标签：获取所有健康知识
          articleData = await medicalApi.getHealthKnowledge({ pageSize: 20 });
        }
        // 后端返回的是分页格式 { items, total, page, pageSize }，需要提取 items
        const articlesList = articleData?.items ?? articleData ?? [];
        setArticles(Array.isArray(articlesList) ? articlesList : []);
      } catch (err) {
        // 如果错误信息包含"未登录"或"401"，显示友好提示
        const errorMessage = err.message ?? t('load-articles-failed');
        if (errorMessage.includes('未登录') || errorMessage.includes('401') || errorMessage.includes('Unauthorized')) {
          if (activeTab === 'favorites') {
            setError(t('please-login-favorites'));
          } else if (activeTab === 'subscriptions') {
            setError(t('please-login-subscriptions'));
          } else {
            setError(errorMessage);
          }
        } else {
          setError(errorMessage);
        }
        setArticles([]);
      } finally {
        setLoading(false);
      }
    };
    loadArticles();
  }, [activeTab]);

  const formatTime = (dateString) => {
    if (!dateString) return '';
    try {
      // 解析日期字符串（可能是UTC时间）
      const date = new Date(dateString);
      
      // 检查日期是否有效
      if (isNaN(date.getTime())) {
        return '';
      }
      
      // 格式化为年月日 (YYYY-MM-DD)
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    } catch (error) {
      console.error('日期格式化错误:', error, dateString);
      return '';
    }
  };

  return (
    <PageLayout>
      {/* 页面标题 */}
      <div className="header">
        <h1 className="page-title">{t('health-science')}</h1>
      </div>

      {/* 标签页导航 */}
      <div className="tabs-nav">
        <div
          className={`tab-item ${activeTab === 'featured' ? 'active' : ''}`}
          onClick={() => setActiveTab('featured')}
        >
          {t('featured')}
        </div>
        <div
          className={`tab-item ${activeTab === 'favorites' ? 'active' : ''}`}
          onClick={() => setActiveTab('favorites')}
        >
          {t('favorites')}
        </div>
        <div
          className={`tab-item ${activeTab === 'subscriptions' ? 'active' : ''}`}
          onClick={() => {
            const token = getAuthToken();
            if (!token) {
              // 未登录，跳转到登录页
              navigate('/login', { state: { from: '/discover' } });
              return;
            }
            setActiveTab('subscriptions');
          }}
        >
          {t('subscriptions')}
        </div>
      </div>

      {/* 文章列表 */}
      <div className="articles-container">
        {loading && <div className="loading">{t('loading')}</div>}
        {error && <div className="error-tip">{error}</div>}
        {!loading && !error && articles.length === 0 && activeTab === 'subscriptions' && (
          <div className="empty-state">
            <div className="empty-icon">📭</div>
            <div className="empty-text">{t('no-subscription-content')}</div>
          </div>
        )}
        {!loading && !error && articles.length === 0 && activeTab !== 'subscriptions' && (
          <div className="empty">{t('no-content')}</div>
        )}
        {articles.map((article) => (
          <div className="article-post" key={article.id}>
            <Link 
              to={`/health-knowledge/${article.id}`}
              className="article-link"
            >
            <div className="post-header">
              <div className="doctor-avatar">
                <img
                  src={buildImageUrl(article.authorAvatar, '/Img/Director.png')}
                  alt={article.authorName ?? '医生'}
                  className="avatar-img"
                  onError={(e) => {
                    e.target.src = '/Img/Director.png';
                  }}
                />
              </div>
              <div className="doctor-info">
                <div className="doctor-name-title">
                  <span className="doctor-name">{article.authorName ?? '医生'}</span>
                  {article.authorTitle && (
                    <span className="doctor-title">{article.authorTitle}</span>
                  )}
                </div>
                <div className="doctor-hospital">
                  {article.authorDepartment && `${article.authorDepartment} · `}
                  {article.authorHospital ?? '医院'}
                </div>
                <div className="post-meta">
                  {formatTime(article.createdAt)} {t('from-doctor')}{article.authorName ?? t('doctor')}{t('doctor-science')}
                </div>
              </div>
            </div>
            <div className="post-content">
              <div className="post-main">
                <h3 className="post-title">{article.title}</h3>
                <p className="post-excerpt">
                  {article.summary ?? article.content?.slice(0, 100) ?? '暂无摘要'}
                </p>
              </div>
              <div className="post-thumbnail">
                <img 
                  src={buildImageUrl(article.coverImageUrl, '/Img/Director.png')} 
                  alt={article.title} 
                  className="thumbnail-img"
                  onError={(e) => {
                    // 如果图片加载失败，使用默认图片
                    e.target.src = '/Img/Director.png';
                  }}
                />
              </div>
            </div>
            </Link>
            <div className="post-actions">
              <span className="action-item">
                <span className="action-icon">👁️</span>
                <span className="action-text">{t('read')} {article.readCount ?? 0}</span>
              </span>
              <span className="action-item">
                <span className="action-icon">⭐</span>
                <span className="action-text">{t('favorite')} {article.favoriteCount ?? 0}</span>
              </span>
            </div>
          </div>
        ))}
      </div>

      {/* 浮动添加按钮（仅在订阅标签显示） */}
      {activeTab === 'subscriptions' && (
        <Link 
          to="/subscription-manage" 
          className="fab-button"
          onClick={(e) => {
            const token = getAuthToken();
            if (!token) {
              e.preventDefault();
              navigate('/login', { state: { from: '/subscription-manage' } });
            }
          }}
        >
          <span className="fab-icon">+</span>
        </Link>
      )}
    </PageLayout>
  );
}
