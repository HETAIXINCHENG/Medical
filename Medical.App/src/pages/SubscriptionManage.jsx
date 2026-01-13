import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import apiClient from '../services/apiClient.js';
import { getAuthToken } from '../config/apiConfig.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function SubscriptionManage() {
  usePageStyles('subscription-manage.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [departments, setDepartments] = useState([]);
  const [selectedDepartment, setSelectedDepartment] = useState(null);
  const [doctors, setDoctors] = useState([]);
  const [mySubscriptions, setMySubscriptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const token = getAuthToken();
    if (!token) {
      navigate('/login', { state: { from: '/subscription-manage' } });
      return;
    }

    const loadData = async () => {
      setLoading(true);
      setError('');
      try {
        // 加载科室列表
        const deptData = await apiClient.get('/api/departments', { params: { pageSize: 100 } });
        const deptList = deptData?.items ?? deptData ?? [];
        setDepartments(Array.isArray(deptList) ? deptList : []);
        
        // 加载用户订阅的医生列表
        const subData = await apiClient.get('/api/subscriptions/my-subscriptions');
        const subList = Array.isArray(subData) ? subData : [];
        setMySubscriptions(subList);
        
        // 默认不选择任何科室，显示所有医生
      } catch (err) {
        setError(err.message ?? t('load-data-failed'));
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [navigate]);

  useEffect(() => {
    const loadDoctors = async () => {
      setLoading(true);
      try {
        // 如果选择了科室，获取该科室下的医生；否则获取所有医生
        const params = selectedDepartment 
          ? { departmentId: selectedDepartment.id, pageSize: 1000 }
          : { pageSize: 1000 };
        
        const doctorData = await apiClient.get('/api/doctors', { params });
        const doctorList = doctorData?.items ?? doctorData ?? [];
        setDoctors(Array.isArray(doctorList) ? doctorList : []);
        
        // 重新加载订阅列表以确保状态最新
        try {
          const subData = await apiClient.get('/api/subscriptions/my-subscriptions');
          const subList = Array.isArray(subData) ? subData : [];
          setMySubscriptions(subList);
        } catch (err) {
          console.error('加载订阅列表失败:', err);
        }
      } catch (err) {
        console.error('加载医生列表失败:', err);
        setDoctors([]);
      } finally {
        setLoading(false);
      }
    };

    loadDoctors();
  }, [selectedDepartment]);

  const handleSubscribe = async (doctorId) => {
    try {
      await apiClient.post(`/api/subscriptions/${doctorId}`);
      // 重新加载订阅列表
      const subData = await apiClient.get('/api/subscriptions/my-subscriptions');
      const subList = Array.isArray(subData) ? subData : [];
      setMySubscriptions(subList);
    } catch (err) {
      alert(err.message ?? t('subscribe-failed'));
    }
  };

  const handleUnsubscribe = async (doctorId) => {
    try {
      await apiClient.del(`/api/subscriptions/${doctorId}`);
      // 重新加载订阅列表
      const subData = await apiClient.get('/api/subscriptions/my-subscriptions');
      const subList = Array.isArray(subData) ? subData : [];
      setMySubscriptions(subList);
    } catch (err) {
      alert(err.message ?? t('unsubscribe-failed'));
    }
  };

  const isSubscribed = (doctorId) => {
    return mySubscriptions.some(sub => sub.doctorId === doctorId);
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">{t('loading')}</div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      {/* 页面标题 */}
      <div className="header">
        <Link to="/discover" className="back-link">
          <span className="back-arrow">←</span>
        </Link>
        <h1 className="page-title">{t('subscription-doctor')}</h1>
      </div>

      {error && <div className="error-tip">{error}</div>}

      {/* 科室标签 */}
      <div className="department-tabs">
        <button
          type="button"
          className={`department-tab ${!selectedDepartment ? 'active' : ''}`}
          onClick={() => setSelectedDepartment(null)}
        >
          {t('all')}
        </button>
        {departments.map((dept) => (
          <button
            key={dept.id}
            type="button"
            className={`department-tab ${selectedDepartment?.id === dept.id ? 'active' : ''}`}
            onClick={() => setSelectedDepartment(dept)}
          >
            {dept.name}
          </button>
        ))}
      </div>

      {/* 医生列表 */}
      <div className="doctors-list">
        {loading && <div className="loading">{t('loading')}</div>}
        {!loading && doctors.length === 0 && (
          <div className="empty-state">
            <div className="empty-text">{t('no-doctors-in-department')}</div>
          </div>
        )}
        {!loading && doctors.map((doctor) => {
          const subscribed = isSubscribed(doctor.id);
          return (
            <div key={doctor.id} className="doctor-card">
              <div className="doctor-avatar-container">
                <img
                  src={buildImageUrl(doctor.avatarUrl, '/Img/Director.png')}
                  alt={doctor.name}
                  className="doctor-avatar"
                  onError={(e) => {
                    e.target.src = '/Img/Director.png';
                  }}
                />
              </div>
              <div className="doctor-info">
                <div className="doctor-header">
                  <span className="doctor-name">{doctor.name}</span>
                  <span className="doctor-title-badge">{doctor.title}</span>
                </div>
                <div className="doctor-specialty">{doctor.departmentName}</div>
                {doctor.specialty && (
                  <div className="doctor-expertise">{doctor.specialty}</div>
                )}
              </div>
              <button
                className={`subscribe-button ${subscribed ? 'subscribed' : ''}`}
                onClick={() => {
                  if (subscribed) {
                    handleUnsubscribe(doctor.id);
                  } else {
                    handleSubscribe(doctor.id);
                  }
                }}
              >
                {subscribed ? t('subscribed') : t('subscribe')}
              </button>
            </div>
          );
        })}
      </div>
    </PageLayout>
  );
}

