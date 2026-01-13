import { useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { getAuthToken } from '../config/apiConfig.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function CreatePost() {
  usePageStyles('create-post.css');
  const { groupId } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [tag, setTag] = useState('求助');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const fileInputRef = useRef(null);
  const [attachments, setAttachments] = useState([]); // {url, file, type}

  const handleSubmit = async () => {
    if (!title.trim()) {
      setError(t('please-enter-title'));
      return;
    }
    if (!content.trim()) {
      setError(t('please-enter-content'));
      return;
    }
    if (content.length > 1000) {
      setError(t('content-too-long-1000'));
      return;
    }

    const token = getAuthToken();
    if (!token) {
      navigate('/login', { state: { from: `/create-post/${groupId}` } });
      return;
    }

    setLoading(true);
    setError('');
    try {
      // 先上传所有附件
      const attachmentUrls = [];
      for (const attachment of attachments) {
        try {
          const result = await medicalApi.uploadPostAttachment(attachment.file);
          if (result?.url) {
            attachmentUrls.push(result.url);
          }
        } catch (err) {
          console.error('上传附件失败:', err);
          setError(`${t('upload')}${attachment.type === 'image' ? t('image') : t('video')}${t('failed')}: ${err.message}`);
          return;
        }
      }

      // 提交帖子
      await medicalApi.createPost(groupId, {
        title: title.trim(),
        content: content.trim(),
        tag,
        attachmentUrls: attachmentUrls.length > 0 ? attachmentUrls : undefined
      });
      navigate(-1); // 返回上一页
    } catch (err) {
      setError(err.message ?? t('publish-failed'));
    } finally {
      setLoading(false);
    }
  };

  const handleUploadClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleFilesChange = (e) => {
    const files = Array.from(e.target.files || []);
    if (!files.length) return;
    const newItems = files.map((file) => {
      const url = URL.createObjectURL(file);
      const type = file.type.startsWith('video') ? 'video' : 'image';
      return { url, file, type };
    });
    setAttachments((prev) => [...prev, ...newItems]);
    // 重置 input 以便可重复选择同一文件
    e.target.value = '';
  };

  const handlePreviewClick = (item) => {
    window.open(item.url, '_blank');
  };

  return (
    <PageLayout>
      {/* 头部 */}
      <div className="header">
        <button className="header-btn cancel-btn" onClick={() => navigate(-1)}>
          {t('cancel')}
        </button>
        <h1 className="header-title">{t('create-post')}</h1>
        <button
          className="header-btn submit-btn"
          onClick={handleSubmit}
          disabled={loading}
        >
          {loading ? t('publishing') : t('publish')}
        </button>
      </div>

      {/* 内容 */}
      <div className="content">
        {error && <div className="error-tip">{error}</div>}
        
        <input
          type="text"
          className="title-input"
          placeholder={t('post-title-placeholder')}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          maxLength={200}
        />

        <textarea
          className="content-input"
          placeholder={t('post-content-placeholder-create')}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          maxLength={1000}
        />

        <div className="char-count">{content.length}/1000{t('characters')}</div>

        <div className="actions">
          <div className="action-item">
            <div className="upload-box" onClick={handleUploadClick}>
              <span className="upload-icon">+</span>
              <span className="upload-text">{t('select-image-video')}</span>
            </div>
            <input
              type="file"
              ref={fileInputRef}
              accept="image/*,video/*"
              multiple
              style={{ display: 'none' }}
              onChange={handleFilesChange}
            />
          </div>
        </div>

        {attachments.length > 0 && (
          <div className="upload-preview">
            {attachments.map((item, idx) => (
              <div
                className="preview-item"
                key={`${item.url}-${idx}`}
              >
                <button
                  className="preview-delete"
                  onClick={(e) => {
                    e.stopPropagation();
                    setAttachments((prev) => prev.filter((_, i) => i !== idx));
                  }}
                >
                  ×
                </button>
                <div className="preview-touch" onClick={() => handlePreviewClick(item)}>
                  {item.type === 'image' ? (
                    <img src={item.url} alt={t('preview')} className="preview-thumb" />
                  ) : (
                    <video src={item.url} className="preview-thumb" />
                  )}
                  <div className="preview-type">{item.type === 'image' ? t('image') : t('video')}</div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </PageLayout>
  );
}


