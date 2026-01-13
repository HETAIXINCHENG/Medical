import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function GroupRules() {
  usePageStyles('group-rules.css');
  const { groupId } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [rules, setRules] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadRules = async () => {
      if (!groupId) return;
      
      setLoading(true);
      setError('');
      try {
        const data = await medicalApi.getGroupRules(groupId);
        setRules(data);
      } catch (err) {
        setError(err.message ?? t('load-rules-failed'));
      } finally {
        setLoading(false);
      }
    };

    loadRules();
  }, [groupId]);

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">{t('loading')}</div>
      </PageLayout>
    );
  }

  if (error || !rules) {
    return (
      <PageLayout>
        <div className="error-tip">{error || t('rules-not-exist')}</div>
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
        <h1 className="header-title">{t('group-rules-title')}</h1>
      </div>

      {/* 内容 */}
      <div className="content">
        <div className="rules-title">{rules.groupName}{t('group-rules')}</div>
        
        <div className="doctor-info">
          <img
            src={buildImageUrl(rules.doctorAvatarUrl, '/Img/Director.png')}
            alt={rules.doctorName}
            className="doctor-avatar"
            onError={(e) => {
              e.target.src = '/Img/Director.png';
            }}
          />
          <div className="doctor-details">
            <div className="doctor-name">{rules.doctorName}</div>
            <div className="doctor-title">{rules.doctorTitle}</div>
            <div className="doctor-hospital">
              {rules.doctorHospital} {rules.doctorDepartmentName}
            </div>
          </div>
        </div>

        {rules.groupDescription && (
          <div className="group-description">
            {rules.groupDescription}
          </div>
        )}

        <div className="rules-content">
          {rules.content.split('\n').map((line, index) => (
            <p key={index} className="rules-text">
              {line}
            </p>
          ))}
        </div>
      </div>
    </PageLayout>
  );
}

