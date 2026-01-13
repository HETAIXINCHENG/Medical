import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Information() {
  usePageStyles('information.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [hasMore, setHasMore] = useState(true);
  const loaderRef = useRef(null);

  // 拉取一页医生并过滤出有已启用患友会的医生
  const loadDoctors = async (nextPage = 1, append = false) => {
    if (!append) setLoading(true);
    else setLoadingMore(true);
    setError('');
    try {
      const data = await medicalApi.getDoctors({ page: nextPage, pageSize });
      const doctorsList = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
      const total = data?.total ?? doctorsList.length;

      // 优化：完全并行处理所有请求，不再使用串行批次
      // 所有请求同时发送，总时间 = 最慢的那个请求的时间
      const checkPromises = doctorsList.map(async (doc) => {
        try {
          await medicalApi.getPatientSupportGroupByDoctor(doc.id);
          return { doc, hasGroup: true };
        } catch {
          return { doc, hasGroup: false };
        }
      });

      // 使用 Promise.allSettled 并行处理所有请求
      const results = await Promise.allSettled(checkPromises);

      const enabledDoctors = results
        .filter(r => r.status === 'fulfilled' && r.value.hasGroup)
        .map(r => r.value.doc);

      setDoctors((prev) => {
        const merged = append ? [...prev, ...enabledDoctors] : enabledDoctors;
        const map = new Map();
        merged.forEach(d => map.set(d.id, d));
        return Array.from(map.values());
      });

      const loaded = (append ? page : 0) * pageSize + enabledDoctors.length;
      setHasMore(nextPage * pageSize < total && enabledDoctors.length > 0);
      setPage(nextPage);
    } catch (err) {
      setError(err.message ?? t('load-doctors-failed'));
      if (!append) setDoctors([]);
      setHasMore(false);
    } finally {
      if (!append) setLoading(false);
      setLoadingMore(false);
    }
  };

  // 首次加载
  useEffect(() => {
    loadDoctors(1, false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // 触底加载更多
  useEffect(() => {
    if (!loaderRef.current) return;
    const observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting && hasMore && !loadingMore) {
        loadDoctors(page + 1, true);
      }
    }, { threshold: 0.1 });

    observer.observe(loaderRef.current);
    return () => observer.disconnect();
  }, [hasMore, loadingMore, page]);

  // 格式化粉丝数显示
  const formatFollowerCount = (count) => {
    if (count >= 10000) {
      return `${(count / 10000).toFixed(1)}${t('ten-thousand')}${t('fans')}`;
    }
    return `${count}${t('fans')}`;
  };

  // 格式化阅读数显示
  const formatReadCount = (count) => {
    return `${count}${t('reads')}`;
  };

  return (
    <PageLayout>
      {/* 页面标题 */}
      <div className="header">
        <h1 className="page-title">{t('doctors-community')}</h1>
      </div>

      {error && <div className="error-tip">{error}</div>}

      {/* 医生列表 */}
      <div className="doctors-list">
        {!loading && !error && doctors.length === 0 && (
          <div className="empty">{t('no-doctors')}</div>
        )}
        {!loading && !error && doctors.map((doctor) => (
          <div
            key={doctor.id}
            className="doctor-card"
            onClick={() => navigate(`/patient-support-group/${doctor.id}`)}
          >
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
                <span className="doctor-name">{doctor.name}{t('doctor-community')}</span>
              </div>
              <div className="doctor-hospital">
                {doctor.hospital} {doctor.departmentName}
              </div>
              <div className="doctor-stats">
                <span className="follower-count">{formatFollowerCount(doctor.followerCount ?? 0)}</span>
                <span className="read-count">{formatReadCount(doctor.totalReadCount ?? 0)}</span>
              </div>
            </div>
            <div className="arrow-icon">→</div>
          </div>
        ))}
        <div ref={loaderRef} style={{ padding: '16px 0', textAlign: 'center', color: '#999' }}>
          {loadingMore && t('load-more')}
          {!hasMore && !loadingMore && doctors.length > 0 && t('no-more')}
        </div>
      </div>
    </PageLayout>
  );
}
