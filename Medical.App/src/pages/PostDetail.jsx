import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { getAuthToken } from '../config/apiConfig.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { initWeChatShare, isWeChat } from '../utils/wechatShare.js';

export default function PostDetail() {
  usePageStyles('post-detail.css');
  const { postId } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [post, setPost] = useState(null);
  const [comments, setComments] = useState([]);
  const [commentText, setCommentText] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isLiked, setIsLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  const [submitting, setSubmitting] = useState(false);
  const [shareInitialized, setShareInitialized] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      if (!postId) return;
      
      setLoading(true);
      setError('');
      try {
        // 获取帖子详情
        const postData = await medicalApi.getPostById(postId);
        setPost(postData);
        setLikeCount(postData.likeCount || 0);
        
        // 增加阅读数
        try {
          await medicalApi.incrementPostReadCount(postId);
          if (postData.readCount !== undefined) {
            setPost(prev => ({ ...prev, readCount: (prev.readCount || 0) + 1 }));
          }
        } catch (err) {
          console.error('增加阅读数失败:', err);
        }

        // 获取评论列表
        const token = getAuthToken();
        if (token) {
          try {
            const commentsData = await medicalApi.getPostComments(postId);
            setComments(Array.isArray(commentsData) ? commentsData : []);
          } catch (err) {
            console.error('获取评论失败:', err);
            setComments([]);
          }
        }

        // 初始化微信分享
        if (isWeChat() && postData) {
          initShare(postData);
        }
      } catch (err) {
        setError(err.message ?? t('load-data-failed'));
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [postId]);

  // 初始化微信分享
  const initShare = async (postData) => {
    if (shareInitialized) return;

    try {
      const currentUrl = window.location.href.split('#')[0]; // 获取当前页面URL（去掉hash）
      
      const success = await initWeChatShare(
        async () => {
          // 获取微信签名
          const signature = await medicalApi.getWeChatSignature(currentUrl);
          return signature;
        },
        {
          title: postData.title || '医疗咨询系统',
          desc: postData.content ? (postData.content.length > 100 ? postData.content.substring(0, 100) + '...' : postData.content) : '查看详情',
          link: currentUrl,
          imgUrl: postData.authorAvatarUrl 
            ? buildImageUrl(postData.authorAvatarUrl) 
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
  const handleShare = () => {
    if (isWeChat()) {
      // 在微信中，分享功能由微信 JS-SDK 自动处理
      // 用户点击右上角分享按钮时会触发
      alert('请点击右上角菜单按钮进行分享');
    } else {
      // 非微信环境，可以复制链接
      const url = window.location.href;
      if (navigator.clipboard) {
        navigator.clipboard.writeText(url).then(() => {
          alert('链接已复制到剪贴板');
        }).catch(() => {
          // 降级方案
          copyToClipboard(url);
        });
      } else {
        copyToClipboard(url);
      }
    }
  };

  // 复制到剪贴板的降级方案
  const copyToClipboard = (text) => {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.opacity = '0';
    document.body.appendChild(textArea);
    textArea.select();
    try {
      document.execCommand('copy');
      alert('链接已复制到剪贴板');
    } catch (err) {
      alert('复制失败，请手动复制链接：' + text);
    }
    document.body.removeChild(textArea);
  };

  const formatTime = (date) => {
    if (!date) return '';
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day} ${hours}:${minutes}`;
  };

  const handleLike = async () => {
    const token = getAuthToken();
    if (!token) {
      navigate('/login', { state: { from: `/post-detail/${postId}` } });
      return;
    }

    try {
      const result = await medicalApi.togglePostLike(postId);
      setIsLiked(result.isLiked);
      setLikeCount(result.likeCount || 0);
    } catch (err) {
      console.error('点赞失败:', err);
    }
  };

  const handleComment = async () => {
    const token = getAuthToken();
    if (!token) {
      navigate('/login', { state: { from: `/post-detail/${postId}` } });
      return;
    }

    if (!commentText.trim()) {
      return;
    }

    setSubmitting(true);
    try {
      const newComment = await medicalApi.createPostComment(postId, {
        content: commentText.trim(),
        attachmentUrls: null
      });
      
      setComments(prev => [...prev, newComment]);
      setCommentText('');
      
      // 更新帖子评论数
      if (post) {
        setPost(prev => ({
          ...prev,
          commentCount: (prev.commentCount || 0) + 1,
          lastReplyAt: new Date().toISOString()
        }));
      }
    } catch (err) {
      console.error('评论失败:', err);
      alert(err.message || t('comment-failed'));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">{t('loading')}</div>
      </PageLayout>
    );
  }

  if (error || !post) {
    return (
      <PageLayout>
        <div className="error-tip">{error || t('post-not-exist')}</div>
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
        <h1 className="header-title">{t('post-detail')}</h1>
      </div>

      {/* 帖子内容 */}
      <div className="post-content">
        <div className="post-title">{post.title}</div>
        
        <div className="post-author-info">
          <div className="author-avatar">
            <img 
              src={buildImageUrl(post.authorAvatarUrl, '/Img/Director.png')} 
              alt={t('avatar')}
              onError={(e) => {
                e.target.src = '/Img/Director.png';
              }}
            />
          </div>
          <div className="author-details">
            <div className="author-name">{post.authorDisplayName}</div>
            <div className="post-meta">
              {formatTime(post.createdAt)} {t('views')} {post.readCount}
            </div>
          </div>
        </div>

        <div className="post-body">{post.content}</div>

        {/* 附件显示 */}
        {post.attachmentUrls && post.attachmentUrls.length > 0 && (
          <div className="post-attachments">
            {post.attachmentUrls.map((url, index) => (
              <img 
                key={index}
                src={buildImageUrl(url)} 
                alt={`附件${index + 1}`}
                className="attachment-image"
              />
            ))}
          </div>
        )}
      </div>

      {/* 评论区域 */}
      <div className="comments-section">
        <div className="comments-header">
          {t('all-replies')}({post.commentCount || comments.length})
        </div>

        {comments.length === 0 ? (
          <div className="no-comments">
            <div className="empty-icon">😊</div>
            <div className="empty-text">{t('no-replies')}</div>
            <div className="empty-hint">{t('click-to-comment')}</div>
          </div>
        ) : (
          <div className="comments-list">
            {comments.map((comment) => (
              <div key={comment.id} className="comment-item">
                <div className="comment-author">
                  <div className="comment-avatar">
                    <img 
                      src={buildImageUrl(comment.authorAvatarUrl, '/Img/Director.png')} 
                      alt={t('avatar')}
                      onError={(e) => {
                        e.target.src = '/Img/Director.png';
                      }}
                    />
                  </div>
                  <div className="comment-info">
                    <div className="comment-author-name">{comment.authorDisplayName}</div>
                    <div className="comment-time">{formatTime(comment.createdAt)}</div>
                  </div>
                </div>
                <div className="comment-content">{comment.content}</div>
                
                {/* 回复列表 */}
                {comment.replies && comment.replies.length > 0 && (
                  <div className="replies-list">
                    {comment.replies.map((reply) => (
                      <div key={reply.id} className="reply-item">
                        <div className="reply-author">
                          <div className="reply-avatar">
                            <img 
                              src={buildImageUrl(reply.authorAvatarUrl, '/Img/Director.png')} 
                              alt={t('avatar')}
                              onError={(e) => {
                                e.target.src = '/Img/Director.png';
                              }}
                            />
                          </div>
                          <div className="reply-info">
                            <div className="reply-author-name">{reply.authorDisplayName}</div>
                            <div className="reply-time">{formatTime(reply.createdAt)}</div>
                          </div>
                        </div>
                        <div className="reply-content">{reply.content}</div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* 底部操作栏 */}
      <div className="bottom-actions">
        <input
          type="text"
          className="comment-input"
          placeholder={t('say-something')}
          value={commentText}
          onChange={(e) => setCommentText(e.target.value)}
          onKeyPress={(e) => {
            if (e.key === 'Enter' && !submitting) {
              handleComment();
            }
          }}
        />
        <div className="action-buttons">
          <button 
            className="action-btn comment-icon-btn"
            onClick={handleComment}
            disabled={submitting}
          >
            💬
          </button>
          <button 
            className={`action-btn like-btn ${isLiked ? 'liked' : ''}`}
            onClick={handleLike}
          >
            👍 {likeCount > 0 && <span className="like-count">{likeCount}</span>}
          </button>
          <button 
            className="action-btn share-btn"
            onClick={handleShare}
          >
            🔗
          </button>
        </div>
      </div>
    </PageLayout>
  );
}

