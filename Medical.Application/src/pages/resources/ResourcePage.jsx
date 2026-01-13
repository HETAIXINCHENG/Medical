import { useEffect, useMemo, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Typography,
  DatePicker,
  Upload,
  Row,
  Col,
  App,
  message
} from 'antd';
const { Search } = Input;
import { PlusOutlined, UploadOutlined, MessageOutlined, SearchOutlined } from '@ant-design/icons';
import resourceService from '../../api/resourceService.js';
import httpClient from '../../api/httpClient.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';
import dayjs from 'dayjs';

const { Dragger } = Upload;

const { Title } = Typography;
const componentMap = {
  input: Input,
  password: Input.Password,
  textarea: Input.TextArea,
  number: InputNumber,
  switch: Switch,
  date: DatePicker,
  upload: Upload
};

// 内部组件，在 App 组件内部使用
function ResourcePageContent({ resource }) {
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({ current: 1, pageSize: 10, total: 0 });
  const [modalOpen, setModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [form] = Form.useForm();
  const [fieldOptions, setFieldOptions] = useState({}); // 存储动态加载的选项
  const [searchKeyword, setSearchKeyword] = useState(''); // 搜索关键词
  const { message: messageApi } = App.useApp(); // 在 App 组件内部使用 App.useApp() 获取 message 实例
  
  // 监听消息类型变化（仅用于咨询消息管理）
  const messageType = Form.useWatch('messageType', form);
  
  // 监听表单中的省份ID值（用于三甲医院的省市二级联动）
  const formProvinceId = Form.useWatch('provinceId', form);
  
  // 使用 useState 跟踪省份ID，用于控制城市列表加载
  const [provinceId, setProvinceId] = useState(null);

  const fetchData = async (page = pagination.current, pageSize = pagination.pageSize, keyword = searchKeyword) => {
    setLoading(true);
    try {
      // 合并默认参数（如角色筛选）
      const params = {
        page,
        pageSize,
        ...(resource.defaultParams || {})
      };
      
      // 添加搜索关键词参数（如果后端支持 keyword 参数）
      if (keyword && keyword.trim()) {
        params.keyword = keyword.trim();
      }
      
      const response = await resourceService.list(resource.basePath, params);
      const items = Array.isArray(response) ? response : response?.items ?? response?.data ?? [];
      const total = response?.total ?? response?.count ?? items.length;
      
      // 调试：打印返回的数据，特别是头像字段
      if (resource.key === 'system-users') {
        console.log('[fetchData] 用户管理数据:', {
          response,
          items,
          '第一个用户': items[0],
          '第一个用户的avatarUrl': items[0]?.avatarUrl,
          '第一个用户的AvatarUrl': items[0]?.AvatarUrl,
          '第一个用户的所有keys': items[0] ? Object.keys(items[0]) : [],
          '第一个用户的完整数据': JSON.stringify(items[0], null, 2)
        });
      }
      
      // 调试：打印患者信息数据，特别是身份证号字段
      if (resource.key === 'patient-info') {
        console.log('[fetchData] 患者信息数据:', {
          response,
          items,
          '第一个患者': items[0],
          '第一个患者的idCardNumber': items[0]?.idCardNumber,
          '第一个患者的IdCardNumber': items[0]?.IdCardNumber,
          '第一个患者的所有keys': items[0] ? Object.keys(items[0]) : [],
          '第一个患者的完整数据': JSON.stringify(items[0], null, 2)
        });
      }
      
      setData(items);
      setPagination({ current: page, pageSize, total });
      
      // 调试：如果列表为空，打印警告信息
      if (items.length === 0 && resource.key === 'system-users') {
        console.warn('[fetchData] 用户管理列表为空，响应数据:', response);
      }
    } catch (err) {
      console.error('[fetchData] 数据加载失败:', err);
      messageApi.error(err.message ?? t('resource.loadFailed'));
      // 出错时也要设置空数组，避免显示旧数据
      setData([]);
      setPagination({ current: 1, pageSize, total: 0 });
    } finally {
      setLoading(false);
    }
  };

  // 处理搜索
  const handleSearch = (value) => {
    setSearchKeyword(value);
    setPagination((prev) => ({ ...prev, current: 1 })); // 搜索时重置到第一页
    fetchData(1, pagination.pageSize, value);
  };

  // 清空搜索
  const handleClearSearch = () => {
    setSearchKeyword('');
    setPagination((prev) => ({ ...prev, current: 1 }));
    fetchData(1, pagination.pageSize, '');
  };

  // 加载动态选项（如科室列表）
  const loadFieldOptions = async () => {
    const optionsMap = {};
    for (const field of resource.formFields || []) {
      if (field.loadOptionsFrom) {
        try {
          // 对于用户列表和医生列表，加载更多数据以支持模糊查询（最多1000条）
          const pageSize = (field.loadOptionsFrom.includes('/users') || field.loadOptionsFrom.includes('/doctors')) ? 1000 : 100;
          
          // 合并字段的查询参数（如 role 筛选）
          const params = {
            page: 1,
            pageSize,
            ...(field.loadOptionsParams || {}) // 合并字段特定的查询参数
          };
          
          const response = await resourceService.list(field.loadOptionsFrom, params);
          const items = Array.isArray(response) ? response : response?.items ?? response?.data ?? [];
          optionsMap[field.name] = items.map(item => {
            // 对于menuUrl字段，API已经返回了 { label, value } 格式，直接使用
            if (field.name === 'menuUrl' && item.label && item.value) {
              return {
                label: item.label,
                value: item.value
              };
            }
            // 对于咨询ID字段，显示更友好的标签
            if (field.name === 'consultationId') {
              const patientInfo = item.patient?.realName || '未知患者';
              const doctorInfo = item.doctor?.name || '未知医生';
              const statusInfo = item.status || '未知状态';
              return {
                label: `咨询 ${item.id?.substring(0, 8)}... (${patientInfo} - ${doctorInfo}, ${statusInfo})`,
                value: item.id
              };
            }
            // 对于patientId字段（patient-info资源），从用户表加载，显示用户名
            if (field.name === 'patientId' && field.loadOptionsFrom?.includes('/users')) {
              return {
                label: item.username || item.Username || String(item.id),
                value: item.id
              };
            }
            // 对于patientId字段（其他资源），显示患者真实姓名
            if (field.name === 'patientId') {
              return {
                label: item.realName || item.RealName || item.name || item.Name || String(item.id),
                value: item.id
              };
            }
            // 对于parentId字段（药品分类），显示分类名称
            if (field.name === 'parentId') {
              const idValue = item.id || item.Id;
              return {
                label: item.categoryName || item.CategoryName || item.name || item.Name || String(idValue),
                value: idValue ? String(idValue) : null // 确保value是字符串格式，null用于顶级分类
              };
            }
            // 对于categoryId字段（药品信息），显示分类名称
            if (field.name === 'categoryId') {
              return {
                label: item.categoryName || item.name || String(item.id),
                value: item.id
              };
            }
            // 对于role字段（用户管理的角色），使用角色的code作为value，name作为label
            if (field.name === 'role' && field.loadOptionsFrom?.includes('/roles')) {
              return {
                label: item.name || item.code || String(item.id),
                value: item.code || item.name || String(item.id) // 使用code作为value
              };
            }
            // 对于permissionType字段（权限类型），使用字典表的name作为label，code作为value
            if (field.name === 'permissionType' && item.name && item.code) {
              return {
                label: item.name,
                value: item.code
              };
            }
            // 对于author字段（健康知识的作者），使用医生姓名作为label，医生ID作为value
            if (field.name === 'author' && field.loadOptionsFrom?.includes('/doctors')) {
              return {
                label: item.name || String(item.id),
                value: item.id
              };
            }
            // 对于hospitalId字段（医生管理），使用医院名称作为label，医院ID作为value
            if (field.name === 'hospitalId' && field.loadOptionsFrom?.includes('/tertiaryhospitals')) {
              return {
                label: item.name || String(item.id),
                value: item.id
              };
            }
            return {
              label: item.name || item.title || item.username || item.categoryName || item.label || String(item.id),
              value: item.value || item.id
            };
          });
          
          // 对于parentId字段（药品分类），添加"顶级分类"选项
          if (field.name === 'parentId' && optionsMap[field.name]) {
            console.log('[parentId字段] 加载选项前:', optionsMap[field.name]);
            optionsMap[field.name] = [
              { label: '顶级分类', value: null },
              ...optionsMap[field.name]
            ];
            console.log('[parentId字段] 加载选项后:', optionsMap[field.name]);
          }
        } catch (err) {
          console.error(`加载 ${field.name} 选项失败:`, err);
        }
      }
    }
    setFieldOptions(optionsMap);
  };

  useEffect(() => {
    setPagination((prev) => ({ ...prev, current: 1 }));
    setSearchKeyword(''); // 切换资源时清空搜索
    fetchData(1, pagination.pageSize, '');
    loadFieldOptions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [resource.key]);

  // 使用 ref 来跟踪上一次的 provinceId，避免重复加载
  const prevProvinceIdRef = useRef(null);
  const isSettingFormValuesRef = useRef(false);
  const isInitializingRef = useRef(false); // 跟踪是否正在初始化新建模式

  // 加载城市列表的函数（在省份 Select 的 onChange 中调用）
  const loadCitiesByProvince = async (provinceIdValue, shouldClearCity = true, autoSelectFirst = false) => {
    console.log('[loadCitiesByProvince] 开始加载城市列表，省份ID:', provinceIdValue, 'shouldClearCity:', shouldClearCity, 'autoSelectFirst:', autoSelectFirst);
    
    if (!provinceIdValue) {
      console.log('[loadCitiesByProvince] 省份ID为空，清空城市选项');
      setFieldOptions(prev => ({
        ...prev,
        cityId: []
      }));
      prevProvinceIdRef.current = null;
      if (shouldClearCity) {
        form.setFieldValue('cityId', undefined);
      }
      return [];
    }
    
    // 如果省份ID没有变化，不重新加载
    if (prevProvinceIdRef.current === provinceIdValue && fieldOptions.cityId && fieldOptions.cityId.length > 0) {
      console.log('[loadCitiesByProvince] 省份ID未变化，返回已有城市选项');
      // 如果需要自动选择第一个城市且当前没有选择城市，则选择第一个
      if (autoSelectFirst) {
        const currentCityId = form.getFieldValue('cityId');
        if (!currentCityId && fieldOptions.cityId.length > 0) {
          form.setFieldValue('cityId', fieldOptions.cityId[0].value);
        }
      }
      return fieldOptions.cityId;
    }
    
    try {
      console.log('[loadCitiesByProvince] 正在请求城市列表，URL:', `/api/provinces/${provinceIdValue}/cities`);
      const response = await httpClient.get(`/api/provinces/${provinceIdValue}/cities`);
      const cities = Array.isArray(response) ? response : response?.items ?? response?.data ?? [];
      console.log('[loadCitiesByProvince] 获取到城市列表:', cities);
      
      const cityOptions = cities.map(city => {
        // 确保ID格式一致（转换为字符串）
        const cityId = city.id || city.Id;
        const cityName = city.name || city.Name || String(cityId);
        return {
          label: cityName,
          value: cityId ? String(cityId) : cityId // 确保value是字符串格式
        };
      });
      
      console.log('[loadCitiesByProvince] 城市选项:', cityOptions);
      
      // 立即更新城市选项
      setFieldOptions(prev => ({
        ...prev,
        cityId: cityOptions
      }));
      
      // 更新 ref
      prevProvinceIdRef.current = provinceIdValue;
      
      // 如果需要自动选择第一个城市
      if (autoSelectFirst && cityOptions.length > 0) {
        console.log('[loadCitiesByProvince] 自动选择第一个城市:', cityOptions[0]);
        form.setFieldValue('cityId', cityOptions[0].value);
      } else if (shouldClearCity) {
        // 如果省份改变了且需要清空城市，则清空城市值
        const currentCityId = form.getFieldValue('cityId');
        const cityExists = cityOptions.some(opt => opt.value === currentCityId);
        if (currentCityId && !cityExists) {
          console.log('[loadCitiesByProvince] 清空城市值，因为城市不在新列表中');
          form.setFieldValue('cityId', undefined);
        }
      }
      
      return cityOptions;
    } catch (err) {
      console.error('[loadCitiesByProvince] 加载城市列表失败:', err);
      setFieldOptions(prev => ({
        ...prev,
        cityId: []
      }));
      prevProvinceIdRef.current = provinceIdValue;
      if (shouldClearCity) {
        form.setFieldValue('cityId', undefined);
      }
      return [];
    }
  };

  // 计算是否为只读模式
  const isReadOnly = resource.readOnly === true || !resource.formFields || resource.formFields.length === 0;

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      
      // 对于 patient-info 资源，自动计算 BMI（如果身高和体重都有值）
      if (resource.key === 'patient-info') {
        const height = values.height;
        const weight = values.weight;
        if (height && weight && height > 0) {
          const heightInMeters = height / 100; // 转换为米
          const bmi = weight / (heightInMeters * heightInMeters);
          values.bmi = Number(bmi.toFixed(2));
        } else {
          values.bmi = null;
        }
        
        // 处理 username 字段：新建时需要通过 username 查找用户ID，编辑时不提交 username
        if (!editingRecord && values.username) {
          // 新建时，需要通过 username 查找用户ID
          try {
            const usersResponse = await httpClient.get('/api/users', { params: { keyword: values.username, pageSize: 1 } });
            const users = Array.isArray(usersResponse) ? usersResponse : usersResponse?.items ?? usersResponse?.data ?? [];
            const user = users.find(u => u.username === values.username);
            if (user) {
              values.patientId = user.id;
            } else {
              messageApi.error(t('resource.patient-info.field.username.notFound'));
              return;
            }
          } catch (err) {
            console.error('查找用户失败:', err);
            messageApi.error(t('resource.patient-info.field.username.lookupFailed'));
            return;
          }
        }
        // 编辑时，移除 username 字段，不提交
        if (editingRecord) {
          delete values.username;
        }
      }
      
      // 处理日期字段：只有有效日期才转换，null/undefined 保持原样
      const dateFields = ['recordTime', 'startTime', 'endTime', 'dateOfBirth', 'emergencyContactDateOfBirth'];
      dateFields.forEach(fieldName => {
        const dateValue = values[fieldName];
        if (dateValue) {
          if (dayjs.isDayjs(dateValue)) {
            if (dateValue.isValid()) {
              values[fieldName] = dateValue.toISOString();
            } else {
              values[fieldName] = null;
            }
          } else if (typeof dateValue === 'string' && dateValue.trim() !== '') {
            // 如果已经是字符串，尝试解析验证
            const parsed = dayjs(dateValue);
            if (parsed.isValid()) {
              values[fieldName] = parsed.toISOString();
            } else {
              values[fieldName] = null;
            }
          } else {
            values[fieldName] = null;
          }
        } else {
          // null, undefined, 空字符串都设为 null
          values[fieldName] = null;
        }
      });
      
      // 处理上传字段（coverImageUrl等）- 从 fileList 中提取 URL（服务器路径）
      // 遍历所有字段，检查哪些是上传字段
      resource.formFields?.forEach(field => {
        if (field.component === 'upload') {
          const fileList = values[field.name];
          const maxCount = field.componentProps?.maxCount || 1;
          const isMultiple = maxCount > 1;
          console.log(`[handleSubmit] ${field.name} fileList:`, fileList, 'maxCount:', maxCount, 'isMultiple:', isMultiple);
          
          if (Array.isArray(fileList) && fileList.length > 0) {
            // 处理多图片上传
            if (isMultiple) {
              const imageUrls = [];
              let hasError = false;
              
              for (const file of fileList) {
                let imageUrl = null;
                
                // 优先从 response 中获取（这是上传成功后服务器返回的路径）
                if (file.response) {
                  imageUrl = file.response.url || file.response.path;
                  // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                  if (typeof imageUrl === 'string') {
                    imageUrl = imageUrl.replace(/\\/g, '/');
                  }
                } else if (file.url && !file.url.startsWith('data:image')) {
                  // 如果没有 response，但有 url 且不是 base64，可能是编辑时加载的已有图片
                  imageUrl = file.url;
                  // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                  if (typeof imageUrl === 'string') {
                    imageUrl = imageUrl.replace(/\\/g, '/');
                  }
                } else if (file.thumbUrl && file.thumbUrl.startsWith('data:image')) {
                  // 如果只有 base64 预览，说明文件还未上传
                  console.warn(`[${field.name}] 文件只有base64预览，未上传到服务器`);
                  messageApi.warning(t('resource.uploadWait'));
                  hasError = true;
                  break;
                } else if (file.originFileObj && !file.status) {
                  // 如果有原始文件对象但没有状态，说明上传还未开始
                  console.warn(`[${field.name}] 文件已选择但未开始上传`);
                  messageApi.warning(t('resource.uploadWait'));
                  hasError = true;
                  break;
                }
                
                // 如果文件有 originFileObj 但没有 response 和 url，说明上传还未完成
                if (file.originFileObj && !file.response && !file.url) {
                  console.warn(`[${field.name}] 文件正在上传中，请等待上传完成`);
                  messageApi.warning(t('resource.uploadWait'));
                  hasError = true;
                  break;
                }
                
                // 如果是 base64 数据（临时预览），说明文件还未上传，提示用户
                if (imageUrl && imageUrl.startsWith('data:image')) {
                  console.warn(`[${field.name}] 检测到base64数据，文件未上传`);
                  messageApi.warning(t('resource.uploadWait'));
                  hasError = true;
                  break;
                }
                
                // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                if (imageUrl && typeof imageUrl === 'string') {
                  imageUrl = imageUrl.replace(/\\/g, '/');
                }
                
                // 如果 imageUrl 是完整的 HTTP URL，提取相对路径
                if (imageUrl && imageUrl.startsWith('http')) {
                  try {
                    const urlObj = new URL(imageUrl);
                    imageUrl = urlObj.pathname;
                  } catch (e) {
                    console.error(`[${field.name}] URL解析失败:`, e);
                  }
                }
                
                // 确保是服务器路径（以 /uploads/ 开头）
                if (imageUrl && !imageUrl.startsWith('/uploads/') && !imageUrl.startsWith('http')) {
                  imageUrl = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;
                }
                
                if (imageUrl && imageUrl.trim() !== '') {
                  imageUrls.push(imageUrl);
                }
              }
              
              if (hasError) {
                values[field.name] = null;
                return;
              }
              
              // 存储为 JSON 数组字符串
              if (imageUrls.length > 0) {
                values[field.name] = JSON.stringify(imageUrls);
                console.log(`[${field.name}] 多图片路径（JSON）:`, values[field.name]);
              } else {
                values[field.name] = null;
              }
            } else {
              // 单图片上传（原有逻辑）
              const file = fileList[0];
              console.log(`[handleSubmit] ${field.name} file对象:`, file);
              let imageUrl = null;
              
              // 优先从 response 中获取（这是上传成功后服务器返回的路径）
              if (file.response) {
                imageUrl = file.response.url || file.response.path;
                console.log(`[${field.name}] 从 response 提取路径:`, imageUrl);
              } else if (file.url && !file.url.startsWith('data:image')) {
                // 如果没有 response，但有 url 且不是 base64，可能是编辑时加载的已有图片
                imageUrl = file.url;
                console.log(`[${field.name}] 从 url 提取路径（原始）:`, imageUrl);
              } else if (file.thumbUrl && file.thumbUrl.startsWith('data:image')) {
                // 如果只有 base64 预览，说明文件还未上传
                console.warn(`[${field.name}] 文件只有base64预览，未上传到服务器`);
                messageApi.warning(t('resource.uploadWait'));
                values[field.name] = null;
                return;
              } else if (file.originFileObj && !file.status) {
                // 如果有原始文件对象但没有状态，说明上传还未开始
                console.warn(`[${field.name}] 文件已选择但未开始上传`);
                messageApi.warning(t('resource.uploadWait'));
                values[field.name] = null;
                return;
              }
              
              // 如果文件有 originFileObj 但没有 response 和 url，说明上传还未完成
              if (file.originFileObj && !file.response && !file.url) {
                console.warn(`[${field.name}] 文件正在上传中，请等待上传完成`);
                messageApi.warning(t('resource.uploadWait'));
                values[field.name] = null;
                return;
              }
              
              // 如果是 base64 数据（临时预览），说明文件还未上传，提示用户
              if (imageUrl && imageUrl.startsWith('data:image')) {
                console.warn(`[${field.name}] 检测到base64数据，文件未上传`);
                messageApi.warning(t('resource.uploadWait'));
                values[field.name] = null;
                return;
              }
              
              // 如果 imageUrl 是完整的 HTTP URL，提取相对路径
              if (imageUrl && imageUrl.startsWith('http')) {
                try {
                  const urlObj = new URL(imageUrl);
                  imageUrl = urlObj.pathname;
                  console.log(`[${field.name}] 从完整URL提取相对路径:`, imageUrl);
                } catch (e) {
                  console.error(`[${field.name}] URL解析失败:`, e);
                }
              }
              
              // 确保是服务器路径（以 /uploads/ 开头）
              if (imageUrl && !imageUrl.startsWith('/uploads/') && !imageUrl.startsWith('http')) {
                imageUrl = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;
              }
              
              if (!imageUrl || imageUrl.trim() === '') {
                console.warn(`[${field.name}] 图片URL为空，设为null`);
                values[field.name] = null;
              } else {
                console.log(`[${field.name}] 最终图片路径:`, imageUrl);
                values[field.name] = imageUrl; // 保存相对路径
              }
            }
          } else if (typeof fileList === 'string' && fileList.trim() !== '') {
            // 如果已经是字符串（URL 路径或 JSON 数组字符串），直接使用
            const url = fileList.trim();
            // 检查是否是 JSON 数组字符串
            if (url.startsWith('[') && url.endsWith(']')) {
              // 已经是 JSON 数组字符串，直接使用
              values[field.name] = url;
            } else {
              // 单个 URL 字符串
              values[field.name] = url;
            }
          } else {
            // 空数组、空字符串或其他情况
            // 对于编辑模式，如果原本有值但现在被删除，应该发送 null 来清空
            // 对于新建模式，如果字段是必填的，应该保持 null
            if (editingRecord && field.name === 'avatarUrl') {
              // 编辑模式下，如果用户删除了头像，应该发送 null 来清空数据库中的值
              values[field.name] = null;
            } else {
              // 新建模式或其他情况，设为 null
              values[field.name] = null;
            }
          }
        }
      });
      
      // 确保 departmentId 是字符串格式（如果存在）
      if (values.departmentId && typeof values.departmentId !== 'string') {
        values.departmentId = String(values.departmentId);
      }
      
      // 只保留当前资源配置中定义的字段
      const validFieldNames = new Set(resource.formFields?.map(f => f.name) || []);
      
      // 清理空值：将空字符串转换为 null，移除 undefined
      const payload = {};
      Object.keys(values).forEach(key => {
        // 只处理当前资源配置中定义的字段
        if (!validFieldNames.has(key)) {
          return;
        }
        
        const value = values[key];
        if (value === undefined) {
          // 跳过 undefined
          return;
        }
        // 处理空值
        // 对于密码字段，编辑时如果为空字符串，则不包含在 payload 中（不更新密码）
        if (key === 'password' && editingRecord && (value === '' || value === null || value === undefined)) {
          // 编辑模式下，密码为空时不更新密码
          return;
        }
        
        if (value === '' || value === 'null' || value === 'undefined') {
          payload[key] = null;
        } else if (Array.isArray(value) && value.length === 0) {
          // 空数组设为 null（对于可选字段）
          payload[key] = null;
        } else if (value === null) {
          // 保持 null
          payload[key] = null;
        } else {
          payload[key] = value;
        }
      });
      
      // 移除不需要的字段（如 patientId，后端会自动创建）
      if (!editingRecord && resource.key === 'doctors') {
        // 创建医生时，不需要 patientId，后端会自动创建
        delete payload.patientId;
      }
      
      // 移除系统字段（不应该从前端发送）
      // 但对于 patient-info 资源，保留 id 字段用于更新
      if (resource.key !== 'patient-info') {
        delete payload.id;
      }
      delete payload.createdAt;
      delete payload.updatedAt;
      
      // 移除导航属性（不应该从前端发送）
      // 对于商品信息，移除 Category 对象，只保留 categoryId
      if (resource.key === 'products') {
        delete payload.category;
        delete payload.Category;
        // 确保 categoryId 存在且有效
        if (!payload.categoryId || payload.categoryId === 'null' || payload.categoryId === 'undefined') {
          // 如果 categoryId 无效，尝试从 Category 对象中提取（虽然应该已经删除了）
          console.warn('商品分类ID无效，请选择商品分类');
        }
      }
      
      // 处理密码字段：编辑时，如果密码为空字符串，则不更新密码（移除该字段）
      if (editingRecord && payload.password === '') {
        delete payload.password;
      }
      
      // 检查图片字段：确保是服务器路径，不是 base64
      Object.keys(payload).forEach(key => {
        if (payload[key] && typeof payload[key] === 'string' && payload[key].startsWith('data:image')) {
          console.warn(`字段 ${key} 包含 base64 数据，但应该使用服务器路径`);
          payload[key] = null;
          messageApi.warning(t('resource.imageNotUploaded', { field: key }));
        }
      });
      
      console.log('提交数据:', JSON.stringify(payload, null, 2));
      console.log('提交数据中的avatarUrl:', payload.avatarUrl);
      
      // 对于医生管理，将hospitalId转换为hospital（医院名称）
      if (resource.key === 'doctors' && payload.hospitalId) {
        try {
          // 从已加载的选项中找到对应的医院名称
          const hospitalOptions = fieldOptions.hospitalId || [];
          const selectedHospital = hospitalOptions.find(opt => opt.value === payload.hospitalId);
          if (selectedHospital) {
            payload.hospital = selectedHospital.label;
          } else {
            // 如果选项中没有，尝试从API获取
            const hospitalResponse = await httpClient.get(`/api/tertiaryhospitals/${payload.hospitalId}`);
            payload.hospital = hospitalResponse.name || hospitalResponse.Name || '';
          }
          // 删除hospitalId字段，因为后端只需要hospital（字符串）
          delete payload.hospitalId;
        } catch (err) {
          console.error('获取医院名称失败:', err);
          // 如果获取失败，保留hospitalId，让后端处理
        }
      }
      
      // 如果有onCreate钩子，处理创建前的数据转换
      let finalPayload = payload;
      if (!editingRecord && resource.onCreate) {
        finalPayload = resource.onCreate(payload);
      }
      
      console.log('最终提交数据:', JSON.stringify(finalPayload, null, 2));
      console.log('最终提交数据中的avatarUrl:', finalPayload.avatarUrl);
      
      if (editingRecord) {
        const updateResult = await resourceService.update(resource.basePath, editingRecord[resource.primaryKey], finalPayload);
        console.log('更新后的返回结果:', updateResult);
        messageApi.success(t('resource.saveSuccess'));
      } else {
        const createResult = await resourceService.create(resource.basePath, finalPayload);
        console.log('创建后的返回结果:', createResult);
        messageApi.success(t('resource.saveSuccess'));
      }
      setModalOpen(false);
      setEditingRecord(null);
      form.resetFields();
      // 刷新列表数据，确保使用当前分页和搜索条件
      await fetchData(pagination.current, pagination.pageSize, searchKeyword);
    } catch (err) {
      if (err?.errorFields) return;
      
      // 获取详细的错误信息
      let errorMessage = t('resource.saveFailed');
      if (err?.response?.data) {
        const errorData = err.response.data;
        if (errorData.message) {
          errorMessage = errorData.message;
        } else if (errorData.errors && Array.isArray(errorData.errors)) {
          errorMessage = errorData.errors.join(', ');
        } else if (typeof errorData === 'string') {
          errorMessage = errorData;
        }
      } else if (err?.message) {
        errorMessage = err.message;
      }
      
      console.error('提交错误:', err);
      console.error('错误详情:', err?.response?.data);
      // 如果有详细的错误信息，也显示在控制台
      if (err?.response?.data) {
        console.error('完整错误响应:', JSON.stringify(err.response.data, null, 2));
      }
      messageApi.error(errorMessage);
    }
  };

  const handleDelete = async (record) => {
    Modal.confirm({
      title: t('resource.deleteConfirm'),
      onOk: async () => {
        try {
          // 正常删除（现在 patients 资源直接绑定到 /api/patients，不需要特殊处理）
          await resourceService.remove(resource.basePath, record[resource.primaryKey]);
          messageApi.success(t('resource.deleteSuccess'));
          fetchData(pagination.current, pagination.pageSize, searchKeyword);
        } catch (err) {
          messageApi.error(err.message ?? t('resource.deleteFailed'));
        }
      }
    });
  };

  // 清空购物车（删除整条购物车记录）
  const handleClearCart = async (record) => {
    Modal.confirm({
      title: t('resource.carts.clearConfirm'),
      content: t('resource.carts.clearContent'),
      onOk: async () => {
        try {
          await resourceService.remove(resource.basePath, record[resource.primaryKey]);
          messageApi.success(t('resource.carts.clearSuccess'));
          fetchData(pagination.current, pagination.pageSize, searchKeyword);
        } catch (err) {
          messageApi.error(err.message ?? t('resource.carts.clearFailed'));
        }
      }
    });
  };

  // 冻结/解冻患友会
  const handleToggleFreeze = async (record) => {
    const isEnabled = record.isEnabled ?? record.IsEnabled ?? true;
    const action = isEnabled ? t('resource.freeze') : t('resource.unfreeze');
    Modal.confirm({
      title: t('resource.freezeConfirm', { action }),
      content: isEnabled 
        ? t('resource.freezeContent')
        : t('resource.unfreezeContent'),
      onOk: async () => {
        try {
          await httpClient.post(`${resource.basePath}/${record[resource.primaryKey]}/toggle-freeze`);
          messageApi.success(t('resource.freezeSuccess', { action }));
          fetchData(pagination.current, pagination.pageSize, searchKeyword);
        } catch (err) {
          messageApi.error(err.message ?? t('resource.freezeFailed', { action }));
        }
      }
    });
  };

  // 提取字段映射逻辑到单独的函数
  const mapRecordToFormValues = (record) => {
    if (!record) return {};
    
    const mapped = {};
    
    // 先复制所有字段的值
    resource.formFields?.forEach((field) => {
      // 对于密码字段，编辑时始终设置为空字符串（不显示已有密码，安全考虑）
      if (field.component === 'password') {
        mapped[field.name] = '';
        return;
      }
      
      // 尝试多种方式获取字段值（因为后端可能返回不同的字段名格式）
      let fieldValue = record[field.name];
      
      // 如果直接获取失败，尝试其他可能的字段名
      if (fieldValue === undefined || fieldValue === null) {
        // 对于 avatarUrl 字段，尝试多种可能的字段名
        if (field.name === 'avatarUrl') {
          fieldValue = record.avatarUrl || record.AvatarUrl || record.avatar_url || record.avatar;
        }
        // 对于 patientId 字段（patient-info资源），从 PatientId 字段获取
        if (field.name === 'patientId' && resource.key === 'patient-info') {
          fieldValue = record.PatientId || record.patientId;
        }
        // 对于 id 字段（patient-info资源），从 Id 字段获取
        if (field.name === 'id' && resource.key === 'patient-info') {
          fieldValue = record.Id || record.id;
        }
        // 对于 username 字段（patient-info资源），从 user.username 字段获取
        if (field.name === 'username' && resource.key === 'patient-info') {
          fieldValue = record.user?.username || record.User?.Username || record.username;
        }
        // 对于 parentId 字段（药品分类），从 parentId 或 parent.id 字段获取
        if (field.name === 'parentId') {
          fieldValue = record.parentId || record.parent?.id || record.ParentId || record.Parent?.Id;
          // 如果parentId等于当前记录的id（自己作为父分类），转换为null（顶级分类）
          if (fieldValue && record.id && String(fieldValue) === String(record.id)) {
            fieldValue = null;
          }
          // 确保value是字符串格式（用于匹配选项），null保持不变
          if (fieldValue !== null && fieldValue !== undefined) {
            fieldValue = String(fieldValue);
          }
        }
        // 对于 cityId 字段（三甲医院），确保转换为字符串格式以匹配选项
        if (field.name === 'cityId' && resource.key === 'tertiary-hospitals') {
          fieldValue = record.cityId || record.CityId;
          if (fieldValue !== null && fieldValue !== undefined) {
            fieldValue = String(fieldValue);
          }
        }
        // 对于 hospitalId 字段（医生管理），从 hospital 字段（医院名称）查找对应的医院ID
        if (field.name === 'hospitalId' && resource.key === 'doctors') {
          const hospitalName = record.hospital || record.Hospital;
          if (hospitalName) {
            // 从已加载的选项中找到对应的医院ID
            const hospitalOptions = fieldOptions.hospitalId || [];
            const matchedHospital = hospitalOptions.find(opt => opt.label === hospitalName);
            if (matchedHospital) {
              fieldValue = matchedHospital.value;
            } else {
              // 如果选项中没有，尝试从API查找
              // 注意：这里需要异步处理，但mapRecordToFormValues是同步函数
              // 所以先设置为undefined，在useEffect中异步加载
              fieldValue = undefined;
            }
          }
        }
        // 对于 categoryId 字段（商品信息），从 categoryId 或 category.id 字段获取
        if (field.name === 'categoryId' && resource.key === 'products') {
          fieldValue = record.categoryId || record.CategoryId || record.category?.id || record.Category?.Id;
          // 确保value是字符串格式（用于匹配选项），null保持不变
          if (fieldValue !== null && fieldValue !== undefined) {
            fieldValue = String(fieldValue);
          }
        }
      }
      
      // 如果字段值仍然为空，且字段有 initialValue，使用 initialValue（仅用于新建模式，编辑模式应该使用实际值）
      // 注意：这里不设置 initialValue，因为编辑模式应该显示实际值，新建模式在 openModal 中已经设置了 initialValues
      
      // 处理日期字段
      if (field.component === 'date' && fieldValue) {
        mapped[field.name] = dayjs(fieldValue);
      }
      // 处理上传字段：如果有URL，转换为文件列表格式
      else if (field.component === 'upload') {
        // 尝试多种可能的字段名（因为后端可能返回不同的字段名，注意大小写）
        let imageUrl = fieldValue || 
                        record[field.name] || 
                        record.avatarUrl || 
                        record.AvatarUrl ||
                        record.coverImageUrl || 
                        record.CoverImageUrl ||
                        record.coverUrl ||
                        record.CoverUrl ||
                        record.imageUrl ||
                        record.ImageUrl;
        
        // 调试：打印图片字段信息（仅商品信息）
        if (resource.key === 'products' && field.name === 'coverUrl') {
          console.log('[mapRecordToFormValues] 商品图片字段调试:', {
            fieldName: field.name,
            fieldValue: fieldValue,
            recordField: record[field.name],
            recordCoverUrl: record.coverUrl,
            recordCoverUrlUpper: record.CoverUrl,
            finalImageUrl: imageUrl,
            recordKeys: Object.keys(record),
            fullRecord: record
          });
        }
        
        const maxCount = field.componentProps?.maxCount || 1;
        
        // 检查是否是 JSON 数组字符串（多图片）
        if (typeof imageUrl === 'string' && imageUrl.trim() !== '' && imageUrl.trim().startsWith('[') && imageUrl.trim().endsWith(']')) {
          try {
            const imageUrls = JSON.parse(imageUrl);
            if (Array.isArray(imageUrls) && imageUrls.length > 0) {
              const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
              mapped[field.name] = imageUrls.map((url, index) => {
                // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                let normalizedUrl = typeof url === 'string' ? url.replace(/\\/g, '/') : url;
                
                let fullUrl;
                if (normalizedUrl.startsWith('http://') || normalizedUrl.startsWith('https://')) {
                  fullUrl = normalizedUrl;
                } else if (normalizedUrl.startsWith('/')) {
                  fullUrl = `${baseURL}${normalizedUrl}`;
                } else {
                  fullUrl = `${baseURL}/${normalizedUrl}`;
                }
                return {
                  uid: `-${index + 1}`,
                  name: `image-${index + 1}`,
                  status: 'done',
                  url: fullUrl,
                  thumbUrl: fullUrl,
                  response: { url: normalizedUrl, path: normalizedUrl }
                };
              });
            } else {
              mapped[field.name] = [];
            }
          } catch (e) {
            mapped[field.name] = [];
          }
        }
        // 确保是字符串（不是数组）- 单图片
        else if (typeof imageUrl === 'string' && imageUrl.trim() !== '') {
          // 保存原始路径（相对路径，用于提交）
          let originalPath = imageUrl.trim();
          
          // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
          originalPath = originalPath.replace(/\\/g, '/');
          
          // 确保路径以 / 开头（如果不是完整URL）
          if (!originalPath.startsWith('http://') && !originalPath.startsWith('https://') && !originalPath.startsWith('/')) {
            originalPath = `/${originalPath}`;
          }
          
          // 构建完整的图片 URL（用于预览）
          const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
          let fullUrl;
          
          if (originalPath.startsWith('http://') || originalPath.startsWith('https://')) {
            fullUrl = originalPath;
          } else if (originalPath.startsWith('/')) {
            fullUrl = `${baseURL}${originalPath}`;
          } else {
            fullUrl = `${baseURL}/${originalPath}`;
          }
          
          // 调试：打印图片URL构建信息（仅商品信息）
          if (resource.key === 'products' && field.name === 'coverUrl') {
            console.log('[mapRecordToFormValues] 商品图片URL构建:', {
              originalImageUrl: imageUrl,
              originalPath: originalPath,
              fullUrl: fullUrl,
              baseURL: baseURL
            });
          }
          
          mapped[field.name] = [{ 
            uid: '-1',
            name: 'image',
            status: 'done',
            url: fullUrl,
            thumbUrl: fullUrl,
            response: { url: originalPath, path: originalPath } // 保存原始路径，用于提交
          }];
        } else if (Array.isArray(imageUrl) && imageUrl.length > 0) {
          // 如果已经是数组，确保 URL 正确
          const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
          mapped[field.name] = imageUrl.map(file => {
            if (file.url && !file.url.startsWith('http') && file.url.startsWith('/')) {
              const fullUrl = `${baseURL}${file.url}`;
              return {
                ...file,
                url: fullUrl,
                thumbUrl: fullUrl,
                // 确保 response 存在
                response: file.response || { url: file.url, path: file.url }
              };
            }
            return file;
          });
        } else {
          // 如果 imageUrl 为空或无效，设为空数组
          mapped[field.name] = [];
        }
      }
      // 处理其他字段（包括 select、input、switch 等）
      else {
        // Switch 字段：确保有布尔值
        if (field.component === 'switch') {
          // 如果值为 undefined 或 null，使用 initialValue 或 false
          mapped[field.name] = fieldValue !== undefined && fieldValue !== null ? Boolean(fieldValue) : (field.initialValue !== undefined ? field.initialValue : false);
        }
        // 其他字段：直接使用原值
        else {
          mapped[field.name] = fieldValue !== undefined && fieldValue !== null ? fieldValue : undefined;
        }
      }
    });
    
    // 确保所有字段都有值（即使是 undefined），这样表单才能正确显示
    resource.formFields?.forEach((field) => {
      if (!(field.name in mapped)) {
        // 根据字段类型设置默认值
        if (field.component === 'password') {
          mapped[field.name] = ''; // 密码字段默认为空
        } else if (field.component === 'switch') {
          mapped[field.name] = field.initialValue !== undefined ? field.initialValue : false; // Switch 字段使用 initialValue 或 false
        } else {
          mapped[field.name] = undefined;
        }
      }
    });
    
    return mapped;
  };

  const openModal = (record) => {
    setEditingRecord(record ?? null);
    // 重置 refs 和状态，确保每次打开弹窗时都能正确工作
    prevProvinceIdRef.current = null;
    isSettingFormValuesRef.current = false;
    isInitializingRef.current = false; // 重置初始化标记
    setProvinceId(null); // 重置省份ID状态
    
    if (!record) {
      // 新建模式：重置表单并设置初始值
      form.resetFields();
      const initialValues = resource.formFields?.reduce(
        (acc, field) => {
          if (field.initialValue !== undefined) {
            acc[field.name] = field.initialValue;
          }
          return acc;
        },
        {}
      );
      if (initialValues && Object.keys(initialValues).length > 0) {
        form.setFieldsValue(initialValues);
      }
    }
    setModalOpen(true);
  };

  // 当 Modal 打开且 editingRecord 存在时，设置表单值
  useEffect(() => {
    if (modalOpen && editingRecord) {
      // 标记正在设置表单值，避免触发省份变化的 useEffect
      isSettingFormValuesRef.current = true;
      
      // 对于三甲医院，如果记录中有 provinceId，先加载城市列表并设置省份ID状态
      if (resource.key === 'tertiary-hospitals' && editingRecord.provinceId) {
        // 更新 prevProvinceIdRef 和 provinceId 状态，避免触发省份变化的 useEffect
        prevProvinceIdRef.current = editingRecord.provinceId;
        setProvinceId(editingRecord.provinceId);
        
        // 使用 loadCitiesByProvince 函数加载城市列表（编辑模式初始化时不清空城市值）
        loadCitiesByProvince(editingRecord.provinceId, false).then((cityOptions) => {
          console.log('[编辑模式] 城市列表加载完成，选项数量:', cityOptions.length);
          console.log('[编辑模式] 城市选项:', cityOptions);
          console.log('[编辑模式] 编辑记录的城市ID:', editingRecord.cityId);
          
          // 确保城市选项已完全加载和渲染，使用更长的延迟
          setTimeout(() => {
            const mapped = mapRecordToFormValues(editingRecord);
            // 确保城市ID格式一致（转换为字符串）
            if (mapped.cityId) {
              mapped.cityId = String(mapped.cityId);
            }
            console.log('[编辑模式] 设置表单值:', mapped);
            form.setFieldsValue(mapped);
            // 设置完成后，取消标记
            isSettingFormValuesRef.current = false;
          }, 200); // 增加延迟时间，确保城市选项已完全渲染
        }).catch(() => {
          // 即使加载失败，也设置表单值
          setTimeout(() => {
            const mapped = mapRecordToFormValues(editingRecord);
            // 确保城市ID格式一致（转换为字符串）
            if (mapped.cityId) {
              mapped.cityId = String(mapped.cityId);
            }
            form.setFieldsValue(mapped);
            // 设置完成后，取消标记
            isSettingFormValuesRef.current = false;
          }, 200);
        });
      } else {
        // 对于医生管理，如果记录中有医院名称，需要查找对应的医院ID
        if (resource.key === 'doctors' && editingRecord.hospital) {
          const hospitalName = editingRecord.hospital || editingRecord.Hospital;
          // 等待医院选项加载完成
          const checkAndSetHospitalId = async () => {
            // 如果医院选项还没有加载，先加载
            if (!fieldOptions.hospitalId || fieldOptions.hospitalId.length === 0) {
              try {
                const hospitalsResponse = await httpClient.get('/api/tertiaryhospitals', { params: { pageSize: 1000 } });
                const hospitals = Array.isArray(hospitalsResponse) ? hospitalsResponse : hospitalsResponse?.items ?? hospitalsResponse?.data ?? [];
                const hospitalOptions = hospitals.map(h => ({
                  label: h.name || String(h.id),
                  value: h.id
                }));
                setFieldOptions(prev => ({
                  ...prev,
                  hospitalId: hospitalOptions
                }));
                // 查找对应的医院ID
                const matchedHospital = hospitalOptions.find(opt => opt.label === hospitalName);
                if (matchedHospital) {
                  const mapped = mapRecordToFormValues(editingRecord);
                  mapped.hospitalId = matchedHospital.value;
                  form.setFieldsValue(mapped);
                } else {
                  const mapped = mapRecordToFormValues(editingRecord);
                  form.setFieldsValue(mapped);
                }
              } catch (err) {
                console.error('加载医院列表失败:', err);
                const mapped = mapRecordToFormValues(editingRecord);
                form.setFieldsValue(mapped);
              }
            } else {
              // 医院选项已加载，直接查找
              const matchedHospital = fieldOptions.hospitalId.find(opt => opt.label === hospitalName);
              const mapped = mapRecordToFormValues(editingRecord);
              if (matchedHospital) {
                mapped.hospitalId = matchedHospital.value;
              }
              form.setFieldsValue(mapped);
            }
            isSettingFormValuesRef.current = false;
          };
          checkAndSetHospitalId();
        } else {
          // 使用 setTimeout 确保表单已经渲染完成，并且选项已加载
          const timer = setTimeout(() => {
            const mapped = mapRecordToFormValues(editingRecord);
            form.setFieldsValue(mapped);
            // 设置完成后，取消标记
            isSettingFormValuesRef.current = false;
          }, 100); // 增加延迟，确保选项已加载
          return () => clearTimeout(timer);
        }
      }
    } else if (modalOpen && !editingRecord) {
      // 新建模式：确保表单已重置
      // 使用 ref 防止重复初始化
      if (isInitializingRef.current) {
        return;
      }
      
      isInitializingRef.current = true;
      form.resetFields();
      const initialValues = resource.formFields?.reduce(
        (acc, field) => {
          if (field.initialValue !== undefined) {
            acc[field.name] = field.initialValue;
          }
          return acc;
        },
        {}
      );
      
      // 对于三甲医院，设置默认省份为"北京"，并加载城市列表
      if (resource.key === 'tertiary-hospitals') {
        // 查找"北京"省份的ID
        const loadDefaultProvince = async () => {
          try {
            // 获取所有省份
            const provincesResponse = await httpClient.get('/api/provinces');
            const provinces = Array.isArray(provincesResponse) ? provincesResponse : provincesResponse?.items ?? provincesResponse?.data ?? [];
            // 查找"北京"或"北京市"
            const beijingProvince = provinces.find(p => 
              p.name === '北京' || p.name === '北京市' || p.Name === '北京' || p.Name === '北京市'
            );
            
            if (beijingProvince) {
              const beijingProvinceId = beijingProvince.id || beijingProvince.Id;
              console.log('[新建模式] 找到北京省份ID:', beijingProvinceId);
              
              // 设置默认省份
              setProvinceId(beijingProvinceId);
              prevProvinceIdRef.current = null; // 重置，强制加载城市列表
              
              // 加载北京的城市列表
              const cityOptions = await loadCitiesByProvince(beijingProvinceId, false);
              
              // 设置表单值：省份和第一个城市
              const defaultValues = {
                ...initialValues,
                provinceId: beijingProvinceId,
                cityId: cityOptions.length > 0 ? cityOptions[0].value : undefined
              };
              
              console.log('[新建模式] 设置默认值:', defaultValues);
              form.setFieldsValue(defaultValues);
            } else {
              console.warn('[新建模式] 未找到北京省份');
              if (initialValues && Object.keys(initialValues).length > 0) {
                form.setFieldsValue(initialValues);
              }
            }
          } catch (err) {
            console.error('[新建模式] 加载默认省份失败:', err);
            if (initialValues && Object.keys(initialValues).length > 0) {
              form.setFieldsValue(initialValues);
            }
          } finally {
            isInitializingRef.current = false;
          }
        };
        
        loadDefaultProvince();
      } else {
        if (initialValues && Object.keys(initialValues).length > 0) {
          const timer = setTimeout(() => {
            form.setFieldsValue(initialValues);
            isInitializingRef.current = false;
          }, 0);
          return () => clearTimeout(timer);
        } else {
          isInitializingRef.current = false;
        }
      }
    } else {
      // Modal 关闭时，重置初始化标记
      isInitializingRef.current = false;
    }
  }, [modalOpen, editingRecord, form, resource]); // 移除 fieldOptions 作为依赖项，避免无限循环

  // 获取表单字段标签的翻译键
  const getFieldLabel = (resourceKey, fieldName, defaultLabel) => {
    const translationKey = `resource.${resourceKey}.field.${fieldName}.label`;
    const translated = t(translationKey);
    // 如果翻译键存在且不是键本身，返回翻译；否则返回原始标签
    return translated !== translationKey ? translated : defaultLabel;
  };

  // 获取表单字段占位符的翻译键
  const getFieldPlaceholder = (resourceKey, fieldName, defaultPlaceholder) => {
    if (defaultPlaceholder) {
      const translationKey = `resource.${resourceKey}.field.${fieldName}.placeholder`;
      const translated = t(translationKey);
      return translated !== translationKey ? translated : defaultPlaceholder;
    }
    return defaultPlaceholder;
  };

  // 获取列标题的翻译键
  const getColumnTitle = (resourceKey, columnTitle, dataIndex) => {
    // 如果 columnTitle 已经是翻译键格式（以 resource. 开头），直接使用它
    if (typeof columnTitle === 'string' && columnTitle.startsWith('resource.')) {
      const translated = t(columnTitle);
      // 如果翻译键存在且不是键本身，返回翻译；否则返回原始标题
      return translated !== columnTitle ? translated : columnTitle;
    }
    
    // 处理数组类型的 dataIndex（如 ['patient', 'realName'] 或 ['parent', 'categoryName']）
    let translationKey;
    if (Array.isArray(dataIndex)) {
      // 对于嵌套的 dataIndex，使用完整路径作为键的一部分
      // 例如 ['patient', 'realName'] -> 'patientRealName'
      // 例如 ['doctor', 'name'] -> 'doctorName'
      // 例如 ['parent', 'categoryName'] -> 'parentCategoryName'
      // 例如 ['operator', 'username'] -> 'operatorUsername'
      // 例如 ['category', 'categoryName'] -> 'categoryCategoryName'
      // 例如 ['drug', 'category', 'categoryName'] -> 'drugCategoryCategoryName'
      if (dataIndex.length > 1) {
        // 将整个路径转换为驼峰命名
        const camelCaseKey = dataIndex.map((key, index) => 
          index === 0 ? key : key.charAt(0).toUpperCase() + key.slice(1)
        ).join('');
        translationKey = `resource.${resourceKey}.column.${camelCaseKey}`;
      } else {
        translationKey = `resource.${resourceKey}.column.${dataIndex[0]}`;
      }
    } else {
      translationKey = `resource.${resourceKey}.column.${dataIndex}`;
    }
    const translated = t(translationKey);
    // 如果翻译键存在且不是键本身，返回翻译；否则返回原始标题
    return translated !== translationKey ? translated : columnTitle;
  };

  const columns = useMemo(() => {
    const baseColumns =
      resource.columns?.map((column) => {
        // 翻译列标题
        const translatedTitle = getColumnTitle(resource.key, column.title, column.dataIndex);
        
        // 对于健康知识的作者字段，需要将医生ID转换为医生姓名
        if (resource.key === 'knowledge' && column.dataIndex === 'author') {
          return {
            ...column,
            title: translatedTitle,
            render: (value, record) => {
              // 如果author是医生ID（Guid格式），从fieldOptions中查找医生姓名
              if (value && typeof value === 'string') {
                // 检查是否是Guid格式（包含连字符且长度合理）
                const isGuid = value.includes('-') && (value.length === 36 || value.length > 30);
                if (isGuid) {
                  // 从fieldOptions中查找医生姓名
                  const doctorOption = fieldOptions.author?.find(opt => {
                    // 比较时转换为字符串，确保类型一致
                    return String(opt.value) === String(value);
                  });
                  if (doctorOption) {
                    return doctorOption.label || value;
                  }
                  // 如果fieldOptions还没加载，显示加载中
                  if (fieldOptions.author && fieldOptions.author.length === 0) {
                    return value; // 暂时显示ID，等加载完成后会更新
                  }
                }
                // 如果不是Guid格式，可能是医生姓名（旧数据），直接显示
                return value;
              }
              // 如果值为空，显示占位符
              return value || '-';
            }
          };
        }
        return {
          ...column,
          title: translatedTitle,
          render:
            column.valueType === 'boolean'
              ? (value) => <Tag color={value ? 'green' : 'red'}>{value ? t('common.yes') : t('common.no')}</Tag>
              : column.render
        };
      }) ?? [];

    baseColumns.push({
      title: t('resource.operation'),
      key: 'actions',
      width: resource.key === 'consultation-messages' ? 220 : (resource.key === 'patient-support-groups' ? 140 : 160),
      fixed: 'right',
      render: (_, record) => (
        <Space>
          {/* 咨询消息查看入口：从咨询消息列表进入会话页面 */}
          {resource.key === 'consultation-messages' && record.consultationId && (
            <Button
              type="link"
              icon={<MessageOutlined />}
              onClick={() => navigate(`/consultations/messages/${record.consultationId}`)}
            >
              {t('resource.viewMessages')}
            </Button>
          )}
          {resource.key === 'carts' && (
            <Button 
              danger 
              type="link" 
              onClick={() => handleClearCart(record)}
            >
              {t('resource.carts.clear')}
            </Button>
          )}
          {!isReadOnly && (!resource.formFields || resource.formFields.length > 0) && (
            <Button type="link" onClick={() => openModal(record)}>
              {t('resource.edit')}
            </Button>
          )}
          {resource.key === 'patient-support-groups' ? null : !isReadOnly && (
            <Button danger type="link" onClick={() => handleDelete(record)}>
              {t('resource.delete')}
            </Button>
          )}
        </Space>
      )
    });

    return baseColumns;
  }, [resource.columns, resource.key, fieldOptions, t]);

  // 资源标题（用于弹窗标题等）
  const translatedResourceTitle =
    t(`resource.${resource.key}.title`) !== `resource.${resource.key}.title`
      ? t(`resource.${resource.key}.title`)
      : resource.title;

  return (
    <div className="resource-page">
        <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <Title level={4} style={{ margin: 0 }}>{t(`resource.${resource.key}.title`) || resource.title}</Title>
          <Space>
            <Space.Compact style={{ width: 300 }}>
              <Input
                placeholder={t('resource.searchPlaceholder')}
                allowClear
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
                onPressEnter={(e) => handleSearch(e.target.value)}
              />
              <Button 
                type="primary" 
                icon={<SearchOutlined />}
                onClick={() => handleSearch(searchKeyword)}
              >
                {t('resource.search')}
              </Button>
            </Space.Compact>
            {!isReadOnly && !resource.disableCreate && (!resource.formFields || resource.formFields.length > 0) && (
              <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal(null)}>
                {t('resource.add')}
              </Button>
            )}
          </Space>
        </div>
        <Table
          rowKey={resource.primaryKey}
          columns={columns}
          dataSource={data}
          loading={loading}
          pagination={{
            current: pagination.current,
            pageSize: pagination.pageSize,
            total: pagination.total,
            showSizeChanger: true,
            showTotal: (total) => t('resource.total', { count: total }),
            pageSizeOptions: ['10', '20', '50', '100'],
            onChange: (current, pageSize) => {
              setPagination((prev) => ({ ...prev, current, pageSize }));
              fetchData(current, pageSize, searchKeyword);
            },
            onShowSizeChange: (current, pageSize) => {
              setPagination((prev) => ({ ...prev, current, pageSize }));
              fetchData(current, pageSize, searchKeyword);
            }
          }}
          scroll={{ x: 'max-content' }}
        />

      <Modal
        title={
          editingRecord
            ? `${t('resource.edit')}${translatedResourceTitle}`
            : `${t('resource.add')}${translatedResourceTitle}`
        }
        open={modalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setModalOpen(false);
          setEditingRecord(null);
          form.resetFields(); // 关闭时重置表单
        }}
        afterClose={() => {
          form.resetFields(); // Modal 完全关闭后重置表单
        }}
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width={900}
      >
        {modalOpen && (
          <Form layout="vertical" form={form} preserve={false}>
          <Row gutter={16}>
          {resource.formFields?.map((field) => {
              const { component = 'input', options = [], componentProps: originalComponentProps = {}, loadOptionsFrom, span = 12 } = field;
              
              // 调试：打印所有字段信息
              if (resource.key === 'system-users') {
                console.log('[表单渲染] 字段:', field.name, '组件:', component, '完整字段配置:', field, 'editingRecord:', !!editingRecord);
              }
              
              // 确保密码字段在编辑时也显示（不进行任何过滤）
              // 密码字段应该始终显示，无论是新建还是编辑模式
              
            const Component = componentMap[component] ?? Input;
            const isSelect = component === 'select';
              const isUpload = component === 'upload';
              
              // 对于咨询消息的附件上传，根据消息类型动态设置上传端点和 accept
              let uploadEndpoint = '/api/upload/image'; // 默认图片上传
              let uploadAccept = originalComponentProps.accept || 'image/*';
              let uploadCategory = null; // 上传分类参数
              
              if (isUpload && resource.key === 'consultation-messages') {
                // 使用 Form.useWatch 监听的消息类型值
                if (messageType === 'Image') {
                  uploadEndpoint = '/api/upload/image';
                  uploadAccept = 'image/*';
                } else if (messageType === 'Video') {
                  uploadEndpoint = '/api/upload/video';
                  uploadAccept = 'video/*';
                } else if (messageType === 'Voice') {
                  uploadEndpoint = '/api/upload/audio';
                  uploadAccept = 'audio/*';
                } else if (messageType === 'File') {
                  uploadEndpoint = '/api/upload/file';
                  uploadAccept = '*/*';
                } else {
                  // Text 类型或其他，默认使用图片上传
                  uploadEndpoint = '/api/upload/image';
                  uploadAccept = 'image/*';
                }
              } else if (isUpload) {
                // 其他资源的上传，使用默认的图片上传
                uploadEndpoint = '/api/upload/image';
                uploadAccept = originalComponentProps.accept || 'image/*';
                
                // 根据资源类型和字段名称确定上传分类
                if (resource.key === 'activities' && field.name === 'coverImageUrl') {
                  uploadCategory = 'activity';
                } else if (resource.key === 'system-users' && field.name === 'avatarUrl') {
                  uploadCategory = 'user';
                } else if (resource.key === 'knowledge' && field.name === 'coverImageUrl') {
                  uploadCategory = 'healthknowledge';
                } else if (resource.key === 'doctors' && field.name === 'avatarUrl') { // 新增医生头像分类
                  uploadCategory = 'doctor';
                } else if (resource.key === 'products' && field.name === 'coverUrl') { // 商品图片分类
                  uploadCategory = 'product';
                }
              }
              
              // 如果字段有动态加载的选项，使用动态选项，否则使用静态选项
              // 对于三甲医院的城市字段，始终使用 fieldOptions 中的选项（通过 useEffect 动态加载）
              let selectOptions = options;
              if (loadOptionsFrom) {
                selectOptions = fieldOptions[field.name] || [];
              } else if (resource.key === 'tertiary-hospitals' && field.name === 'cityId') {
                // 城市字段没有 loadOptionsFrom，但通过 useEffect 动态加载到 fieldOptions
                selectOptions = fieldOptions[field.name] || [];
              }
              
              // 对于三甲医院的城市字段，强制不禁用（新建和编辑模式下都可用）
              let componentProps = originalComponentProps;
              if (resource.key === 'tertiary-hospitals' && field.name === 'cityId') {
                // 城市字段始终不禁用，强制设置为 false（即使配置中有 disabled: true）
                componentProps = {
                  ...originalComponentProps,
                  disabled: false // 强制不禁用
                };
                console.log('[cityId字段] 城市字段禁用状态:', componentProps.disabled);
              }
              
              // 对于三甲医院的省份字段，添加 onChange 处理，直接加载城市列表
              if (resource.key === 'tertiary-hospitals' && field.name === 'provinceId') {
                const originalOnChange = originalComponentProps.onChange;
                componentProps = {
                  ...originalComponentProps,
                  onChange: async (value) => {
                    console.log('[provinceId onChange] 省份选择变化，新值:', value);
                    // 更新 provinceId 状态（用于控制城市列表加载）
                    setProvinceId(value);
                    // 重置 prevProvinceIdRef，强制重新加载城市列表
                    prevProvinceIdRef.current = null;
                    // 清空城市字段的值
                    form.setFieldValue('cityId', undefined);
                    // 直接加载城市列表（新建和编辑模式下都清空城市值）
                    const cityOptions = await loadCitiesByProvince(value, true);
                    console.log('[provinceId onChange] 城市列表加载完成，选项数量:', cityOptions.length);
                    
                    // 新建和编辑模式下，都默认选择第一个城市
                    if (cityOptions.length > 0) {
                      console.log('[provinceId onChange] 默认选择第一个城市:', cityOptions[0].value);
                      form.setFieldValue('cityId', cityOptions[0].value);
                    }
                    
                    // 调用原始的 onChange（如果有）
                    if (originalOnChange) {
                      originalOnChange(value);
                    }
                  }
                };
              }
              
              // 调试：parentId字段的选项
              if (field.name === 'parentId') {
                console.log('[parentId字段] selectOptions:', selectOptions);
                console.log('[parentId字段] fieldOptions[parentId]:', fieldOptions[field.name]);
              }

              // Upload 组件使用 fileList，其他组件使用 value 或 checked
              const valuePropName = isUpload ? 'fileList' : (component === 'switch' ? 'checked' : 'value');
              
              // 处理上传组件的值转换
              const getValueFromEvent = isUpload ? (e) => {
                if (Array.isArray(e)) {
                  return e;
                }
                return e?.fileList || [];
              } : undefined;
              
              const normalizeValue = isUpload ? (value) => {
                if (!value) return [];
                // 如果已经是数组，直接返回（可能是从 openModal 设置的）
                if (Array.isArray(value)) {
                  // 确保数组中的每个文件对象都有正确的 URL 和 response
                  return value.map(file => {
                    // 确保保留所有原有属性，特别是 response
                    const fileObj = { ...file };
                    
                    // 如果 url 是相对路径，构建完整 URL 用于预览
                    if (fileObj.url && !fileObj.url.startsWith('http') && fileObj.url.startsWith('/')) {
                      const fullUrl = `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${fileObj.url}`;
                      fileObj.url = fullUrl;
                      fileObj.thumbUrl = fullUrl;
                    }
                    
                    // 确保 response 存在（用于提交时提取路径）
                    if (!fileObj.response && fileObj.url) {
                      // 如果 url 是完整 URL，提取相对路径
                      let originalPath = fileObj.url;
                      if (originalPath.startsWith('http')) {
                        try {
                          const urlObj = new URL(originalPath);
                          originalPath = urlObj.pathname;
                        } catch (e) {
                          // 如果解析失败，保持原样
                        }
                      }
                      fileObj.response = { url: originalPath, path: originalPath };
                    }
                    
                    return fileObj;
                  });
                }
                if (typeof value === 'string') {
                  // 如果是字符串URL（服务器路径），转换为文件列表格式
                  // 构建完整的图片 URL（如果后端返回的是相对路径）
                  const originalPath = value;
                  const imageUrl = value.startsWith('http') ? value : 
                    (value.startsWith('/') ? `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${value}` : value);
                  return [{
                    uid: '-1',
                    name: 'image',
                    status: 'done',
                    url: imageUrl,
                    thumbUrl: imageUrl,
                    response: { url: originalPath, path: originalPath } // 保存原始路径
                  }];
                }
                return [];
              } : undefined;

            // 确保密码字段始终显示（调试日志）
            if (resource.key === 'system-users' && field.name === 'password') {
              console.log('[表单渲染] 渲染密码字段，editingRecord:', !!editingRecord, 'field:', field);
            }
            
            // BMI 字段需要监听身高和体重的变化
            const isBMIField = resource.key === 'patient-info' && field.name === 'bmi';
            
            // 获取翻译后的标签和占位符
            const translatedLabel = getFieldLabel(resource.key, field.name, field.label);
            const translatedPlaceholder = getFieldPlaceholder(resource.key, field.name, field.placeholder || componentProps.placeholder);

            // 对于 patient-info 资源的 username 字段，编辑时设置为只读（不可编辑）
            let readOnly = componentProps.readOnly;
            let disabled = componentProps.disabled;
            if (resource.key === 'patient-info' && field.name === 'username' && editingRecord) {
              readOnly = true;
              disabled = true; // Ant Design Input 组件使用 disabled 属性
            }
            
            // 合并组件属性，确保占位符使用翻译后的文本覆盖原始占位符
            const mergedComponentProps = {
              ...componentProps,
              readOnly: readOnly,
              disabled: disabled,
              placeholder:
                translatedPlaceholder ||
                componentProps.placeholder ||
                (component === 'select'
                  ? t('common.pleaseSelect', { field: translatedLabel })
                  : t('common.pleaseEnter', { field: translatedLabel }))
            };
            
            // 处理 rules 中的 message 翻译
            const translatedRules = field.rules?.map(rule => {
              // 处理 validator 函数
              if (rule.validator) {
                const originalValidator = rule.validator;
                return {
                  ...rule,
                  validator: async (_, value) => {
                    try {
                      await originalValidator(_, value);
                      return Promise.resolve();
                    } catch (error) {
                      // 如果错误消息是翻译键，尝试翻译它
                      const errorMessage = error?.message || String(error);
                      const messageKey = `resource.${resource.key}.field.${field.name}.rule.pattern`;
                      const translatedMessage = t(messageKey);
                      const finalMessage = (errorMessage === messageKey || errorMessage.startsWith('resource.')) 
                        ? (translatedMessage !== messageKey ? translatedMessage : errorMessage)
                        : errorMessage;
                      return Promise.reject(new Error(finalMessage));
                    }
                  }
                };
              }
              
              if (rule.message) {
                // 尝试翻译 message
                // 优先级：required > pattern > max > message
                let messageKey;
                if (rule.required) {
                  messageKey = `resource.${resource.key}.field.${field.name}.rule.required`;
                } else if (rule.pattern) {
                  messageKey = `resource.${resource.key}.field.${field.name}.rule.pattern`;
                } else if (rule.max !== undefined) {
                  messageKey = `resource.${resource.key}.field.${field.name}.rule.max`;
                } else {
                  messageKey = `resource.${resource.key}.field.${field.name}.rule.message`;
                }
                const translatedMessage = t(messageKey);
                return {
                  ...rule,
                  message: translatedMessage !== messageKey ? translatedMessage : rule.message
                };
              }
              return rule;
            });

            return (
                <Col span={span} key={field.name}>
              <Form.Item
                name={field.name}
                label={translatedLabel}
                rules={translatedRules || field.rules}
                valuePropName={valuePropName}
                    getValueFromEvent={getValueFromEvent}
                    normalize={normalizeValue}
                    dependencies={
                      isBMIField 
                        ? ['height', 'weight'] 
                        : (resource.key === 'tertiary-hospitals' && field.name === 'cityId' ? ['provinceId'] : undefined)
                    }
                    shouldUpdate={(prevValues, currentValues) => {
                      // BMI 字段需要监听身高和体重的变化
                      if (isBMIField) {
                        const prevHeight = prevValues.height;
                        const prevWeight = prevValues.weight;
                        const currentHeight = currentValues.height;
                        const currentWeight = currentValues.weight;
                        return prevHeight !== currentHeight || prevWeight !== currentWeight;
                      }
                      // 三甲医院的城市字段需要监听省份变化，以更新禁用状态
                      if (resource.key === 'tertiary-hospitals' && field.name === 'cityId') {
                        const prevProvinceId = prevValues.provinceId;
                        const currentProvinceId = currentValues.provinceId;
                        return prevProvinceId !== currentProvinceId;
                      }
                      return false;
                    }}
              >
                {isSelect ? (
                      <Select 
                        options={selectOptions} 
                        showSearch
                        allowClear
                        filterOption={(input, option) => {
                          const label = option?.label ?? '';
                          const value = option?.value ?? '';
                          const searchText = input.toLowerCase().trim();
                          if (!searchText) return true;
                          // 支持模糊查询：检查 label 或 value 是否包含搜索文本
                          return label.toLowerCase().includes(searchText) || 
                                 String(value).toLowerCase().includes(searchText);
                        }}
                        {...mergedComponentProps} 
                      />
                    ) : isUpload ? (
                      <Upload
                        key={resource.key === 'consultation-messages' ? `upload-${messageType || 'default'}` : `upload-${field.name}`}
                        listType={mergedComponentProps.listType || 'text'}
                        maxCount={mergedComponentProps.maxCount || 1}
                        accept={uploadAccept}
                        customRequest={async ({ file, onSuccess, onError, onProgress }) => {
                          console.log(`[${field.name}] customRequest 被调用:`, {
                            fileName: file.name,
                            fileSize: file.size,
                            fileType: file.type,
                            fileUid: file.uid,
                            uploadEndpoint
                          });
                          
                          try {
                            const formData = new FormData();
                            formData.append('file', file);
                            
                            // 使用 httpClient 上传文件
                            const token = window.localStorage.getItem('medical-admin-token');
                            const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
                            
                            // 构建上传URL，添加category查询参数
                            let uploadUrl = uploadEndpoint;
                            if (uploadCategory) {
                              uploadUrl += `?category=${uploadCategory}`;
                            }
                            
                            console.log(`[${field.name}] 开始上传到: ${baseURL}${uploadUrl}`, { category: uploadCategory });
                            
                            const xhr = new XMLHttpRequest();
                            
                            // 上传进度
                            xhr.upload.addEventListener('progress', (e) => {
                              if (e.lengthComputable) {
                                const percent = Math.round((e.loaded / e.total) * 100);
                                console.log(`[${field.name}] 上传进度: ${percent}%`);
                                onProgress?.({ percent });
                              }
                            });
                            
                            // 上传成功
                            xhr.addEventListener('load', () => {
                              console.log(`[${field.name}] xhr.load 事件触发，状态码: ${xhr.status}`);
                              if (xhr.status === 200) {
                                const response = JSON.parse(xhr.responseText);
                                let imageUrl = response.url || response.path;
                                
                                // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                                if (typeof imageUrl === 'string') {
                                  imageUrl = imageUrl.replace(/\\/g, '/');
                                }
                                
                                console.log(`[${field.name}] 图片上传成功:`, {
                                  response,
                                  imageUrl,
                                  fileName: response.fileName
                                });
                                
                                // 更新文件列表，设置上传后的 URL
                                const currentFileList = form.getFieldValue(field.name) || [];
                                console.log(`[${field.name}] 当前文件列表:`, currentFileList);
                                
                                // 构建完整 URL 用于预览（如果是相对路径）
                                const previewUrl = imageUrl.startsWith('http') ? imageUrl : 
                                  (imageUrl.startsWith('/') ? `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${imageUrl}` : imageUrl);
                                
                                const updatedFileList = currentFileList.map(f => {
                                  if (f.uid === file.uid) {
                                    const updated = { 
                                      ...f, 
                                      status: 'done', 
                                      url: previewUrl, 
                                      thumbUrl: previewUrl, 
                                      response: response // 保存完整响应，包含原始路径（相对路径）
                                    };
                                    console.log(`[${field.name}] 更新文件对象:`, {
                                      uid: updated.uid,
                                      status: updated.status,
                                      url: updated.url,
                                      response: updated.response
                                    });
                                    return updated;
                                  }
                                  return f;
                                });
                                
                                console.log(`[${field.name}] 更新后的文件列表:`, updatedFileList);
                                // 先更新表单值
                                form.setFieldsValue({ [field.name]: updatedFileList });
                                // 然后调用 onSuccess，让 Upload 组件知道上传完成
                                onSuccess?.(response, xhr);
                                // 再次确保文件列表已更新（防止异步问题）
                                setTimeout(() => {
                                  const finalFileList = form.getFieldValue(field.name) || [];
                                  const finalUpdatedList = finalFileList.map(f => {
                                    if (f.uid === file.uid && f.status !== 'done') {
                                      return {
                                        ...f,
                                        status: 'done',
                                        url: previewUrl,
                                        thumbUrl: previewUrl,
                                        response: response
                                      };
                                    }
                                    return f;
                                  });
                                  if (JSON.stringify(finalFileList) !== JSON.stringify(finalUpdatedList)) {
                                    form.setFieldsValue({ [field.name]: finalUpdatedList });
                                  }
                                }, 100);
                                messageApi.success(t('resource.uploadSuccess'));
                              } else {
                                const error = JSON.parse(xhr.responseText || '{}');
                                console.error(`[${field.name}] 图片上传失败:`, {
                                  status: xhr.status,
                                  statusText: xhr.statusText,
                                  responseText: xhr.responseText,
                                  error
                                });
                                onError?.(new Error(error.message || t('resource.uploadFailed')));
                                messageApi.error(error.message || t('resource.uploadFailed'));
                              }
                            });
                            
                            // 上传错误
                            xhr.addEventListener('error', (e) => {
                              console.error(`[${field.name}] xhr.error 事件触发:`, e);
                              onError?.(new Error(t('resource.uploadFailed')));
                              messageApi.error(t('resource.uploadFailed'));
                            });
                            
                            // 上传中止
                            xhr.addEventListener('abort', () => {
                              console.warn(`[${field.name}] 上传被中止`);
                            });
                            
                            // 发送请求
                            xhr.open('POST', `${baseURL}${uploadUrl}`);
                            if (token) {
                              xhr.setRequestHeader('Authorization', `Bearer ${token}`);
                            }
                            console.log(`[${field.name}] 发送上传请求到: ${baseURL}${uploadUrl}...`);
                            xhr.send(formData);
                          } catch (err) {
                            console.error(`[${field.name}] customRequest 异常:`, err);
                            onError?.(err);
                            messageApi.error(t('resource.uploadFailed'));
                          }
                        }}
                        onChange={(info) => {
                          const { fileList, file } = info;
                          console.log(`[${field.name}] onChange 被调用:`, {
                            fileListLength: fileList.length,
                            currentFile: file ? {
                              uid: file.uid,
                              name: file.name,
                              status: file.status,
                              hasResponse: !!file.response,
                              hasUrl: !!file.url,
                              hasOriginFileObj: !!file.originFileObj
                            } : null,
                            fileList: fileList.map(f => ({
                              uid: f.uid,
                              name: f.name,
                              status: f.status,
                              hasResponse: !!f.response,
                              hasUrl: !!f.url,
                              hasOriginFileObj: !!f.originFileObj
                            }))
                          });
                          
                          // 限制文件数量
                          let newFileList = fileList.slice(-(componentProps.maxCount || 1));
                          
                          // 处理文件列表
                          newFileList = newFileList.map(fileItem => {
                            // 如果文件已上传成功，使用返回的 URL
                            if (fileItem.status === 'done') {
                              // 优先使用已有的 url 或 thumbUrl
                              let imageUrl = fileItem.url || fileItem.thumbUrl;
                              
                              // 如果没有 url，尝试从 response 中获取
                              if (!imageUrl && fileItem.response) {
                                imageUrl = fileItem.response.url || fileItem.response.path;
                              }
                              
                              // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                              if (imageUrl && typeof imageUrl === 'string') {
                                imageUrl = imageUrl.replace(/\\/g, '/');
                              }
                              
                              // 构建完整 URL 用于预览（如果是相对路径）
                              const previewUrl = imageUrl && imageUrl.startsWith('http') ? imageUrl : 
                                (imageUrl && imageUrl.startsWith('/') ? `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${imageUrl}` : imageUrl);
                              
                              return {
                                ...fileItem,
                                status: 'done',
                                url: previewUrl || fileItem.url,
                                thumbUrl: previewUrl || fileItem.thumbUrl,
                                // 确保 response 存在，用于提交时提取路径
                                response: fileItem.response || { url: imageUrl, path: imageUrl }
                              };
                            }
                            // 如果文件正在上传或上传中，保持原样
                            if (fileItem.status === 'uploading' || fileItem.status === 'error') {
                              return fileItem;
                            }
                            // 如果是新选择的文件（有 originFileObj），需要手动触发上传
                            if (fileItem.originFileObj && !fileItem.status && fileItem.status !== 'uploading') {
                              console.log(`[${field.name}] onChange: 检测到新文件，手动触发上传`, fileItem.uid);
                              // 手动触发上传
                              const uploadFile = fileItem.originFileObj || fileItem;
                              const formData = new FormData();
                              formData.append('file', uploadFile);
                              
                              const token = window.localStorage.getItem('medical-admin-token');
                              const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
                              
                              // 构建上传URL，添加category查询参数
                              let uploadUrl = '/api/upload/image';
                              if (uploadCategory) {
                                uploadUrl += `?category=${uploadCategory}`;
                              }
                              
                              console.log(`[${field.name}] 手动上传到: ${baseURL}${uploadUrl}`, { category: uploadCategory });
                              
                              const xhr = new XMLHttpRequest();
                              
                              // 设置状态为上传中
                              const uploadingFile = {
                                ...fileItem,
                                status: 'uploading',
                                percent: 0
                              };
                              const tempFileList = newFileList.map(f => f.uid === fileItem.uid ? uploadingFile : f);
                              form.setFieldsValue({ [field.name]: tempFileList });
                              
                              // 上传进度
                              xhr.upload.addEventListener('progress', (e) => {
                                if (e.lengthComputable) {
                                  const percent = Math.round((e.loaded / e.total) * 100);
                                  console.log(`[${field.name}] 上传进度: ${percent}%`);
                                  const progressFileList = form.getFieldValue(field.name) || [];
                                  const updatedProgressList = progressFileList.map(f => 
                                    f.uid === fileItem.uid ? { ...f, percent } : f
                                  );
                                  form.setFieldsValue({ [field.name]: updatedProgressList });
                                }
                              });
                              
                              // 上传成功
                              xhr.addEventListener('load', () => {
                                console.log(`[${field.name}] xhr.load 事件触发，状态码: ${xhr.status}`);
                                if (xhr.status === 200) {
                                  const response = JSON.parse(xhr.responseText);
                                  let imageUrl = response.url || response.path;
                                  
                                  // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
                                  if (typeof imageUrl === 'string') {
                                    imageUrl = imageUrl.replace(/\\/g, '/');
                                  }
                                  
                                  console.log(`[${field.name}] 图片上传成功:`, {
                                    response,
                                    imageUrl,
                                    fileName: response.fileName
                                  });
                                  
                                  // 构建完整 URL 用于预览（如果是相对路径）
                                  const previewUrl = imageUrl.startsWith('http') ? imageUrl : 
                                    (imageUrl.startsWith('/') ? `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${imageUrl}` : imageUrl);
                                  
                                  const currentFileList = form.getFieldValue(field.name) || [];
                                  const updatedFileList = currentFileList.map(f => 
                                    f.uid === fileItem.uid 
                                      ? { 
                                          ...f, 
                                          status: 'done', 
                                          url: previewUrl, 
                                          thumbUrl: previewUrl, 
                                          response: response // 保存完整响应，包含原始路径（相对路径）
                                        }
                                      : f
                                  );
                                  
                                  console.log(`[${field.name}] 更新后的文件列表:`, updatedFileList);
                                  // 先更新表单值
                                  form.setFieldsValue({ [field.name]: updatedFileList });
                                  // 再次确保文件列表已更新（防止异步问题）
                                  setTimeout(() => {
                                    const finalFileList = form.getFieldValue(field.name) || [];
                                    const finalUpdatedList = finalFileList.map(f => {
                                      if (f.uid === fileItem.uid && f.status !== 'done') {
                                        return {
                                          ...f,
                                          status: 'done',
                                          url: previewUrl,
                                          thumbUrl: previewUrl,
                                          response: response
                                        };
                                      }
                                      return f;
                                    });
                                    if (JSON.stringify(finalFileList) !== JSON.stringify(finalUpdatedList)) {
                                      form.setFieldsValue({ [field.name]: finalUpdatedList });
                                    }
                                  }, 100);
                                  messageApi.success(t('resource.uploadSuccess'));
                                } else {
                                  const error = JSON.parse(xhr.responseText || '{}');
                                  console.error(`[${field.name}] 图片上传失败:`, {
                                    status: xhr.status,
                                    statusText: xhr.statusText,
                                    responseText: xhr.responseText,
                                    error
                                  });
                                  const currentFileList = form.getFieldValue(field.name) || [];
                                  const updatedFileList = currentFileList.map(f => 
                                    f.uid === fileItem.uid 
                                      ? { ...f, status: 'error' }
                                      : f
                                  );
                                  form.setFieldsValue({ [field.name]: updatedFileList });
                                  messageApi.error(error.message || t('resource.uploadFailed'));
                                }
                              });
                              
                              // 上传错误
                              xhr.addEventListener('error', (e) => {
                                console.error(`[${field.name}] xhr.error 事件触发:`, e);
                                const currentFileList = form.getFieldValue(field.name) || [];
                                const updatedFileList = currentFileList.map(f => 
                                  f.uid === fileItem.uid 
                                    ? { ...f, status: 'error' }
                                    : f
                                );
                                form.setFieldsValue({ [field.name]: updatedFileList });
                                messageApi.error(t('resource.uploadFailed'));
                              });
                              
                              // 发送请求
                              xhr.open('POST', `${baseURL}${uploadUrl}`);
                              if (token) {
                                xhr.setRequestHeader('Authorization', `Bearer ${token}`);
                              }
                              console.log(`[${field.name}] 发送上传请求到: ${baseURL}${uploadUrl}...`);
                              xhr.send(formData);
                              
                              // 返回上传中的文件对象
                              return uploadingFile;
                            }
                            return fileItem;
                          });
                          
                          // 更新表单值（只有在没有手动触发上传的情况下）
                          const hasNewFile = newFileList.some(f => f.originFileObj && !f.status && f.status !== 'uploading');
                          if (!hasNewFile) {
                            form.setFieldsValue({ [field.name]: newFileList });
                          }
                        }}
                        onRemove={() => {
                          form.setFieldsValue({ [field.name]: [] });
                        }}
                        beforeUpload={(file) => {
                          // 验证文件类型
                          const isImage = file.type.startsWith('image/');
                          if (!isImage) {
                            messageApi.error('只能上传图片文件');
                            return Upload.LIST_IGNORE; // 阻止添加到列表
                          }
                          // 验证文件大小（5MB）
                          const isLt5M = file.size / 1024 / 1024 < 5;
                          if (!isLt5M) {
                            messageApi.error('图片大小不能超过 5MB');
                            return Upload.LIST_IGNORE; // 阻止添加到列表
                          }
                          // 返回 false 阻止自动上传，使用 customRequest
                          // 文件会被添加到列表，但不会自动上传，等待 customRequest 处理
                          return false;
                        }}
                      >
                        {((mergedComponentProps.listType === 'picture-card' || mergedComponentProps.listType === 'picture') && 
                          (!form.getFieldValue(field.name) || form.getFieldValue(field.name)?.length < (mergedComponentProps.maxCount || 1))) && (
                          <div>
                            <PlusOutlined />
                            <div style={{ marginTop: 8 }}>{t('resource.upload')}</div>
                          </div>
                        )}
                      </Upload>
                ) : (
                  // 对于密码字段，确保使用 Input.Password 组件
                  component === 'password' ? (
                    <Input.Password 
                      {...mergedComponentProps} 
                      placeholder={translatedPlaceholder || t('common.pleaseEnter', { field: translatedLabel })} 
                      allowClear
                    />
                  ) : (
                    // BMI 自动计算：为身高和体重字段添加 onChange 事件
                    (resource.key === 'patient-info' && (field.name === 'height' || field.name === 'weight')) ? (
                      <Component 
                        {...mergedComponentProps}
                        onChange={(value) => {
                          // 调用原有的 onChange（如果有）
                          if (mergedComponentProps.onChange) {
                            mergedComponentProps.onChange(value);
                          }
                          
                          // 自动计算 BMI
                          const height = field.name === 'height' ? value : form.getFieldValue('height');
                          const weight = field.name === 'weight' ? value : form.getFieldValue('weight');
                          
                          if (height && weight && height > 0) {
                            // BMI = 体重(kg) / 身高(m)²
                            const heightInMeters = height / 100; // 转换为米
                            const bmi = weight / (heightInMeters * heightInMeters);
                            form.setFieldsValue({ bmi: Number(bmi.toFixed(2)) });
                          } else {
                            form.setFieldsValue({ bmi: null });
                          }
                        }}
                      />
                    ) : (
                      <Component 
                        {...mergedComponentProps} 
                        placeholder={translatedPlaceholder || componentProps.placeholder || t('common.pleaseEnter', { field: translatedLabel })}
                      />
                    )
                  )
                )}
              </Form.Item>
                </Col>
            );
          })}
          </Row>
        </Form>
        )}
      </Modal>
    </div>
  );
}

// 外部组件，用 App 包裹
const ResourcePage = ({ resource }) => {
  return (
    <App>
      <ResourcePageContent resource={resource} />
    </App>
  );
};

export default ResourcePage;

