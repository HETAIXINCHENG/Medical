import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage, SUPPORTED_LANGUAGES } from '../contexts/LanguageContext.jsx';

export default function LanguageSettings() {
  usePageStyles('language-settings.css');
  const navigate = useNavigate();
  const { language, setLanguage, t } = useLanguage();
  const [selectedLanguage, setSelectedLanguage] = useState(language);
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    setHasChanges(selectedLanguage !== language);
  }, [selectedLanguage, language]);

  const handleSave = () => {
    setLanguage(selectedLanguage);
    setHasChanges(false);
    // 可以显示保存成功的提示
    setTimeout(() => {
      navigate(-1);
    }, 300);
  };

  const handleSelectLanguage = (langCode) => {
    setSelectedLanguage(langCode);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="language-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">{t('language-title')}</h1>
        <button 
          className={`save-btn ${hasChanges ? 'active' : ''}`}
          onClick={handleSave}
          disabled={!hasChanges}
        >
          {t('save')}
        </button>
      </div>

      {/* 内容区域 */}
      <div className="language-content">
        <div className="language-list">
          {Object.values(SUPPORTED_LANGUAGES).map((lang) => (
            <div
              key={lang.code}
              className={`language-item ${selectedLanguage === lang.code ? 'selected' : ''}`}
              onClick={() => handleSelectLanguage(lang.code)}
            >
              <span className="language-name">{lang.nativeName}</span>
              {selectedLanguage === lang.code && (
                <span className="check-icon">✓</span>
              )}
            </div>
          ))}
        </div>
      </div>
    </PageLayout>
  );
}

