/**
 * 微信分享工具
 * 使用微信 JS-SDK 实现公众号网页分享
 */

// 微信 JS-SDK 是否已加载
let wxReady = false;
let wxConfigReady = false;

/**
 * 加载微信 JS-SDK
 */
export function loadWeChatSDK() {
  return new Promise((resolve, reject) => {
    // 检查是否已经加载
    if (window.wx) {
      wxReady = true;
      resolve();
      return;
    }

    // 动态加载微信 JS-SDK
    const script = document.createElement('script');
    script.src = 'https://res.wx.qq.com/open/js/jweixin-1.6.0.js';
    script.onload = () => {
      wxReady = true;
      resolve();
    };
    script.onerror = () => {
      reject(new Error('微信 JS-SDK 加载失败'));
    };
    document.head.appendChild(script);
  });
}

/**
 * 配置微信 JS-SDK
 * @param {Object} config - 微信配置信息（从后端获取）
 * @param {string} config.appId - 公众号的唯一标识
 * @param {number} config.timestamp - 生成签名的时间戳
 * @param {string} config.nonceStr - 生成签名的随机串
 * @param {string} config.signature - 签名
 */
export function configWeChat(config) {
  return new Promise((resolve, reject) => {
    if (!window.wx) {
      reject(new Error('微信 JS-SDK 未加载'));
      return;
    }

    window.wx.config({
      debug: false, // 生产环境设为 false
      appId: config.appId,
      timestamp: config.timestamp,
      nonceStr: config.nonceStr,
      signature: config.signature,
      jsApiList: [
        'updateAppMessageShareData', // 分享给朋友
        'updateTimelineShareData'    // 分享到朋友圈
      ],
      success: () => {
        wxConfigReady = true;
        resolve();
      },
      fail: (err) => {
        reject(new Error('微信配置失败: ' + JSON.stringify(err)));
      }
    });
  });
}

/**
 * 配置分享内容
 * @param {Object} shareData - 分享数据
 * @param {string} shareData.title - 分享标题
 * @param {string} shareData.desc - 分享描述
 * @param {string} shareData.link - 分享链接
 * @param {string} shareData.imgUrl - 分享图标
 */
export function setupWeChatShare(shareData) {
  return new Promise((resolve, reject) => {
    if (!window.wx || !wxConfigReady) {
      reject(new Error('微信 JS-SDK 未配置'));
      return;
    }

    window.wx.ready(() => {
      // 分享给朋友
      window.wx.updateAppMessageShareData({
        title: shareData.title,
        desc: shareData.desc,
        link: shareData.link,
        imgUrl: shareData.imgUrl,
        success: () => {
          console.log('分享给朋友配置成功');
        },
        cancel: () => {
          console.log('用户取消分享给朋友');
        }
      });

      // 分享到朋友圈
      window.wx.updateTimelineShareData({
        title: shareData.title,
        link: shareData.link,
        imgUrl: shareData.imgUrl,
        success: () => {
          console.log('分享到朋友圈配置成功');
        },
        cancel: () => {
          console.log('用户取消分享到朋友圈');
        }
      });

      resolve();
    });

    window.wx.error((err) => {
      reject(new Error('微信 JS-SDK 错误: ' + JSON.stringify(err)));
    });
  });
}

/**
 * 初始化微信分享
 * @param {Function} getSignature - 获取签名的函数，返回 Promise<{appId, timestamp, nonceStr, signature}>
 * @param {Object} shareData - 分享数据
 */
export async function initWeChatShare(getSignature, shareData) {
  try {
    // 1. 加载微信 JS-SDK
    await loadWeChatSDK();

    // 2. 获取签名配置
    const config = await getSignature();

    // 3. 配置微信 JS-SDK
    await configWeChat(config);

    // 4. 设置分享内容
    await setupWeChatShare(shareData);

    return true;
  } catch (error) {
    console.error('初始化微信分享失败:', error);
    return false;
  }
}

/**
 * 检查是否在微信环境中
 */
export function isWeChat() {
  return /micromessenger/i.test(navigator.userAgent);
}

