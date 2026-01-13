import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { buildImageUrl } from '../utils/imageUtils.js';

// 中国省市数据
const PROVINCES_AND_CITIES = [
  { value: 'all', label: '全国' },
  { value: 'beijing', label: '北京' },
  { value: 'shanghai', label: '上海' },
  { value: 'guangdong', label: '广东' },
  { value: 'zhejiang', label: '浙江' },
  { value: 'jiangsu', label: '江苏' },
  { value: 'shandong', label: '山东' },
  { value: 'sichuan', label: '四川' },
  { value: 'hubei', label: '湖北' },
  { value: 'henan', label: '河南' },
  { value: 'hunan', label: '湖南' },
  { value: 'fujian', label: '福建' },
  { value: 'anhui', label: '安徽' },
  { value: 'liaoning', label: '辽宁' },
  { value: 'hebei', label: '河北' },
  { value: 'shanxi', label: '山西' },
  { value: 'jilin', label: '吉林' },
  { value: 'heilongjiang', label: '黑龙江' },
  { value: 'jiangxi', label: '江西' },
  { value: 'chongqing', label: '重庆' },
  { value: 'tianjin', label: '天津' },
  { value: 'yunnan', label: '云南' },
  { value: 'guangxi', label: '广西' },
  { value: 'xinjiang', label: '新疆' },
  { value: 'neimenggu', label: '内蒙古' },
  { value: 'xizang', label: '西藏' },
  { value: 'ningxia', label: '宁夏' },
  { value: 'qinghai', label: '青海' },
  { value: 'gansu', label: '甘肃' },
  { value: 'guizhou', label: '贵州' },
  { value: 'hainan', label: '海南' },
  { value: 'shaanxi', label: '陕西' }
];

export default function Department() {
  usePageStyles('department.css');
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [searchParams] = useSearchParams();
  const consultationType = searchParams.get('type') || 'normal';
  const deptIdFromUrl = searchParams.get('deptId');
  const diseaseIdFromUrl = searchParams.get('diseaseId');
  
  const [departments, setDepartments] = useState([]);
  const [diseaseCategories, setDiseaseCategories] = useState([]);
  const [selectedDepartmentId, setSelectedDepartmentId] = useState('');
  const [selectedDiseaseId, setSelectedDiseaseId] = useState('');
  const [selectedRegion, setSelectedRegion] = useState('all');
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchValue, setSearchValue] = useState('');
  const [error, setError] = useState('');
  
  // 下拉列表展开状态
  const [openDropdown, setOpenDropdown] = useState('');

  // 加载科室列表
  useEffect(() => {
    const loadDepartments = async () => {
      try {
        const result = await medicalApi.getDepartments({ pageSize: 1000 });
        const departmentsList = result?.items ?? result ?? [];
        setDepartments(Array.isArray(departmentsList) ? departmentsList : []);
      } catch (err) {
        console.error('加载科室失败:', err);
      }
    };
    loadDepartments();
  }, []);

  // 当 URL 中的 deptId 变化时，同步更新选中状态
  useEffect(() => {
    if (deptIdFromUrl && departments.length > 0) {
      const foundDept = departments.find(dept => String(dept.id) === String(deptIdFromUrl));
      if (foundDept) {
        const currentDeptId = selectedDepartmentId ? String(selectedDepartmentId) : '';
        const newDeptId = String(foundDept.id);
        if (currentDeptId !== newDeptId) {
          console.log('从URL设置选中科室:', foundDept.name, 'ID:', foundDept.id);
          setSelectedDepartmentId(foundDept.id);
        }
      }
    }
    // 注意：不要在这里清除 selectedDepartmentId，因为可能会在加载过程中被意外清空
    // 清除逻辑应该在用户手动选择"全部科室"时触发
  }, [deptIdFromUrl, departments]);

  // 加载疾病分类列表
  useEffect(() => {
    const loadDiseaseCategories = async () => {
      try {
        // 尝试加载疾病分类，不管是否有 token
        const result = await medicalApi.getDiseaseCategories({ pageSize: 1000 });
        const diseaseList = result?.items ?? result ?? [];
        
        // 确保每个疾病都有 departmentId（支持不同的字段名格式）
        const validDiseases = Array.isArray(diseaseList) 
          ? diseaseList.filter(d => d.id && d.name && (d.departmentId || d.DepartmentId))
          : [];
        
        setDiseaseCategories(validDiseases);
        
        // 如果 URL 中有 diseaseId，设置为选中
        if (diseaseIdFromUrl) {
          const diseaseExists = validDiseases.some(disease => disease.id === diseaseIdFromUrl);
          if (diseaseExists) {
            setSelectedDiseaseId(diseaseIdFromUrl);
          }
        }
      } catch (apiErr) {
        // 静默失败，可能是权限问题，不影响页面功能
        setDiseaseCategories([]);
      }
    };
    loadDiseaseCategories();
  }, [diseaseIdFromUrl]);

  // 根据选择的科室或疾病加载医生
  useEffect(() => {
    const loadDoctors = async () => {
      setLoading(true);
      setError('');
      try {
        let params = { pageSize: 500 }; // 增加页面大小以获取更多医生
        
        // 优先使用 URL 中的 deptId（从首页跳转过来的情况）
        let effectiveDeptId = null;
        if (deptIdFromUrl) {
          effectiveDeptId = deptIdFromUrl;
          console.log('使用URL中的科室ID:', deptIdFromUrl);
        } else if (selectedDepartmentId) {
          effectiveDeptId = selectedDepartmentId;
          console.log('使用选中的科室ID:', selectedDepartmentId);
        }
        
        // 如果选择了疾病，需要通过疾病找到对应的科室
        if (selectedDiseaseId) {
          // 先找到疾病对应的科室
          const disease = diseaseCategories.find(d => 
            String(d.id) === String(selectedDiseaseId)
          );
          // 支持不同的字段名格式（departmentId 或 DepartmentId）
          const deptId = disease?.departmentId || disease?.DepartmentId;
          if (deptId) {
            params.departmentId = deptId;
            console.log('使用疾病的科室ID:', deptId);
          } else if (effectiveDeptId) {
            // 如果疾病没有关联科室，使用选中的科室或URL中的科室
            params.departmentId = effectiveDeptId;
          }
        } else if (effectiveDeptId) {
          // 如果选择了科室或URL中有科室ID，只加载该科室的医生
          params.departmentId = effectiveDeptId;
          console.log('加载科室医生，科室ID:', effectiveDeptId, '参数:', params);
        } else {
          // 如果没有选择科室和疾病，加载所有医生
          console.log('加载所有医生');
        }

        const result = await medicalApi.getDoctors(params);
        const doctorsList = result?.items ?? result ?? [];
        
        console.log('API返回的医生数量:', doctorsList.length, '参数:', params);
        if (doctorsList.length > 0) {
          console.log('第一个医生的科室ID:', doctorsList[0].departmentId || doctorsList[0].DepartmentId);
        }
        
        // 根据问诊类型过滤医生
        let filteredList = Array.isArray(doctorsList) ? doctorsList : [];
        if (consultationType === 'expert') {
          filteredList = filteredList.filter(doctor => 
            doctor.title && doctor.title.includes('主任医师')
          );
        } else {
          filteredList = filteredList.filter(doctor => 
            !doctor.title || !doctor.title.includes('主任医师')
          );
        }
        
        // 如果传递了 departmentId，再次确认过滤（双重保险）
        if (params.departmentId && filteredList.length > 0) {
          const deptIdStr = String(params.departmentId);
          const beforeFilter = filteredList.length;
          filteredList = filteredList.filter(doctor => {
            const doctorDeptId = String(doctor.departmentId || doctor.DepartmentId || '');
            return doctorDeptId === deptIdStr;
          });
          console.log(`前端二次过滤：从 ${beforeFilter} 个医生过滤到 ${filteredList.length} 个医生`);
        }
        
        console.log('最终显示的医生数量:', filteredList.length);
        setDoctors(filteredList);
      } catch (err) {
        setError(err.message ?? t('cannot-load-doctors'));
      } finally {
        setLoading(false);
      }
    };
    loadDoctors();
  }, [selectedDepartmentId, selectedDiseaseId, consultationType, diseaseCategories, deptIdFromUrl]);

  const filteredDoctors = useMemo(() => {
    let result = doctors;
    
    // 搜索过滤
    if (searchValue) {
      result = result.filter(
        (doctor) =>
          doctor.name?.includes(searchValue) || 
          doctor.title?.includes(searchValue) ||
          doctor.hospital?.includes(searchValue) ||
          doctor.specialty?.includes(searchValue)
      );
    }
    
    // 地区过滤（如果医生有地区信息）
    if (selectedRegion && selectedRegion !== 'all') {
      // 这里可以根据医生的地区字段进行过滤
      // 暂时先不过滤，因为医生实体可能没有地区字段
    }
    
    return result;
  }, [doctors, searchValue, selectedRegion]);

  const handleDepartmentSelect = (deptId) => {
    setSelectedDepartmentId(deptId);
    setSelectedDiseaseId(''); // 选择科室时清除疾病选择
    setOpenDropdown('');
    // 更新 URL
    if (deptId) {
      navigate(`/department?type=${consultationType}&deptId=${deptId}`, { replace: true });
    } else {
      navigate(`/department?type=${consultationType}`, { replace: true });
    }
  };

  const handleDiseaseSelect = (diseaseId) => {
    setSelectedDiseaseId(diseaseId);
    setSelectedDepartmentId(''); // 选择疾病时清除科室选择
    setOpenDropdown('');
    // 更新 URL
    if (diseaseId) {
      navigate(`/department?type=${consultationType}&diseaseId=${diseaseId}`, { replace: true });
    } else {
      navigate(`/department?type=${consultationType}`, { replace: true });
    }
  };

  const handleRegionSelect = (region) => {
    setSelectedRegion(region);
    setOpenDropdown('');
  };

  // 点击外部关闭下拉列表
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (!event.target.closest('.filter-item')) {
        setOpenDropdown('');
      }
    };
    if (openDropdown) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [openDropdown]);

  const selectedDepartment = departments.find(d => d.id === selectedDepartmentId);
  const selectedDisease = diseaseCategories.find(d => d.id === selectedDiseaseId);
  const selectedRegionLabel = PROVINCES_AND_CITIES.find(r => r.value === selectedRegion)?.label || '全国';

  return (
    <PageLayout>
      {/* 顶部搜索与返回 */}
      <div className="search-section">
        <Link to="/" className="back-link">
          <img src="/Img/return.png" alt="返回" className="back-arrow" />
        </Link>
        <div className="search-bar">
          <img src="/Img/search.png" alt="搜索" className="search-icon" />
          <input
            type="text"
            id="department-search-input"
            name="department-search"
            placeholder={t('search-department-keyword')}
            value={searchValue}
            onChange={(event) => setSearchValue(event.target.value)}
          />
          <div className="search-divider"></div>
          <button className="search-btn" type="button">
            {t('search')}
          </button>
        </div>
      </div>

      {/* 顶部筛选栏 */}
      <div className="filter-bar">
        <div className="filter-item">
          <div 
            className="filter-dropdown"
            onClick={() => setOpenDropdown(openDropdown === 'department' ? '' : 'department')}
          >
            <span className="filter-value">
              {selectedDepartment ? selectedDepartment.name : '全部科室'}
            </span>
            <span className="filter-arrow">▼</span>
          </div>
          {openDropdown === 'department' && (
            <div className="dropdown-menu">
              <div 
                className="dropdown-item"
                onClick={() => handleDepartmentSelect('')}
              >
                全部科室
              </div>
              {departments.map((dept) => (
                <div
                  key={dept.id}
                  className={`dropdown-item ${selectedDepartmentId === dept.id ? 'active' : ''}`}
                  onClick={() => handleDepartmentSelect(dept.id)}
                >
                  {dept.name}
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="filter-item">
          <div 
            className="filter-dropdown"
            onClick={() => setOpenDropdown(openDropdown === 'disease' ? '' : 'disease')}
          >
            <span className="filter-value">
              {selectedDisease ? selectedDisease.name : '全部疾病'}
            </span>
            <span className="filter-arrow">▼</span>
          </div>
          {openDropdown === 'disease' && (
            <div className="dropdown-menu">
              <div 
                className="dropdown-item"
                onClick={() => handleDiseaseSelect('')}
              >
                全部疾病
              </div>
              {diseaseCategories.map((disease) => (
                <div
                  key={disease.id}
                  className={`dropdown-item ${selectedDiseaseId === disease.id ? 'active' : ''}`}
                  onClick={() => handleDiseaseSelect(disease.id)}
                >
                  {disease.name}
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="filter-item">
          <div 
            className="filter-dropdown"
            onClick={() => setOpenDropdown(openDropdown === 'region' ? '' : 'region')}
          >
            <span className="filter-value">{selectedRegionLabel}</span>
            <span className="filter-arrow">▼</span>
          </div>
          {openDropdown === 'region' && (
            <div className="dropdown-menu">
              {PROVINCES_AND_CITIES.map((region) => (
                <div
                  key={region.value}
                  className={`dropdown-item ${selectedRegion === region.value ? 'active' : ''}`}
                  onClick={() => handleRegionSelect(region.value)}
                >
                  {region.label}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {error && <div className="error-tip">{error}</div>}

      {/* 医生列表 */}
      <div className="container">
        <div className="content">
          <div className="classification-section">
            <h2 className="section-title">
              {selectedDepartment ? selectedDepartment.name : 
                selectedDisease ? selectedDisease.name : 
                '全部医生'}
            </h2>
            {loading && <div className="loading">{t('loading')}</div>}
            {!loading && (
              <div className="doctor-list">
                {filteredDoctors.map((doctor) => (
                  <div 
                    className="doctor-card"
                    key={doctor.id}
                  >
                    <div className="doctor-avatar">
                      <img 
                        src={buildImageUrl(doctor.avatarUrl, '/Img/02-发现/1.png')} 
                        alt={doctor.name}
                        className="avatar-img"
                        onError={(e) => {
                          e.target.src = '/Img/02-发现/1.png';
                        }}
                      />
                    </div>
                    <div className="doctor-info">
                      <div className="doctor-header">
                        <span className="doctor-name">{doctor.name}</span>
                        <span className="doctor-tag">{doctor.title || '医生'}</span>
                      </div>
                      {doctor.department && (
                        <div className="doctor-department">{doctor.department}</div>
                      )}
                      <div className="doctor-specialty">{doctor.specialty ?? t('specialty-info-missing')}</div>
                      <div className="doctor-stats">
                        <span className="doctor-rating">
                          <span className="rating-icon">😊</span>
                          <span className="rating-score">{doctor.rating || '4.5'}</span>
                        </span>
                        <span className="rating-divider">|</span>
                        <span className="consultation-count">{doctor.consultationCount || 0}人已咨询</span>
                      </div>
                    </div>
                    <button 
                      className="consult-btn"
                      onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/doctor/${doctor.id}`);
                      }}
                    >
                      咨询
                    </button>
                  </div>
                ))}
                {filteredDoctors.length === 0 && !loading && (
                  <div className="empty">{t('no-related-doctors')}</div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </PageLayout>
  );
}
