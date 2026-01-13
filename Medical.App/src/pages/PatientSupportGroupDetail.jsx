import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { getAuthToken } from '../config/apiConfig.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { initWeChatShare, isWeChat } from '../utils/wechatShare.js';

export default function PatientSupportGroupDetail() {
  usePageStyles('patient-support-group-detail.css');
  const { doctorId } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [group, setGroup] = useState(null);
  const [posts, setPosts] = useState([]);
  const [sortBy, setSortBy] = useState('reply'); // 'reply' 或 'post'
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [shareInitialized, setShareInitialized] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      if (!doctorId) return;
      
      setLoading(true);
      setError('');
      try {
        // 先获取患友会信息
        const groupData = await medicalApi.getPatientSupportGroupByDoctor(doctorId);
        setGroup(groupData);
        
        // 然后获取帖子列表（使用group的ID）
        const postsData = await medicalApi.getPosts(groupData.id, { sortBy, pageSize: 20 });
        const postsList = postsData?.items ?? postsData ?? [];
        setPosts(Array.isArray(postsList) ? postsList : []);

        // 初始化微信分享
        if (isWeChat() && groupData) {
          initShare(groupData);
        }
      } catch (err) {
        setError(err.message ?? t('load-data-failed'));
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [doctorId, sortBy]);

  // 初始化微信分享
  const initShare = async (groupData) => {
    if (shareInitialized) return;

    try {
      const currentUrl = window.location.href.split('#')[0];
      
      const success = await initWeChatShare(
        async () => {
          const signature = await medicalApi.getWeChatSignature(currentUrl);
          return signature;
        },
        {
          title: `${groupData.name} - 医疗咨询系统`,
          desc: groupData.description || `欢迎加入${groupData.name}，与患友一起交流`,
          link: currentUrl,
          imgUrl: groupData.doctorAvatarUrl 
            ? buildImageUrl(groupData.doctorAvatarUrl) 
            : `${window.location.origin}/Img/Director.png`
        }
      );

      if (success) {
        setShareInitialized(true);
      }
    } catch (err) {
      console.error('初始化微信分享失败:', err);
    }
  };

  // 处理分享按钮点击
  const handleShare = (post) => {
    if (isWeChat()) {
      // 在微信中，需要为每个帖子单独配置分享
      const currentUrl = `${window.location.origin}/post-detail/${post.id}`;
      initWeChatShare(
        async () => {
          const signature = await medicalApi.getWeChatSignature(currentUrl);
          return signature;
        },
        {
          title: post.title || '医疗咨询系统',
          desc: post.content ? (post.content.length > 100 ? post.content.substring(0, 100) + '...' : post.content) : '查看详情',
          link: currentUrl,
          imgUrl: post.authorAvatarUrl 
            ? buildImageUrl(post.authorAvatarUrl) 
            : `${window.location.origin}/Img/Director.png`
        }
      ).then(() => {
        alert('请点击右上角菜单按钮进行分享');
      });
    } else {
      // 非微信环境，复制链接
      const url = `${window.location.origin}/post-detail/${post.id}`;
      if (navigator.clipboard) {
        navigator.clipboard.writeText(url).then(() => {
          alert('链接已复制到剪贴板');
        });
      }
    }
  };

  const formatTime = (date) => {
    if (!date) return '';
    const d = new Date(date);
    const now = new Date();
    const diff = now - d;
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    
    if (days === 0) return t('today');
    if (days === 1) return `1${t('days-ago')}`;
    if (days < 7) return `${days}${t('days-ago')}`;
    if (days < 30) return `${Math.floor(days / 7)}${t('weeks-ago')}`;
    if (days < 365) return `${Math.floor(days / 30)}${t('months-ago')}`;
    return `${Math.floor(days / 365)}${t('years-ago')}`;
  };

  const handlePostClick = async (postId) => {
    // 增加阅读数
    try {
      await medicalApi.incrementPostReadCount(postId);
    } catch (err) {
      console.error('增加阅读数失败:', err);
    }
    // 跳转到帖子详情页
    navigate(`/post-detail/${postId}`);
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">{t('loading')}</div>
      </PageLayout>
    );
  }

  if (error || !group) {
    return (
      <PageLayout>
        <div className="error-tip">{error || t('group-not-exist')}</div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      {/* 头部 */}
      <div className="header">
        <div className="header-back" onClick={() => navigate(-1)}>
          <span className="back-arrow">←</span>
        </div>
        <div className="header-content">
          <div className="doctor-avatar-header">
            <img
              src={buildImageUrl(group.doctorAvatarUrl, '/Img/Director.png')}
              alt={group.doctorName}
              className="header-avatar"
              onError={(e) => {
                e.target.src = '/Img/Director.png';
              }}
            />
          </div>
          <div className="header-info">
            <h1 className="header-title">{group.name}</h1>
            <div className="header-stats">
              <span>{group.postCount}{t('people-published')}</span>
              <span>{group.totalReadCount}{t('people-viewed')}</span>
            </div>
          </div>
        </div>
      </div>

      {/* 会规按钮 */}
      <div className="rules-section">
        <Link to={`/group-rules/${group.id}`} className="rules-button">
          {t('group-rules')}
        </Link>
        <span className="rules-text">{group.name}{t('group-rules')}</span>
      </div>

      {/* 排序选项 */}
      <div className="sort-section">
        <span className="sort-label">{t('post-order')}</span>
        <div className="sort-buttons">
          <button
            className={`sort-btn ${sortBy === 'reply' ? 'active' : ''}`}
            onClick={() => setSortBy('reply')}
          >
            {t('reply-first')}
          </button>
          <button
            className={`sort-btn ${sortBy === 'post' ? 'active' : ''}`}
            onClick={() => setSortBy('post')}
          >
            {t('publish-first')}
          </button>
        </div>
      </div>

      {/* 帖子列表 */}
      <div className="posts-list">
        {posts.length === 0 ? (
          <div className="empty">{t('no-posts')}</div>
        ) : (
          posts.map((post) => (
            <div
              key={post.id}
              className="post-card"
              onClick={() => handlePostClick(post.id)}
            >
              <div className="post-header">
                <div className="post-author">
                  <div className="author-avatar">
                    <img 
                      src={buildImageUrl(post.authorAvatarUrl, '/Img/Director.png')} 
                      alt={t('avatar')}
                      onError={(e) => {
                        e.target.src = '/Img/Director.png';
                      }}
                    />
                  </div>
                  <div className="author-info">
                    <div className="author-name">{post.authorDisplayName}</div>
                    <div className="post-meta">
                      {t('replied-at')}{formatTime(post.lastReplyAt || post.createdAt)} {post.readCount}{t('people-read')}
                    </div>
                  </div>
                </div>
                {post.tag && (
                  <span className="post-tag">{post.tag}</span>
                )}
              </div>
              <div className="post-title">{post.title}</div>
              <div className="post-content">{post.content}</div>
              <div className="post-actions">
                <span className="action-item" onClick={(e) => { e.stopPropagation(); handleShare(post); }}>
                  {t('share')}
                </span>
                <span className="action-item" onClick={(e) => { e.stopPropagation(); navigate(`/post-detail/${post.id}`); }}>
                  {t('comment')} {post.commentCount}
                </span>
                <span className="action-item">{t('like')} {post.likeCount}</span>
              </div>
            </div>
          ))
        )}
      </div>

      {/* 浮动发布按钮 */}
      <div
        className="fab-button"
        onClick={() => {
          const token = getAuthToken();
          if (!token) {
            navigate('/login', { state: { from: `/patient-support-group/${doctorId}` } });
            return;
          }
          navigate(`/create-post/${group.id}`);
        }}
      >
        <span className="fab-icon">+</span>
      </div>
    </PageLayout>
  );
}

