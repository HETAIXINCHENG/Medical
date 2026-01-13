# 微信分享功能配置说明

## 后端配置

### 1. 配置微信公众平台信息

在 `appsettings.json` 或 `appsettings.Development.json` 中添加：

```json
{
  "WeChat": {
    "AppId": "你的微信公众号AppID",
    "AppSecret": "你的微信公众号AppSecret"
  }
}
```

### 2. 获取微信公众平台信息

1. 登录 [微信公众平台](https://mp.weixin.qq.com/)
2. 进入"开发" -> "基本配置"
3. 获取 `AppID` 和 `AppSecret`

### 3. 配置 JS 接口安全域名

1. 在微信公众平台中，进入"设置" -> "公众号设置" -> "功能设置"
2. 在"JS接口安全域名"中添加你的域名（不需要加 http:// 或 https://）
3. 例如：`yourdomain.com` 或 `www.yourdomain.com`

## 前端使用

微信分享功能已自动集成到以下页面：
- `PostDetail.jsx` - 帖子详情页
- `PatientSupportGroupDetail.jsx` - 患友会详情页

### 功能说明

1. **自动初始化**：在微信环境中打开页面时，会自动初始化微信分享
2. **分享内容**：
   - 标题：帖子标题或患友会名称
   - 描述：帖子内容或患友会描述
   - 链接：当前页面URL
   - 图标：作者头像或医生头像

3. **分享方式**：
   - 在微信中：用户点击右上角菜单按钮进行分享
   - 非微信环境：点击分享按钮会复制链接到剪贴板

## API 接口

### GET /api/wechat/signature

获取微信 JS-SDK 签名配置

**请求参数：**
- `url` (query): 当前页面的完整URL（不包含#及其后面部分）

**返回数据：**
```json
{
  "appId": "wx1234567890abcdef",
  "timestamp": 1234567890,
  "nonceStr": "abc123def456",
  "signature": "sha1签名"
}
```

## 注意事项

1. **域名配置**：必须在微信公众平台配置 JS 接口安全域名
2. **HTTPS**：生产环境必须使用 HTTPS
3. **URL 参数**：传给后端的 URL 必须与当前页面 URL 完全一致（不包含 hash）
4. **签名有效期**：微信签名有时效性，建议每次分享前重新获取

