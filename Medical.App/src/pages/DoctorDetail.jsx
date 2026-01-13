import { useEffect, useMemo, useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function DoctorDetail() {
  usePageStyles('doctor-detail.css');
  const { doctorId } = useParams();
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [doctor, setDoctor] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedConsultationType, setSelectedConsultationType] = useState(null);
  const [phonePrice, setPhonePrice] = useState(388); // 默认价格
  const [textPrice, setTextPrice] = useState(300); // 默认价格

  useEffect(() => {
    const loadDoctor = async () => {
      setLoading(true);
      setError('');
      try {
        const detail = await medicalApi.getDoctorById(doctorId);
        setDoctor(detail);
        // 设置咨询价格（如果后端返回了价格）
        if (detail.phonePrice != null) {
          setPhonePrice(detail.phonePrice);
        }
        if (detail.textPrice != null) {
          setTextPrice(detail.textPrice);
        }
      } catch (err) {
        setError(err.message ?? t('cannot-load-doctor-detail'));
      } finally {
        setLoading(false);
      }
    };
    loadDoctor();
  }, [doctorId]);

  const scheduleByDay = useMemo(() => {
    if (!doctor?.schedules) return [];
    const group = doctor.schedules.reduce((acc, item) => {
      const day = item.dayOfWeek ?? '未知';
      acc[day] = acc[day] ?? [];
      acc[day].push(item);
      return acc;
    }, {});
    return Object.entries(group);
  }, [doctor]);

  return (
    <PageLayout>
      {/* 顶部导航：返回列表及快捷按钮 */}
      <div className="navbar">
        <Link to="/doctors" className="back-arrow-link">
          <img src="/Img/return.png" alt="返回" className="back-arrow" />
        </Link>
      </div>

      {/* 数据加载过程中给出提示 */}
      {loading && <div className="loading">{t('doctor-info-loading')}</div>}
      {error && <div className="error-tip">{error}</div>}

      {doctor && !loading && (
        <>
          {/* 医生资料区：头像、名称、擅长方向 */}
          <div className="doctor-profile">
            <div className="doctor-avatar">
              <img
                src={buildImageUrl(doctor.avatarUrl, '/Img/08-专家主页详情/1.png')}
                alt={doctor.name}
                className="avatar-img"
                onError={(e) => {
                  // 如果图片加载失败，使用默认图片
                  e.target.src = '/Img/08-专家主页详情/1.png';
                }}
              />
            </div>
            <div className="doctor-info">
              <div className="doctor-name">{doctor.name}</div>
              <div className="doctor-title">
                {doctor.title} · {doctor.departmentName}
              </div>
              <div className="doctor-specialty">{doctor.specialty ?? t('specialty-info-missing')}</div>
            </div>
          </div>

          {/* 咨询方式选项 */}
          <div className="consultation-section">
            <div className="consultation-header">
              <h2 className="consultation-title">{t('online-consultation')}</h2>
              <div className="waiting-time">
                {t('general-waiting-time')} <span className="time-highlight">2{t('hours')}</span>
              </div>
            </div>
            <div className="consultation-notice">
              {t('consultation-notice')}
            </div>
            <div className="consultation-services">
              <div className="service-option">
                <input 
                  type="radio" 
                  name="consultation-type" 
                  id="phone-consult" 
                  className="service-radio"
                  checked={selectedConsultationType === 'phone'}
                  onChange={() => setSelectedConsultationType('phone')}
                />
                <label htmlFor="phone-consult" className="service-label">
                  <div className="service-header">
                    <span className="service-name">{t('phone-consultation')}</span>
                    <span className="service-price">{phonePrice}{t('yuan')}{t('per')}10{t('minutes')}</span>
                  </div>
                  <ul className="service-features">
                    <li>{t('phone-consult-feature1')}</li>
                    <li>{t('phone-consult-feature2')}</li>
                  </ul>
                  <Link to={`/service-description?type=phone`} className="service-link">{t('service-description')} &gt;</Link>
                </label>
              </div>
              <div className="service-option">
                <input 
                  type="radio" 
                  name="consultation-type" 
                  id="text-consult" 
                  className="service-radio"
                  checked={selectedConsultationType === 'text'}
                  onChange={() => setSelectedConsultationType('text')}
                />
                <label htmlFor="text-consult" className="service-label">
                  <div className="service-header">
                    <span className="service-name">{t('text-consultation')}</span>
                    <span className="service-price">{textPrice}{t('yuan')}{t('per')}2{t('days')}</span>
                  </div>
                  <ul className="service-features">
                    <li>{t('text-consult-feature1')}</li>
                    <li>{t('text-consult-feature2')}</li>
                  </ul>
                  <Link to={`/service-description?type=text`} className="service-link">{t('service-description')} &gt;</Link>
                </label>
              </div>
            </div>
          </div>

          {/* 个人简介 */}
          <div className="schedule-section">
            <div className="section-header">
              <h2 className="section-title">{t('personal-introduction')}</h2>
            </div>
            <div className="introduction-content">
              {doctor.introduction ? (
                <div className="introduction-text">{doctor.introduction}</div>
              ) : (
                <div className="empty">{t('no-personal-introduction')}</div>
              )}
            </div>
          </div>

          {/* 立即申请按钮 */}
          <div className="apply-section">
            <button 
              className="apply-btn"
              onClick={() => {
                if (!selectedConsultationType) {
                  alert(t('please-select-consultation-type'));
                  return;
                }
                // 检查是否已登录
                const token = localStorage.getItem('token');
                if (!token) {
                  // 未登录，跳转到登录页面，并保存当前路径
                  navigate('/login', { state: { from: `/warm-reminder?doctorId=${doctorId}&type=${selectedConsultationType}` } });
                  return;
                }
                navigate(`/warm-reminder?doctorId=${doctorId}&type=${selectedConsultationType}`);
              }}
            >
              {t('apply-now')}
            </button>
          </div>
        </>
      )}
    </PageLayout>
  );
}
