import { getApiBaseUrl, API_TIMEOUT, getAuthToken } from '../config/apiConfig.js';

/**
 * 通用的网络请求封装，统一处理查询参数、请求头、超时与错误提示。
 */
async function apiRequest(path, { method = 'GET', params, body, headers, timeout } = {}) {
  const controller = new AbortController();
  const requestTimeout = timeout ?? API_TIMEOUT;
  const timeoutId = window.setTimeout(() => controller.abort(), requestTimeout);

  try {
    const url = buildUrl(path, params);
    const token = getAuthToken();
    const finalHeaders = {
      'Content-Type': 'application/json',
      ...(headers ?? {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    };

    const response = await fetch(url, {
      method,
      headers: finalHeaders,
      body: body ? JSON.stringify(body) : undefined,
      signal: controller.signal
    });

    if (!response.ok) {
      // 401 未授权，清除 token 并跳转到登录页
      if (response.status === 401) {
        window.localStorage.removeItem('medical-jwt');
        window.localStorage.removeItem('token');
        window.localStorage.removeItem('user');
        // 获取当前路径，用于登录后跳转回来
        const currentPath = window.location.pathname + window.location.search;
        window.location.href = `/login?from=${encodeURIComponent(currentPath)}`;
        return;
      }

      const errorPayload = await safeJson(response);
      const message = errorPayload?.message ?? `请求失败（${response.status}）`;
      throw new Error(message);
    }

    if (response.status === 204) {
      return null;
    }

    return safeJson(response);
  } finally {
    window.clearTimeout(timeoutId);
  }
}

/**
 * 构建完整 URL，并自动拼接查询参数。
 */
function buildUrl(path, params) {
  const baseUrl = getApiBaseUrl();
  const url = new URL(path.startsWith('http') ? path : `${baseUrl}${path}`);

  if (params) {
    Object.entries(params)
      .filter(([, value]) => value !== undefined && value !== null && value !== '')
      .forEach(([key, value]) => {
        url.searchParams.set(key, value);
      });
  }

  return url.toString();
}

/**
 * 安全解析 JSON，避免报错导致整个页面崩溃。
 */
async function safeJson(response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

/**
 * 文件上传请求
 */
async function uploadRequest(path, file) {
  const controller = new AbortController();
  const timeoutId = window.setTimeout(() => controller.abort(), API_TIMEOUT * 3); // 文件上传超时时间更长

  try {
    const baseUrl = getApiBaseUrl();
    const url = path.startsWith('http') ? path : `${baseUrl}${path}`;
    const token = getAuthToken();
    
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
        // 不设置 Content-Type，让浏览器自动设置 multipart/form-data
      },
      body: formData,
      signal: controller.signal
    });

    if (!response.ok) {
      if (response.status === 401) {
        window.localStorage.removeItem('medical-jwt');
        window.localStorage.removeItem('token');
        window.localStorage.removeItem('user');
        const currentPath = window.location.pathname + window.location.search;
        window.location.href = `/login?from=${encodeURIComponent(currentPath)}`;
        return;
      }

      const errorPayload = await safeJson(response);
      const message = errorPayload?.message ?? `上传失败（${response.status}）`;
      throw new Error(message);
    }

    return safeJson(response);
  } finally {
    window.clearTimeout(timeoutId);
  }
}

const apiClient = {
  get: (path, options) => apiRequest(path, { ...(options ?? {}), method: 'GET' }),
  post: (path, body, options) => apiRequest(path, { ...(options ?? {}), method: 'POST', body }),
  put: (path, body, options) => apiRequest(path, { ...(options ?? {}), method: 'PUT', body }),
  del: (path, options) => apiRequest(path, { ...(options ?? {}), method: 'DELETE' }),
  upload: (path, file) => uploadRequest(path, file)
};

export default apiClient;

