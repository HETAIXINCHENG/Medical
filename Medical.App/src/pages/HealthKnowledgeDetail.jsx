import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { getAuthToken } from '../config/apiConfig.js';
import apiClient from '../services/apiClient.js';

export default function HealthKnowledgeDetail() {
  usePageStyles('health-knowledge-detail.css');
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [article, setArticle] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isFavorited, setIsFavorited] = useState(false);
  const [favoriteCount, setFavoriteCount] = useState(0);

  useEffect(() => {
    const loadArticle = async () => {
      setLoading(true);
      setError('');
      try {
        const detail = await medicalApi.getHealthKnowledgeById(id);
        setArticle(detail);
        setFavoriteCount(detail.favoriteCount || detail.FavoriteCount || 0);
        
        // 检查是否已收藏
        const token = getAuthToken();
        if (token) {
          try {
            const checkResult = await apiClient.get(`/api/healthknowledge/${id}/favorite/check`);
            setIsFavorited(checkResult.isFavorited || false);
          } catch (err) {
            console.error('检查收藏状态失败:', err);
          }
        }
      } catch (err) {
        setError(err.message ?? '加载文章失败');
      } finally {
        setLoading(false);
      }
    };
    loadArticle();
  }, [id]);

  const handleFavorite = async () => {
    const token = getAuthToken();
    if (!token) {
      navigate('/login', { state: { from: `/health-knowledge/${id}` } });
      return;
    }

    try {
      if (isFavorited) {
        // 取消收藏（如果需要取消收藏的API）
        // await apiClient.delete(`/api/healthknowledge/${id}/favorite`);
        alert('取消收藏功能待实现');
      } else {
        // 收藏
        await apiClient.post(`/api/healthknowledge/${id}/favorite`);
        setIsFavorited(true);
        setFavoriteCount(prev => prev + 1);
      }
    } catch (err) {
      if (err.message?.includes('已经收藏')) {
        setIsFavorited(true);
      } else {
        alert(err.message || '操作失败，请重试');
      }
    }
  };

  const formatTime = (dateString) => {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return '';
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    } catch (error) {
      return '';
    }
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">加载中...</div>
      </PageLayout>
    );
  }

  if (error || !article) {
    return (
      <PageLayout>
        <div className="error-tip">{error || '文章不存在'}</div>
      </PageLayout>
    );
  }

  const articleImageUrl = buildImageUrl(article.coverImageUrl || article.CoverImageUrl, '/Img/default-article.png');
  const authorName = article.authorName || article.AuthorName || '';
  const authorTitle = article.authorTitle || article.AuthorTitle || '';
  const authorDepartment = article.authorDepartment || article.AuthorDepartment || '';
  const authorHospital = article.authorHospital || article.AuthorHospital || '';
  const authorAvatar = article.authorAvatar || article.AuthorAvatar || '';

  return (
    <PageLayout>
      <div className="health-knowledge-detail">
        {/* 顶部导航 */}
        <div className="detail-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="back" />
          </button>
          <div className="header-title">文章详情</div>
          <button 
            className={`favorite-btn ${isFavorited ? 'favorited' : ''}`}
            onClick={handleFavorite}
          >
            {isFavorited ? '已收藏' : '收藏'}
          </button>
        </div>

        {/* 文章内容 */}
        <div className="article-detail">
          {/* 作者信息 */}
          <div className="author-section">
            <div className="author-avatar">
              <img 
                src={buildImageUrl(authorAvatar, '/Img/Director.png')} 
                alt={authorName}
                onError={(e) => {
                  e.target.src = '/Img/Director.png';
                }}
              />
            </div>
            <div className="author-info">
              <div className="author-name-title">
                <span className="author-name">{authorName}</span>
                {authorTitle && (
                  <span className="author-title">{authorTitle}</span>
                )}
              </div>
              <div className="author-department">
                {authorDepartment && `${authorDepartment} · `}
                {authorHospital || '医院'}
              </div>
              <div className="article-meta">
                {formatTime(article.createdAt || article.CreatedAt)} {t('from-doctor')}{authorName}{t('doctor-science')}
              </div>
            </div>
          </div>

          {/* 文章标题 */}
          <h1 className="article-title">{article.title || article.Title}</h1>

          {/* 文章封面图 */}
          {articleImageUrl && (
            <div className="article-cover">
              <img 
                src={articleImageUrl} 
                alt={article.title || article.Title}
                onError={(e) => {
                  e.target.src = '/Img/default-article.png';
                }}
              />
            </div>
          )}

          {/* 文章内容 */}
          <div className="article-content">
            {article.content || article.Content || article.summary || article.Summary || '暂无内容'}
          </div>

          {/* 底部统计 */}
          <div className="article-stats">
            <span className="stat-item">
              <span className="stat-icon">👁️</span>
              <span className="stat-text">阅读 {article.readCount || article.ReadCount || 0}</span>
            </span>
            <span className="stat-item">
              <span className="stat-icon">⭐</span>
              <span className="stat-text">收藏 {favoriteCount}</span>
            </span>
          </div>
        </div>
      </div>
    </PageLayout>
  );
}

