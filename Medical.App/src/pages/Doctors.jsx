import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Doctors() {
  usePageStyles('doctors.css');
  const { t } = useLanguage();

  const [departments, setDepartments] = useState([]);
  const [activeDepartmentId, setActiveDepartmentId] = useState('');
  const [keyword, setKeyword] = useState('');
  const [keywordInput, setKeywordInput] = useState('');
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadInitialData = async () => {
      try {
        const deptData = await medicalApi.getDepartments();
        // 后端返回的是分页格式 { items, total, page, pageSize }，需要提取 items
        const departmentsList = deptData?.items ?? deptData ?? [];
        setDepartments(departmentsList);
        if (departmentsList.length > 0) {
          setActiveDepartmentId(departmentsList[0].id);
        }
      } catch (err) {
        setError(err.message ?? t('cannot-load-departments'));
      }
    };
    loadInitialData();
  }, []);

  useEffect(() => {
    const loadDoctors = async () => {
      setLoading(true);
      setError('');
      try {
        const params = activeDepartmentId ? { departmentId: activeDepartmentId } : {};
        const data = keyword
          ? await medicalApi.searchDoctors(keyword)
          : await medicalApi.getDoctors({
              ...params,
              pageSize: 20
            });
        // 后端返回的是分页格式 { items, total, page, pageSize }，需要提取 items
        const doctorsList = data?.items ?? data ?? [];
        setDoctors(Array.isArray(doctorsList) ? doctorsList : []);
      } catch (err) {
        setError(err.message ?? t('cannot-load-doctors'));
      } finally {
        setLoading(false);
      }
    };
    loadDoctors();
  }, [activeDepartmentId, keyword]);

  const activeDepartmentName = useMemo(() => {
    if (!activeDepartmentId) return t('hot-departments');
    return departments.find((dept) => dept.id === activeDepartmentId)?.name ?? t('hot-departments');
  }, [departments, activeDepartmentId, t]);

  return (
    <PageLayout>
      {/* 搜索栏：支持输入医生 / 科室关键字 */}
      <div className="navbar">
        <Link to="/" className="back-arrow-link">
          <img src="/Img/return.png" alt="返回" className="back-arrow" />
        </Link>
        <div className="search-bar">
          <img src="/Img/search.png" alt="搜索" className="search-icon" />
          <input
            type="text"
            placeholder={t('search-department-doctor')}
            value={keywordInput}
            onChange={(event) => setKeywordInput(event.target.value)}
            onKeyPress={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
                setKeyword(keywordInput.trim());
              }
            }}
          />
          <div className="search-divider"></div>
          <button
            className="search-btn"
            type="button"
            onClick={() => setKeyword(keywordInput.trim())}
          >
            {t('search')}
          </button>
        </div>
      </div>

      {/* 科室标签：点击后触发筛选 */}
      <div className="department-tabs">
        {(departments.length > 0 ? departments : [{ id: '', name: t('hot-departments') }]).map((dept) => (
          <button
            type="button"
            key={dept.id || 'hot'}
            className={`tab-item${activeDepartmentId === dept.id ? ' active' : ''}`}
            onClick={() => {
              setKeyword('');
              setKeywordInput('');
              setActiveDepartmentId(dept.id ?? '');
            }}
          >
            {dept.name ?? t('hot-departments')}
          </button>
        ))}
      </div>

      {/* 错误提示信息 */}
      {error && <div className="error-tip">{error}</div>}

      {/* 医生列表：展示来自后端的数据 */}
      <div className="doctor-list">
        {loading && <div className="loading">{t('loading')}</div>}
        {!loading && doctors.length === 0 && (
          <div className="empty">{t('no-doctors-data')}</div>
        )}
        {!loading &&
          doctors.map((doctor) => (
            <div className="doctor-card" key={doctor.id}>
              <div className="doctor-avatar">
                <img
                  src={buildImageUrl(doctor.avatarUrl, '/Img/07-专家在线/1.png')}
                  alt={doctor.name}
                  className="avatar-img"
                  onError={(e) => {
                    // 如果图片加载失败，使用默认图片
                    e.target.src = '/Img/07-专家在线/1.png';
                  }}
                />
              </div>
              <div className="doctor-info">
                <div className="doctor-header">
                  <span className="doctor-name">{doctor.name}</span>
                  <span className="doctor-tag">{doctor.title}</span>
                </div>
                <div className="doctor-department">{doctor.departmentName ?? activeDepartmentName}</div>
                <div className="doctor-specialty">{doctor.specialty ?? '擅长信息暂缺'}</div>
                <div className="doctor-stats">
                  <span className="doctor-rating">
                    <span className="rating-icon">😊</span>
                    <span className="rating-score">{doctor.rating ?? '5.0'}</span>
                  </span>
                  <span className="rating-divider">|</span>
                  <span className="consultation-count">
                    {doctor.consultationCount ?? 0}人已咨询
                  </span>
                </div>
              </div>
              <Link to={`/doctor/${doctor.id}`} className="consult-btn">
                {t('consult')}
              </Link>
            </div>
          ))}
      </div>
    </PageLayout>
  );
}
