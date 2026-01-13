import { getApiBaseUrl } from '../config/apiConfig.js';

/**
 * 构建完整的图片URL
 * @param {string} imageUrl - 图片URL（可能是相对路径或完整URL）
 * @param {string} defaultImage - 默认图片路径（本地图片）
 * @returns {string} 完整的图片URL
 */
export const buildImageUrl = (imageUrl, defaultImage = null) => {
  // 如果没有提供图片URL，返回默认图片
  if (!imageUrl || imageUrl.trim() === '') {
    // 如果没有指定默认图片，使用默认头像
    if (!defaultImage) {
      return '/Img/default-avatar.png';
    }
    return defaultImage;
  }

  // 将Windows路径的反斜杠转换为正斜杠（用于Web URL）
  let normalizedUrl = typeof imageUrl === 'string' ? imageUrl.replace(/\\/g, '/') : imageUrl;

  // 如果已经是完整的HTTP/HTTPS URL，直接返回
  if (normalizedUrl.startsWith('http://') || normalizedUrl.startsWith('https://')) {
    return normalizedUrl;
  }

  // 如果是本地图片路径（以 /Img 开头），直接返回
  if (normalizedUrl.startsWith('/Img')) {
    return normalizedUrl;
  }

  // 如果是相对路径（以 /uploads 开头），需要加上API基础URL
  if (normalizedUrl.startsWith('/uploads')) {
    return `${getApiBaseUrl()}${normalizedUrl}`;
  }

  // 其他情况，也加上API基础URL
  return `${getApiBaseUrl()}/${normalizedUrl}`;
};

