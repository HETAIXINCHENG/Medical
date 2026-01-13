import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';

export default function TcmSpecialtyRanking() {
  usePageStyles('tcm-specialty-ranking.css');
  const navigate = useNavigate();
  
  const [selectedDiseaseId, setSelectedDiseaseId] = useState(null);
  const [diseaseCategories, setDiseaseCategories] = useState([]);
  const [hospitals, setHospitals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showMoreModal, setShowMoreModal] = useState(false);
  const [visibleDiseases, setVisibleDiseases] = useState([]); // 前8个疾病，用于显示

  // 加载疾病分类
  const loadDiseaseCategories = useCallback(async () => {
    try {
      // 获取所有疾病分类（用于"更多"弹窗）
      const allResponse = await medicalApi.getDiseaseCategories({ 
        page: 1, 
        pageSize: 1000 
      });
      const allDiseases = Array.isArray(allResponse?.items) ? allResponse.items : [];
      console.log('所有疾病分类API响应:', allResponse);
      console.log('所有疾病分类:', allDiseases);
      setDiseaseCategories(allDiseases);
      
      // 获取前7个疾病分类（用于显示）
      const top7Response = await medicalApi.getDiseaseCategories({ 
        page: 1, 
        pageSize: 7 
      });
      const top7Diseases = Array.isArray(top7Response?.items) ? top7Response.items : [];
      console.log('前7个疾病分类API响应:', top7Response);
      console.log('前7个疾病分类:', top7Diseases);
      
      // 确保数据格式正确（处理可能的字段名大小写问题）
      const normalizedTop7 = top7Diseases.map(d => ({
        id: d.id || d.Id,
        name: d.name || d.Name,
        departmentId: d.departmentId || d.DepartmentId,
        departmentName: d.departmentName || d.DepartmentName
      }));
      
      setVisibleDiseases(normalizedTop7);
      
      // 默认选择第一个疾病
      if (normalizedTop7.length > 0 && !selectedDiseaseId) {
        setSelectedDiseaseId(normalizedTop7[0].id);
      }
    } catch (err) {
      console.error('加载疾病分类失败:', err);
      setError('加载疾病分类失败: ' + (err.message || '未知错误'));
    }
  }, [selectedDiseaseId]);

  // 加载医院列表
  const loadHospitals = useCallback(async () => {
    if (!selectedDiseaseId) return;

    setLoading(true);
    setError('');
    try {
      // 获取所有中医医院
      const response = await medicalApi.getTertiaryHospitals({ 
        page: 1, 
        pageSize: 1000 
      });
      
      const allHospitals = Array.isArray(response?.items) ? response.items : [];
      
      // 筛选中医医院
      const tcmHospitals = allHospitals.filter(h => 
        h.isEnabled && 
        h.type && (
          h.type.includes('中医') || 
          h.type.includes('中医院') ||
          h.type === '中医医院'
        )
      );

      // 按 SortOrder 排序
      const sortedHospitals = [...tcmHospitals].sort((a, b) => {
        if (a.sortOrder !== b.sortOrder) {
          return (a.sortOrder || 0) - (b.sortOrder || 0);
        }
        return (a.name || '').localeCompare(b.name || '', 'zh-CN');
      });

      setHospitals(sortedHospitals);
    } catch (err) {
      console.error('加载医院列表失败:', err);
      setError(err.message || '加载医院列表失败');
    } finally {
      setLoading(false);
    }
  }, [selectedDiseaseId]);

  useEffect(() => {
    loadDiseaseCategories();
  }, [loadDiseaseCategories]);

  useEffect(() => {
    loadHospitals();
  }, [loadHospitals]);

  const handleDiseaseSelect = (diseaseId) => {
    setSelectedDiseaseId(diseaseId);
    setShowMoreModal(false);
  };

  return (
    <PageLayout>
      <div className="tcm-specialty-ranking-page">
        {/* 头部 */}
        <div className="ranking-header">
          <div className="header-title">中医院专科学术影响力榜</div>
          <div className="header-subtitle">
            由中华中医药学会联合中国中医科学院发布,采用5年数据,对610家公立三级中医医院各学科的客观评价。
          </div>
        </div>

        {/* 疾病筛选 */}
        <div className="disease-filter">
          <div className="disease-grid">
            {visibleDiseases.length > 0 ? (
              visibleDiseases.map((disease) => (
                <button
                  key={disease.id || disease.Id}
                  className={`disease-btn ${selectedDiseaseId === (disease.id || disease.Id) ? 'active' : ''}`}
                  onClick={() => handleDiseaseSelect(disease.id || disease.Id)}
                >
                  {disease.name || disease.Name}
                </button>
              ))
            ) : (
              <div style={{ color: 'white', padding: '10px', textAlign: 'center', gridColumn: '1 / -1' }}>
                加载中...
              </div>
            )}
            <button
              className="disease-btn more"
              onClick={() => setShowMoreModal(true)}
            >
              更多
            </button>
          </div>
        </div>

        {/* 更多疾病弹窗 */}
        {showMoreModal && (
          <div className="more-modal-overlay" onClick={() => setShowMoreModal(false)}>
            <div className="more-modal-content" onClick={(e) => e.stopPropagation()}>
              <div className="more-modal-header">
                <button 
                  className="more-modal-cancel"
                  onClick={() => setShowMoreModal(false)}
                >
                  取消
                </button>
              </div>
              <div className="more-modal-list">
                {diseaseCategories.map((disease) => {
                  const diseaseId = disease.id || disease.Id;
                  const diseaseName = disease.name || disease.Name;
                  return (
                    <button
                      key={diseaseId}
                      className={`more-modal-item ${selectedDiseaseId === diseaseId ? 'active' : ''}`}
                      onClick={() => handleDiseaseSelect(diseaseId)}
                    >
                      {diseaseName}
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        )}

        {/* 医院列表 */}
        {loading ? (
          <div className="loading-container">
            <div className="loading-text">加载中...</div>
          </div>
        ) : error ? (
          <div className="error-container">
            <div className="error-text">{error}</div>
          </div>
        ) : hospitals.length === 0 ? (
          <div className="empty-container">
            <div className="empty-text">暂无医院数据</div>
          </div>
        ) : (
          <div className="hospital-list">
            {hospitals.map((hospital, index) => (
              <div key={hospital.id} className="hospital-card">
                {/* TOP 徽章 */}
                <div className={`top-badge top-${index + 1}`}>
                  TOP {index + 1}
                </div>
                
                {/* 医院信息 */}
                <div className="hospital-info">
                  <div className="hospital-name-row">
                    <h3 className="hospital-name">{hospital.name}</h3>
                  </div>
                  
                  {/* 标签 */}
                  <div className="hospital-tags">
                    <span className="tag tag-public">公立</span>
                    <span className="tag tag-level">{hospital.level || '三甲'}</span>
                    <span className="tag tag-type">{hospital.type || '中医医院'}</span>
                  </div>
                  
                  {/* 重点专科 */}
                  <div className="hospital-specialties">
                    国家重点专科:外科、妇科
                  </div>
                  
                  {/* 电话 */}
                  {hospital.phone && (
                    <div className="hospital-phone">
                      电话: {hospital.phone}
                    </div>
                  )}
                  
                  {/* 地址 */}
                  {hospital.address && (
                    <div className="hospital-address">
                      {hospital.provinceName} {hospital.cityName} {hospital.address}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </PageLayout>
  );
}

