import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { getAuthToken } from '../config/apiConfig.js';
import apiClient from '../services/apiClient.js';

export default function AddPatient() {
  usePageStyles('add-patient.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const from = searchParams.get('from'); // 来源页面，用于返回
  
  const [formData, setFormData] = useState({
    name: '',
    gender: '',
    dateOfBirth: '',
    provinceId: '',
    cityId: '',
    phoneNumber: '',
    relationship: '本人' // 与本人关系，默认为"本人"
  });
  const [provinces, setProvinces] = useState([]);
  const [cities, setCities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [loadingProvinces, setLoadingProvinces] = useState(true);

  // 加载省份列表
  useEffect(() => {
    const loadProvinces = async () => {
      try {
        const data = await medicalApi.getProvinces();
        setProvinces(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error('加载省份列表失败:', err);
        setProvinces([]);
      } finally {
        setLoadingProvinces(false);
      }
    };
    loadProvinces();
  }, []);

  // 当选择省份时，加载对应的城市列表
  useEffect(() => {
    const loadCities = async () => {
      if (!formData.provinceId) {
        setCities([]);
        setFormData(prev => ({ ...prev, cityId: '' }));
        return;
      }
      try {
        const data = await medicalApi.getCitiesByProvince(formData.provinceId);
        setCities(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error('加载城市列表失败:', err);
        setCities([]);
      }
    };
    loadCities();
  }, [formData.provinceId]);

  const handleInputChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    
    // 验证所有必填字段
    if (!formData.name.trim()) {
      setError('请输入患者姓名');
      return;
    }
    if (!formData.gender) {
      setError('请选择性别');
      return;
    }
    if (!formData.dateOfBirth) {
      setError('请选择出生日期');
      return;
    }
    if (!formData.provinceId) {
      setError('请选择省份');
      return;
    }
    if (!formData.cityId) {
      setError('请选择城市');
      return;
    }
    if (!formData.phoneNumber.trim()) {
      setError('请输入手机号码');
      return;
    }
    if (!formData.relationship) {
      setError('请选择与本人关系');
      return;
    }

    // 验证手机号格式
    const phoneRegex = /^1[3-9]\d{9}$/;
    if (!phoneRegex.test(formData.phoneNumber)) {
      setError('请输入正确的手机号码');
      return;
    }

    setLoading(true);
    try {
      const token = getAuthToken();
      if (!token) {
        navigate('/login', { state: { from: '/add-patient' } });
        return;
      }

      // 获取当前用户信息
      const currentUser = await medicalApi.getCurrentUser();
      if (!currentUser) {
        setError('获取用户信息失败，请重新登录');
        return;
      }

      // 获取选中的省份和城市名称
      const selectedProvince = provinces.find(p => p.id === formData.provinceId);
      const selectedCity = cities.find(c => c.id === formData.cityId);
      const address = selectedProvince && selectedCity 
        ? `${selectedProvince.name}${selectedCity.name}` 
        : '';

      // 根据"与本人关系"决定创建Patient还是FamilyMember
      if (formData.relationship === '本人') {
        // 如果选择"本人"，创建或更新Patient记录（使用当前登录用户的UserId）
        await apiClient.post('/api/patients/me', {
          realName: formData.name,
          gender: formData.gender === '男' ? 'Male' : formData.gender === '女' ? 'Female' : formData.gender,
          dateOfBirth: formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString() : null,
          phoneNumber: formData.phoneNumber,
          address: address
        });
      } else {
        // 如果选择其他关系，先确保当前用户有Patient记录，然后创建FamilyMember记录
        // 先尝试获取当前用户的Patient记录
        try {
          await apiClient.get('/api/patients/by-user/' + currentUser.id);
        } catch (err) {
          // 如果没有Patient记录，先创建一个（使用当前用户信息，不更新）
          await apiClient.post('/api/patients/me', {
            realName: currentUser.username || '本人',
            gender: null,
            dateOfBirth: null,
            phoneNumber: null,
            address: null
          });
        }

        // 将中文关系转换为枚举值（RelationshipType: Spouse=1, Child=2, Parent=3, Other=4）
        const relationshipMap = {
          '配偶': 1,  // Spouse
          '子女': 2,  // Child
          '父母': 3,  // Parent
          '其他': 4   // Other
        };

        // 创建FamilyMember记录（记录与当前用户的关系）
        await apiClient.post('/api/users/me/family-members', {
          name: formData.name,
          relationship: relationshipMap[formData.relationship] || 4,
          gender: formData.gender === '男' ? 'Male' : formData.gender === '女' ? 'Female' : null,
          dateOfBirth: formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString() : null,
          phoneNumber: formData.phoneNumber,
          idCardNumber: null
        });
      }

      // 保存成功后，返回来源页面或患者选择页面
      if (from) {
        navigate(from);
      } else {
        navigate('/pre-consultation');
      }
    } catch (err) {
      console.error('保存患者信息失败:', err);
      setError(err.message ?? '保存患者信息失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">诊前信息收集</h1>
      </div>

      {/* 警告提示 */}
      <div className="warning-banner">
        <span className="warning-icon">⚠</span>
        <span className="warning-text">
          提示: 急重症患者不适合网上诊疗/咨询,请即刻前往当地医院急诊。
        </span>
      </div>

      {/* 表单内容 */}
      <form className="patient-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label className="form-label">
            患者姓名 <span className="required">*</span>
          </label>
          <input
            type="text"
            className="form-input"
            placeholder="请输入真实姓名"
            value={formData.name}
            onChange={(e) => handleInputChange('name', e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label className="form-label">
            性别 <span className="required">*</span>
          </label>
          <div className="radio-group">
            <label className="radio-label">
              <input
                type="radio"
                name="gender"
                value="男"
                checked={formData.gender === '男'}
                onChange={(e) => handleInputChange('gender', e.target.value)}
              />
              <span>男</span>
            </label>
            <label className="radio-label">
              <input
                type="radio"
                name="gender"
                value="女"
                checked={formData.gender === '女'}
                onChange={(e) => handleInputChange('gender', e.target.value)}
              />
              <span>女</span>
            </label>
          </div>
        </div>

        <div className="form-group">
          <label className="form-label">
            出生日期 <span className="required">*</span>
          </label>
          <input
            type="date"
            className="form-input"
            placeholder="请选择出生日期"
            value={formData.dateOfBirth}
            onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
            max={new Date().toISOString().split('T')[0]}
            required
          />
        </div>

        <div className="form-group">
          <label className="form-label">
            所在省份 <span className="required">*</span>
          </label>
          <select
            className="form-select"
            value={formData.provinceId}
            onChange={(e) => {
              handleInputChange('provinceId', e.target.value);
              handleInputChange('cityId', ''); // 清空城市选择
            }}
            disabled={loadingProvinces}
            required
          >
            <option value="">请选择省份</option>
            {provinces.map(province => (
              <option key={province.id} value={province.id}>
                {province.name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label className="form-label">
            所在城市 <span className="required">*</span>
          </label>
          <select
            className="form-select"
            value={formData.cityId}
            onChange={(e) => handleInputChange('cityId', e.target.value)}
            disabled={!formData.provinceId || cities.length === 0}
            required
          >
            <option value="">{formData.provinceId ? '请选择城市' : '请先选择省份'}</option>
            {cities.map(city => (
              <option key={city.id} value={city.id}>
                {city.name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label className="form-label">
            手机号码 <span className="required">*</span>
          </label>
          <input
            type="tel"
            className="form-input"
            placeholder="请输入手机号码"
            value={formData.phoneNumber}
            onChange={(e) => handleInputChange('phoneNumber', e.target.value)}
            maxLength={11}
            required
          />
        </div>

        <div className="form-group">
          <label className="form-label">
            与本人关系 <span className="required">*</span>
          </label>
          <select
            className="form-select"
            value={formData.relationship}
            onChange={(e) => handleInputChange('relationship', e.target.value)}
            required
          >
            <option value="本人">本人</option>
            <option value="配偶">配偶</option>
            <option value="子女">子女</option>
            <option value="父母">父母</option>
            <option value="其他">其他</option>
          </select>
        </div>

        {/* 隐私说明 */}
        <div className="privacy-notice">
          <span className="notice-icon">✓</span>
          <span className="notice-text">
            了解您的性别年龄居住区域等基本信息,能帮助医生更好的给您诊断建议,同时信息也将严格对外保密(注:信息提交后无法修改或删除)
          </span>
        </div>

        {error && <div className="error-message">{error}</div>}

        {/* 提交按钮 */}
        <button type="submit" className="submit-btn" disabled={loading}>
          {loading ? '保存中...' : '下一步'}
        </button>
      </form>
    </PageLayout>
  );
}

