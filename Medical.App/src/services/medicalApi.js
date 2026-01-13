import apiClient from './apiClient.js';

/**
 * 面向页面的业务 API 封装，集中列出所有可用的后台接口。
 */
export const medicalApi = {
  getActivities: (params) => apiClient.get('/api/activities', { params }),
  getDoctors: (params) => apiClient.get('/api/doctors', { params }),
  searchDoctors: (keyword, params) =>
    apiClient.get('/api/doctors/search', { params: { keyword, ...(params ?? {}) } }),
  getDoctorById: (doctorId) => apiClient.get(`/api/doctors/${doctorId}`),
  getDepartments: (params) => apiClient.get('/api/departments', { params }),
  getDiseaseCategories: (params) => apiClient.get('/api/diseasecategories', { params }),
  getDepartmentDiseases: (departmentId) => apiClient.get(`/api/departments/${departmentId}/diseases`),
  getHealthKnowledge: (params) => apiClient.get('/api/healthknowledge', { params }),
  getHealthKnowledgeById: (id) => apiClient.get(`/api/healthknowledge/${id}`),
  getMyFavorites: (params) => apiClient.get('/api/healthknowledge/my-favorites', { params }),
  searchHealthKnowledge: (keyword, params) =>
    apiClient.get('/api/healthknowledge/search', { params: { keyword, ...(params ?? {}) } }),
  getQuestions: (params) => apiClient.get('/api/questions', { params }),
  getConsultations: (params) => apiClient.get('/api/consultations', { params }),
  login: (payload) => apiClient.post('/api/auth/login', payload),
  register: (payload) => apiClient.post('/api/auth/register', payload),
  getCurrentUser: () => apiClient.get('/api/users/me'),
  getMyPatients: () => apiClient.get('/api/patients/my-patients'),
  addFamilyMember: (member) => apiClient.post('/api/users/me/family-members', member),
  // 病历相关API
  getMedicalRecords: (params) => apiClient.get('/api/medicalrecords', { params }),
  getMedicalRecordById: (id) => apiClient.get(`/api/medicalrecords/${id}`),
  createMedicalRecord: (data) => apiClient.post('/api/medicalrecords', data),
  updateMedicalRecord: (id, data) => apiClient.put(`/api/medicalrecords/${id}`, data),
  deleteMedicalRecord: (id) => apiClient.delete(`/api/medicalrecords/${id}`),
  // 我的医生相关API
  getMyConsultedDoctors: () => apiClient.get('/api/doctors/my-consulted-doctors'),
  // 患友会相关API
  getPatientSupportGroupByDoctor: (doctorId) => apiClient.get(`/api/patientsupportgroups/by-doctor/${doctorId}`),
  getPosts: (groupId, params) => apiClient.get(`/api/patientsupportgroups/${groupId}/posts`, { params }),
  getPostById: (postId) => apiClient.get(`/api/patientsupportgroups/posts/${postId}`),
  createPost: (groupId, data) => apiClient.post(`/api/patientsupportgroups/${groupId}/posts`, data),
  getGroupRules: (groupId) => apiClient.get(`/api/patientsupportgroups/${groupId}/rules`),
  incrementPostReadCount: (postId) => apiClient.post(`/api/patientsupportgroups/posts/${postId}/read`),
  togglePostLike: (postId) => apiClient.post(`/api/patientsupportgroups/posts/${postId}/like`),
  uploadPostAttachment: (file) => apiClient.upload(`/api/patientsupportgroups/posts/upload`, file),
  // 评论相关API
  getPostComments: (postId) => apiClient.get(`/api/patientsupportgroups/posts/${postId}/comments`),
  createPostComment: (postId, data) => apiClient.post(`/api/patientsupportgroups/posts/${postId}/comments`, data),
  uploadCommentAttachment: (file) => apiClient.upload(`/api/patientsupportgroups/posts/comments/upload`, file),
  // 反馈相关API
  submitFeedback: (data) => apiClient.post('/api/feedbacks', data),
  // 三甲医院相关API
  getTertiaryHospitals: (params) => apiClient.get('/api/tertiaryhospitals', { params }),
  // 省份相关API
  getProvinces: () => apiClient.get('/api/provinces'),
  getCitiesByProvince: (provinceId) => apiClient.get(`/api/provinces/${provinceId}/cities`),
  // AI预诊相关API（设置120秒超时，因为AI响应可能需要更长时间）
  aiDiagnosis: (data) => apiClient.post('/api/aidiagnosis/chat', data, { timeout: 120000 }),
  // 商品相关API
  getProducts: (params) => apiClient.get('/api/products', { params }),
  // IP定位相关API
  getProvinceByIp: () => apiClient.get('/api/iplocation/province'),
  // 微信分享相关API
  getWeChatSignature: (url) => apiClient.get('/api/wechat/signature', { params: { url } })
};

export default medicalApi;

