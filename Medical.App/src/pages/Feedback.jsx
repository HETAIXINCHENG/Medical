import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Feedback() {
  usePageStyles('feedback.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // 验证输入
    if (!title.trim()) {
      setError(t('title-required'));
      return;
    }
    if (!content.trim()) {
      setError(t('content-required'));
      return;
    }
    if (title.trim().length > 100) {
      setError(t('title-too-long'));
      return;
    }
    if (content.trim().length > 2000) {
      setError(t('content-too-long'));
      return;
    }

    setLoading(true);
    setError('');
    
    try {
      await medicalApi.submitFeedback({
        title: title.trim(),
        content: content.trim()
      });
      
      setSuccess(true);
      // 3秒后返回上一页
      setTimeout(() => {
        navigate(-1);
      }, 2000);
    } catch (err) {
      setError(err.message ?? t('feedback-submit-failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="feedback-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('feedback')}</h1>
      </div>

      {/* 内容区域 */}
      <div className="feedback-content">
        {success ? (
          <div className="success-message">
            <div className="success-icon">✓</div>
            <div className="success-text">{t('feedback-submit-success')}</div>
          </div>
        ) : (
          <form className="feedback-form" onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">{t('feedback-title')} <span className="required">*</span></label>
              <input
                type="text"
                className="form-input"
                placeholder={t('title-placeholder')}
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                maxLength={100}
                disabled={loading}
              />
              <div className="char-count">{title.length}/100</div>
            </div>

            <div className="form-group">
              <label className="form-label">{t('feedback-content')} <span className="required">*</span></label>
              <textarea
                className="form-textarea"
                placeholder={t('content-placeholder')}
                value={content}
                onChange={(e) => setContent(e.target.value)}
                maxLength={2000}
                rows={10}
                disabled={loading}
              />
              <div className="char-count">{content.length}/2000</div>
            </div>

            {error && (
              <div className="error-message">
                {error}
              </div>
            )}

            <div className="form-tips">
              <div className="tip-title">{t('warm-tips')}:</div>
              <div className="tip-item">• {t('tip-describe-detail')}</div>
              <div className="tip-item">• {t('tip-process-time')}</div>
              <div className="tip-item">• {t('tip-customer-service')}: 400-888-8888</div>
            </div>

            <button
              type="submit"
              className="submit-btn"
              disabled={loading || !title.trim() || !content.trim()}
            >
              {loading ? t('submitting') : t('submit-feedback')}
            </button>
          </form>
        )}
      </div>
    </PageLayout>
  );
}

