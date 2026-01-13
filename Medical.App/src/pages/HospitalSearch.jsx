import { useEffect, useState, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';

export default function HospitalSearch() {
  usePageStyles('hospital-search.css');
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  
  const [selectedProvince, setSelectedProvince] = useState(null);
  const [provinces, setProvinces] = useState([]);
  const [hospitals, setHospitals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filterType, setFilterType] = useState('nearest'); // 'nearest', 'online', 'appointment', 'tertiary'
  const [showProvinceDropdown, setShowProvinceDropdown] = useState(false);

  // 初始化：从URL参数或IP定位获取省份
  useEffect(() => {
    const initProvince = async () => {
      // 优先从URL参数获取省份
      const provinceFromUrl = searchParams.get('province');
      if (provinceFromUrl) {
        // 加载省份列表并找到对应的省份
        try {
          const provincesData = await medicalApi.getProvinces();
          const provincesList = Array.isArray(provincesData) ? provincesData : provincesData?.items ?? [];
          const matchedProvince = provincesList.find(p => 
            p.name === provinceFromUrl || 
            p.name === provinceFromUrl + '省' ||
            p.name === provinceFromUrl + '市'
          );
          if (matchedProvince) {
            setSelectedProvince(matchedProvince);
            return;
          }
        } catch (err) {
          console.error('加载省份列表失败:', err);
        }
      }


      // 默认选择广东
      try {
        const provincesData = await medicalApi.getProvinces();
        const provincesList = Array.isArray(provincesData) ? provincesData : provincesData?.items ?? [];
        const guangdong = provincesList.find(p => 
          p.name === '广东' || 
          p.name === '广东省' ||
          p.name?.includes('广东')
        );
        if (guangdong) {
          setSelectedProvince(guangdong);
        } else if (provincesList.length > 0) {
          setSelectedProvince(provincesList[0]);
        }
      } catch (err) {
        console.error('加载默认省份失败:', err);
      }
    };

    initProvince();
  }, [searchParams]);

  // 加载省份列表
  useEffect(() => {
    const loadProvinces = async () => {
      try {
        const provincesData = await medicalApi.getProvinces();
        const provincesList = Array.isArray(provincesData) ? provincesData : provincesData?.items ?? [];
        setProvinces(provincesList);
      } catch (err) {
        console.error('加载省份列表失败:', err);
      }
    };
    loadProvinces();
  }, []);


  // 加载医院列表
  const loadHospitals = useCallback(async () => {
    if (!selectedProvince) return;

    setLoading(true);
    setError('');
    try {
      const params = {
        page: 1,
        pageSize: 1000,
        provinceId: selectedProvince.id
      };

      const response = await medicalApi.getTertiaryHospitals(params);
      const allHospitals = Array.isArray(response?.items) ? response.items : [];
      
      // 根据筛选类型进一步筛选
      let filteredHospitals = allHospitals.filter(h => {
        if (!h.isEnabled) return false;
        
        // 如果选择了"三甲医院"筛选，只显示三甲医院
        if (filterType === 'tertiary') {
          return h.level === '三甲' || h.level === 'Grade A Tertiary';
        }
        
        // 其他筛选类型暂时不过滤（后续可以扩展）
        return true;
      });

      // 如果选择"离我最近"，按 SortOrder 排序
      if (filterType === 'nearest') {
        // 按 SortOrder 排序
        filteredHospitals.sort((a, b) => {
          if (a.sortOrder !== b.sortOrder) {
            return (a.sortOrder || 0) - (b.sortOrder || 0);
          }
          return (a.name || '').localeCompare(b.name || '', 'zh-CN');
        });
      } else {
        // 其他情况按 SortOrder 排序
        filteredHospitals.sort((a, b) => {
          if (a.sortOrder !== b.sortOrder) {
            return (a.sortOrder || 0) - (b.sortOrder || 0);
          }
          return (a.name || '').localeCompare(b.name || '', 'zh-CN');
        });
      }

      setHospitals(filteredHospitals);
    } catch (err) {
      console.error('加载医院列表失败:', err);
      setError(err.message || '加载医院列表失败');
    } finally {
      setLoading(false);
    }
  }, [selectedProvince, filterType]);

  useEffect(() => {
    loadHospitals();
  }, [loadHospitals]);

  const handleProvinceChange = (province) => {
    setSelectedProvince(province);
    setShowProvinceDropdown(false);
    // 更新URL参数
    const provinceName = province.name?.replace('省', '').replace('市', '') || province.name;
    setSearchParams({ province: provinceName });
  };

  return (
    <PageLayout>
      <div className="hospital-search-page">
        {/* 顶部筛选栏 */}
        <div className="search-header">
          <div className="location-selector" onClick={() => setShowProvinceDropdown(!showProvinceDropdown)}>
            <span className="location-text">{selectedProvince?.name?.replace('省', '').replace('市', '') || '广东'}</span>
            <span className="dropdown-icon">▼</span>
            {showProvinceDropdown && (
              <div className="province-dropdown">
                {provinces.map(province => (
                  <div
                    key={province.id}
                    className={`province-item ${selectedProvince?.id === province.id ? 'active' : ''}`}
                    onClick={() => handleProvinceChange(province)}
                  >
                    {province.name?.replace('省', '').replace('市', '') || province.name}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* 筛选按钮 */}
        <div className="filter-buttons">
          <button
            className={`filter-btn ${filterType === 'nearest' ? 'active' : ''}`}
            onClick={() => setFilterType('nearest')}
          >
            离我最近
          </button>
          <button
            className={`filter-btn ${filterType === 'online' ? 'active' : ''}`}
            onClick={() => setFilterType('online')}
          >
            在线问诊
          </button>
          <button
            className={`filter-btn ${filterType === 'tertiary' ? 'active' : ''}`}
            onClick={() => setFilterType('tertiary')}
          >
            三甲医院
          </button>
        </div>

        {/* 全国医院榜单横幅 */}
        <div className="ranking-banner" onClick={() => navigate('/hospital-ranking')}>
          <div className="banner-left">
            <div className="banner-title">全国医院榜单</div>
            <div className="banner-subtitle">权威机构排行 官方数据支持</div>
          </div>
          <div className="banner-right">
            <div className="trophy-icon">🏆</div>
            <div className="view-button">立即查看&gt;</div>
          </div>
        </div>

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
            {hospitals.map((hospital) => (
              <div key={hospital.id} className="hospital-card">
                <div className="hospital-info">
                  <h3 className="hospital-name">{hospital.name}</h3>
                  
                  {/* 标签 */}
                  <div className="hospital-tags">
                    <span className="tag tag-public">公立</span>
                    <span className="tag tag-level">{hospital.level || '三甲'}</span>
                    <span className="tag tag-type">{hospital.type || '综合医院'}</span>
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

