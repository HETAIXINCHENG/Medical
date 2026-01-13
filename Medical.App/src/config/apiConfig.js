const DEFAULT_API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

/**
 * 获取格式化后的 API 根地址，确保末尾没有多余的 /。
 */
export const getApiBaseUrl = () => DEFAULT_API_BASE_URL.replace(/\/$/, '');

/**
 * 统一的接口超时时间（毫秒）。
 */
export const API_TIMEOUT = 15000;

/**
 * 获取本地保存的 JWT Token，用于需要鉴权的请求。
 */
export const getAuthToken = () => {
  return window.localStorage.getItem('medical-jwt') || window.localStorage.getItem('token') || '';
};

/**
 * 将 Token 写入本地，方便其他页面复用。
 */
export const setAuthToken = (token) => {
  if (!token) return;
  window.localStorage.setItem('medical-jwt', token);
};

