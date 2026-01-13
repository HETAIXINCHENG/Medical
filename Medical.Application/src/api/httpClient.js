import axios from 'axios';

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  timeout: 15000
});

httpClient.interceptors.request.use((config) => {
  const token = window.localStorage.getItem('medical-admin-token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  } else {
    // 如果没有token且不在登录页，记录警告
    if (window.location.pathname !== '/login') {
      console.warn('[httpClient] 未找到认证token，请求可能失败');
    }
  }
  return config;
});

httpClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    // 处理401未授权错误
    if (error.response?.status === 401) {
      // 清除token
      window.localStorage.removeItem('medical-admin-token');
      // 重定向到登录页
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    
    // 保留完整的错误信息，包括 response 对象
    const errorMessage = error.response?.data?.message || error.message || '请求失败';
    const enhancedError = new Error(errorMessage);
    enhancedError.response = error.response;
    enhancedError.status = error.response?.status;
    return Promise.reject(enhancedError);
  }
);

export default httpClient;

