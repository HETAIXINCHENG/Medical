import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';

// 权威医院排名（综合医院前50名）
const AUTHORITATIVE_HOSPITAL_RANKING = [
  '北京大学第三医院',
  '北京大学第一医院',
  '北京协和医院',
  '南方医科大学南方医院',
  '中山大学附属第一医院',
  '郑州大学第一附属医院',
  '华中科技大学同济医学院附属同济医院',
  '华中科技大学同济医学院附属协和医院',
  '中南大学湘雅医院',
  '江苏省人民医院',
  '中国医科大学附属第一医院',
  '山东大学齐鲁医院',
  '复旦大学附属华山医院',
  '复旦大学附属中山医院',
  '上海交通大学医学院附属第九人民医院',
  '上海交通大学医学院附属仁济医院',
  '上海交通大学医学院附属瑞金医院',
  '四川大学华西医院',
  '浙江大学医学院附属第二医院',
  '浙江大学医学院附属第一医院',
  '北京大学人民医院',
  '首都医科大学附属北京安贞医院',
  '首都医科大学附属北京天坛医院',
  '首都医科大学宣武医院',
  '中日友好医院',
  '广东省人民医院',
  '广州医科大学附属第一医院',
  '中山大学附属第三医院',
  '中山大学孙逸仙纪念医院',
  '武汉大学人民医院',
  '中南大学湘雅二医院',
  '南京鼓楼医院',
  '苏州大学附属第一医院',
  '中国医科大学附属盛京医院',
  '青岛大学附属医院',
  '山东第一医科大学附属省立医院',
  '上海交通大学医学院附属新华医院',
  '上海市第六人民医院',
  '四川省人民医院',
  '浙江大学医学院附属邵逸夫医院',
  '安徽医科大学第一附属医院',
  '中国科学技术大学附属第一医院',
  '北京朝阳医院',
  '北京积水潭医院',
  '北京医院',
  '首都医科大学附属北京同仁医院',
  '首都医科大学附属北京友谊医院',
  '重庆医科大学附属第一医院',
  '福建医科大学附属第一医院',
  '福建医科大学附属协和医院'
];

// 权威中医医院排名
const AUTHORITATIVE_TCM_HOSPITAL_RANKING = [
  '广东省中医院',
  '江苏省中医院',
  '上海中医药大学附属龙华医院',
  '中国中医科学院广安门医院',
  '中国中医科学院西苑医院',
  '上海中医药大学附属曙光医院',
  '北京中医药大学东直门医院',
  '广州中医药大学第一附属医院',
  '辽宁中医药大学附属医院',
  '浙江省中医院',
  '天津中医药大学第一附属医院',
  '成都中医药大学附属医院',
  '首都医科大学附属北京中医医院',
  '河南中医药大学第一附属医院',
  '山东中医药大学附属医院',
  '重庆市中医院',
  '上海中医药大学附属岳阳中西医结合医院',
  '湖北省中医院',
  '广西中医药大学第一附属医院',
  '长春中医药大学附属医院',
  '安徽中医药大学第一附属医院',
  '浙江省立同德医院',
  '黑龙江中医药大学附属第一医院',
  '武汉市第一医院',
  '福建中医药大学附属人民医院',
  '北京中医药大学东方医院',
  '中国中医科学院望京医院',
  '佛山市中医院',
  '湖南中医药大学第一附属医院',
  '陕西中医药大学附属医院',
  '江西中医药大学附属医院',
  '深圳市中医院',
  '成都市中西医结合医院',
  '新疆维吾尔自治区中医医院',
  '河北省中医院'
];

// 获取医院在权威排名中的位置（综合医院）
const getHospitalRank = (hospitalName) => {
  if (!hospitalName) return 9999; // 未排名的医院排在最后
  
  // 精确匹配
  const exactIndex = AUTHORITATIVE_HOSPITAL_RANKING.findIndex(name => name === hospitalName);
  if (exactIndex !== -1) return exactIndex + 1;
  
  // 模糊匹配（包含关系）
  const fuzzyIndex = AUTHORITATIVE_HOSPITAL_RANKING.findIndex(name => 
    hospitalName.includes(name) || name.includes(hospitalName)
  );
  if (fuzzyIndex !== -1) return fuzzyIndex + 1;
  
  return 9999; // 未找到，排在最后
};

// 获取中医医院在权威排名中的位置
const getTcmHospitalRank = (hospitalName) => {
  if (!hospitalName) return 9999; // 未排名的医院排在最后
  
  // 精确匹配
  const exactIndex = AUTHORITATIVE_TCM_HOSPITAL_RANKING.findIndex(name => name === hospitalName);
  if (exactIndex !== -1) return exactIndex + 1;
  
  // 模糊匹配（包含关系）
  const fuzzyIndex = AUTHORITATIVE_TCM_HOSPITAL_RANKING.findIndex(name => 
    hospitalName.includes(name) || name.includes(hospitalName)
  );
  if (fuzzyIndex !== -1) return fuzzyIndex + 1;
  
  return 9999; // 未找到，排在最后
};

export default function HospitalRanking() {
  usePageStyles('hospital-ranking.css');
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('tcm'); // 'tcm' 中医医院, 'general' 综合三甲医院
  const [hospitals, setHospitals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadHospitals = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      // 获取所有启用的三甲医院
      const response = await medicalApi.getTertiaryHospitals({ 
        page: 1, 
        pageSize: 1000 // 获取所有数据
      });
      
      const allHospitals = Array.isArray(response?.items) ? response.items : [];
      // 后端已经按 SortOrder 排序，这里只需要筛选，不需要重新排序
      
      // 根据标签页筛选医院类型
      let filteredHospitals = [];
      if (activeTab === 'tcm') {
        // 中医医院：Type 包含"中医"或"中医院"
        filteredHospitals = allHospitals.filter(h => 
          h.isEnabled && 
          h.type && (
            h.type.includes('中医') || 
            h.type.includes('中医院') ||
            h.type === '中医医院'
          )
        );
      } else {
        // 综合医院：Type 包含"综合"或等于"综合医院"
        filteredHospitals = allHospitals.filter(h => 
          h.isEnabled && 
          h.type && (
            h.type.includes('综合') || 
            h.type === '综合医院'
          )
        );
      }
      
      // 按照权威排名排序
      let sortedHospitals = [];
      
      if (activeTab === 'general') {
        // 综合医院：使用权威排名
        sortedHospitals = [...filteredHospitals]
          .map(hospital => ({
            ...hospital,
            rank: getHospitalRank(hospital.name)
          }))
          .sort((a, b) => {
            // 先按权威排名排序
            if (a.rank !== b.rank) {
              return a.rank - b.rank;
            }
            // 如果排名相同，按名称排序
            return (a.name || '').localeCompare(b.name || '', 'zh-CN');
          })
          .filter(hospital => hospital.rank <= 50) // 只显示前50名
          .slice(0, 50); // 确保最多50个
      } else {
        // 中医医院：使用权威排名
        sortedHospitals = [...filteredHospitals]
          .map(hospital => ({
            ...hospital,
            rank: getTcmHospitalRank(hospital.name)
          }))
          .sort((a, b) => {
            // 先按权威排名排序
            if (a.rank !== b.rank) {
              return a.rank - b.rank;
            }
            // 如果排名相同，按 SortOrder 排序
            if (a.sortOrder !== b.sortOrder) {
              return (a.sortOrder || 0) - (b.sortOrder || 0);
            }
            // 如果 SortOrder 也相同，按名称排序
            return (a.name || '').localeCompare(b.name || '', 'zh-CN');
          });
        // 中医医院显示所有在排名中的医院，不限制数量
      }
      
      // 只在数据真正变化时才更新状态
      setHospitals(prevHospitals => {
        // 比较数组长度和内容，避免不必要的更新
        if (prevHospitals.length !== sortedHospitals.length) {
          return sortedHospitals;
        }
        // 比较每个医院的ID，如果都相同则不更新
        const hasChanged = prevHospitals.some((prev, index) => 
          prev.id !== sortedHospitals[index]?.id
        );
        return hasChanged ? sortedHospitals : prevHospitals;
      });
    } catch (err) {
      console.error('加载医院列表失败:', err);
      setError(err.message || '加载医院列表失败');
    } finally {
      setLoading(false);
    }
  }, [activeTab]);

  useEffect(() => {
    loadHospitals();
  }, [loadHospitals]);

  return (
    <PageLayout>
      <div className="hospital-ranking-page">
        {/* 头部 */}
        <div className="ranking-header">
          <div className="header-title">
            <span className="title-text">全国医院榜单</span>
          </div>
          <div className="header-subtitle">权威机构排行 · 官方数据支持</div>
        </div>

        {/* 标签页 */}
        <div className="ranking-tabs">
          <button
            className={`tab-item ${activeTab === 'tcm' ? 'active' : ''}`}
            onClick={() => setActiveTab('tcm')}
          >
            中医医院
          </button>
          <button
            className={`tab-item ${activeTab === 'general' ? 'active' : ''}`}
            onClick={() => setActiveTab('general')}
          >
            综合医院
          </button>
        </div>

        {/* 说明文字 */}
        <div className="ranking-intro">
          <div className="intro-title">
            {activeTab === 'tcm' 
              ? '2023届中医医院' 
              : '中国医院竞争力报告综合医院榜'}
          </div>
          <div className="intro-content">
            {activeTab === 'tcm'
              ? '基于"四横八纵两国际"的排名体系，开展对比研究，形成年度行业报告。'
              : '《医院蓝皮书：中国医院竞争力报告(2025)》基于"四横八纵两国际"的排名体系，开展对比研究，形成年度行业报告。'}
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
                    {/* 距离信息（如果有） */}
                    {hospital.distance && (
                      <span className="hospital-distance">{hospital.distance}km</span>
                    )}
                  </div>
                  
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
