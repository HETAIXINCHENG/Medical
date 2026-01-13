import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function MyDoctors() {
  usePageStyles('my-doctors.css');
  const { t } = useLanguage();
  const navigate = useNavigate();
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadDoctors = async () => {
      setLoading(true);
      setError('');
      try {
        const data = await medicalApi.getMyConsultedDoctors();
        setDoctors(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error('加载我的医生列表失败', err);
        setError(err.message ?? '加载失败，请稍后重试');
        setDoctors([]);
      } finally {
        setLoading(false);
      }
    };
    loadDoctors();
  }, []);

  const handleDoctorClick = (doctorId) => {
    navigate(`/doctor/${doctorId}`);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">我的医生</h1>
      </div>

      {/* 内容区域 */}
      <div className="content-area">
        {loading ? (
          <div className="loading">加载中...</div>
        ) : error ? (
          <div className="error">{error}</div>
        ) : doctors.length === 0 ? (
          <div className="empty">暂无咨询过的医生</div>
        ) : (
          <div className="doctor-list">
            {doctors.map((doctor) => (
              <div
                key={doctor.id}
                className="doctor-card"
                onClick={() => handleDoctorClick(doctor.id)}
              >
                <div className="doctor-avatar">
                  <img
                    src={buildImageUrl(doctor.avatarUrl, '/Img/Director.png')}
                    alt={doctor.name}
                    onError={(e) => {
                      e.target.src = '/Img/Director.png';
                    }}
                  />
                </div>
                <div className="doctor-info">
                  <div className="doctor-name-row">
                    <span className="doctor-name">{doctor.name}</span>
                    {doctor.title && (
                      <span className="doctor-title">{doctor.title}</span>
                    )}
                  </div>
                  <div className="doctor-meta">
                    {doctor.department?.name && (
                      <span className="doctor-department">
                        {doctor.department.name}
                      </span>
                    )}
                    {doctor.hospital && (
                      <span className="doctor-hospital">{doctor.hospital}</span>
                    )}
                  </div>
                  {doctor.specialty && (
                    <div className="doctor-specialty">{doctor.specialty}</div>
                  )}
                  <div className="doctor-stats">
                    <span className="consultation-count">
                      咨询次数: {doctor.consultationCount || 0}
                    </span>
                    {doctor.rating > 0 && (
                      <span className="doctor-rating">
                        评分: {doctor.rating.toFixed(1)}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </PageLayout>
  );
}

