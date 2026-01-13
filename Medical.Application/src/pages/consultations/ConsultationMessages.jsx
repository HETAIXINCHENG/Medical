import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card,
  Input,
  Button,
  Upload,
  Image,
  Spin,
  Empty,
  Modal,
  Tag,
  App
} from 'antd';
import {
  SendOutlined,
  PictureOutlined,
  VideoCameraOutlined,
  AudioOutlined,
  FileOutlined
} from '@ant-design/icons';
import resourceService from '../../api/resourceService.js';
import './ConsultationMessages.css';

const { TextArea } = Input;

function ConsultationMessagesContent() {
  const { consultationId } = useParams();
  const navigate = useNavigate();
  const { message: messageApi } = App.useApp();
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [sending, setSending] = useState(false);
  const [content, setContent] = useState('');
  const [messageType, setMessageType] = useState('Text');
  const [attachmentFile, setAttachmentFile] = useState(null);
  const [consultation, setConsultation] = useState(null);
  const messagesEndRef = useRef(null);
  const [previewVisible, setPreviewVisible] = useState(false);
  const [previewUrl, setPreviewUrl] = useState('');

  const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

  // 滚动到底部
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  // 加载咨询信息
  const loadConsultation = async () => {
    if (!consultationId) {
      console.warn('consultationId 为空，无法加载咨询信息');
      return;
    }
    try {
      console.log('加载咨询信息，consultationId:', consultationId);
      const response = await resourceService.get(`/api/consultations/${consultationId}`);
      console.log('咨询信息响应:', response);
      setConsultation(response);
    } catch (err) {
      console.error('加载咨询信息失败:', err);
      console.error('错误详情:', {
        message: err.message,
        status: err.status,
        response: err.response
      });
      messageApi.error('加载咨询信息失败: ' + (err.message || '未知错误'));
    }
  };

  // 加载消息列表
  const loadMessages = async () => {
    if (!consultationId) {
      console.warn('consultationId 为空，无法加载消息');
      return;
    }
    
    setLoading(true);
    try {
      console.log('开始加载消息，consultationId:', consultationId);
      console.log('API路径: /api/ConsultationMessages');
      console.log('查询参数:', { consultationId, page: 1, pageSize: 1000 });
      
      // 尝试两种API路径格式
      let response;
      try {
        response = await resourceService.list('/api/ConsultationMessages', {
          consultationId,
          page: 1,
          pageSize: 1000
        });
      } catch (err) {
        console.warn('尝试 /api/ConsultationMessages 失败，尝试小写路径');
        response = await resourceService.list('/api/consultationmessages', {
          consultationId,
          page: 1,
          pageSize: 1000
        });
      }
      
      console.log('消息API响应:', response);
      console.log('响应类型:', typeof response);
      console.log('是否为数组:', Array.isArray(response));
      
      // API返回格式: { items, total, page, pageSize }
      const items = Array.isArray(response) ? response : (response?.items ?? response?.data ?? []);
      console.log('解析后的消息列表:', items, '数量:', items.length);
      
      setMessages(items);
      setTimeout(scrollToBottom, 100);
    } catch (err) {
      console.error('加载消息失败:', err);
      console.error('错误详情:', {
        message: err.message,
        status: err.status,
        response: err.response
      });
      messageApi.error('加载消息失败: ' + (err.message || '未知错误'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    console.log('ConsultationMessages useEffect, consultationId:', consultationId);
    if (consultationId) {
      loadConsultation();
      loadMessages();
      // 每5秒刷新一次消息
      const interval = setInterval(loadMessages, 5000);
      return () => clearInterval(interval);
    } else {
      console.warn('consultationId 未提供');
    }
  }, [consultationId]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // 上传文件
  const uploadFile = async (file, type) => {
    const token = window.localStorage.getItem('medical-admin-token');
    const formData = new FormData();
    formData.append('file', file);

    let endpoint = '/api/upload/image';
    if (type === 'Video') {
      endpoint = '/api/upload/video';
    } else if (type === 'Voice') {
      endpoint = '/api/upload/audio';
    } else if (type === 'File') {
      endpoint = '/api/upload/file';
    }

    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${baseURL}${endpoint}`);
      if (token) {
        xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      }

      xhr.addEventListener('load', () => {
        if (xhr.status === 200) {
          const response = JSON.parse(xhr.responseText);
          resolve(response.url || response.path);
        } else {
          const error = JSON.parse(xhr.responseText || '{}');
          reject(new Error(error.message || '上传失败'));
        }
      });

      xhr.addEventListener('error', () => {
        reject(new Error('上传失败'));
      });

      xhr.send(formData);
    });
  };

  // 发送消息
  const handleSend = async () => {
    if (!content.trim() && !attachmentFile && messageType === 'Text') {
      messageApi.warning('请输入消息内容');
      return;
    }

    if (messageType !== 'Text' && !attachmentFile) {
      messageApi.warning('请选择附件');
      return;
    }

    setSending(true);
    try {
      let attachmentUrl = null;

      // 如果有附件，先上传
      if (attachmentFile) {
        try {
          attachmentUrl = await uploadFile(attachmentFile, messageType);
        } catch (err) {
          messageApi.error('附件上传失败: ' + err.message);
          setSending(false);
          return;
        }
      }

      // 发送消息
      const payload = {
        consultationId,
        content: content || (messageType !== 'Text' ? `发送了${getMessageTypeLabel(messageType)}` : ''),
        messageType,
        attachmentUrl
      };

      await resourceService.create('/api/consultationmessages', payload);
      
      messageApi.success('消息发送成功');
      setContent('');
      setAttachmentFile(null);
      setMessageType('Text');
      loadMessages();
    } catch (err) {
      console.error('发送消息失败:', err);
      messageApi.error('发送消息失败');
    } finally {
      setSending(false);
    }
  };

  // 处理文件选择
  const handleFileSelect = (file, type) => {
    setMessageType(type);
    setAttachmentFile(file);
    return false; // 阻止自动上传
  };

  // 获取消息类型标签
  const getMessageTypeLabel = (type) => {
    const typeMap = {
      'Text': '文本',
      'Image': '图片',
      'Voice': '语音',
      'Video': '视频',
      'File': '文件'
    };
    return typeMap[type] || type;
  };

  // 格式化时间
  const formatTime = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const minutes = Math.floor(diff / 60000);
    
    if (minutes < 1) return '刚刚';
    if (minutes < 60) return `${minutes}分钟前`;
    if (minutes < 1440) return `${Math.floor(minutes / 60)}小时前`;
    return date.toLocaleString('zh-CN', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
  };

  // 预览图片/视频
  const handlePreview = (url) => {
    setPreviewUrl(url);
    setPreviewVisible(true);
  };

  // 获取完整URL
  const getFullUrl = (url) => {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return `${baseURL}${url}`;
  };

  if (!consultationId) {
    return (
      <Card>
        <Empty description="请选择咨询" />
        <div style={{ marginTop: '16px', color: '#999', fontSize: '12px' }}>
          调试信息: consultationId 为空
        </div>
      </Card>
    );
  }

  return (
    <div className="consultation-messages-page">
      {/* 调试信息 */}
      <Card size="small" style={{ marginBottom: '16px', background: '#fffbe6', border: '1px solid #ffe58f' }}>
        <div style={{ fontSize: '12px', color: '#666' }}>
          <div><strong>🔍 调试信息：</strong></div>
          <div>consultationId: <code>{consultationId || '未提供'}</code></div>
          <div>消息数量: <strong>{messages.length}</strong></div>
          <div>加载状态: {loading ? '⏳ 加载中...' : '✅ 已完成'}</div>
          <div>咨询信息: {consultation ? '✅ 已加载' : '❌ 未加载'}</div>
          {consultation && (
            <>
              <div>患者: {consultation.patient?.realName || '-'}</div>
              <div>医生: {consultation.doctor?.name || '-'}</div>
            </>
          )}
          {messages.length > 0 && (
            <div style={{ marginTop: '8px', padding: '4px', background: '#f0f0f0', borderRadius: '4px' }}>
              第一条消息: {JSON.stringify(messages[0], null, 2).substring(0, 200)}...
            </div>
          )}
        </div>
      </Card>
      
      {/* 咨询信息头部 */}
      {consultation && (
        <Card className="consultation-header" size="small">
          <div className="consultation-info">
            <div>
              <strong>患者：</strong>
              {consultation.patient?.realName || '-'}
            </div>
            <div>
              <strong>医生：</strong>
              {consultation.doctor?.name || '-'}
            </div>
            <div>
              <strong>状态：</strong>
              <Tag color={consultation.status === 'Completed' ? 'green' : 'blue'}>
                {consultation.status === 'Pending' ? '待处理' :
                 consultation.status === 'InProgress' ? '进行中' :
                 consultation.status === 'Completed' ? '已完成' : '已取消'}
              </Tag>
            </div>
          </div>
        </Card>
      )}

      {/* 消息列表 */}
      <Card className="messages-container" bodyStyle={{ padding: '16px', height: 'calc(100vh - 400px)', overflowY: 'auto' }}>
        <Spin spinning={loading}>
          {loading ? (
            <div style={{ textAlign: 'center', padding: '20px' }}>加载中...</div>
          ) : messages.length === 0 ? (
            <Empty description="暂无消息" />
          ) : (
            <div className="messages-list">
              {messages.map((msg) => {
                if (!msg || !msg.id) {
                  console.warn('无效的消息对象:', msg);
                  return null;
                }
                return (
                <div
                  key={msg.id}
                  className={`message-item ${msg.isFromDoctor ? 'doctor-message' : 'patient-message'}`}
                >
                  <div className="message-avatar">
                    {msg.isFromDoctor ? '👨‍⚕️' : '👤'}
                  </div>
                  <div className="message-content">
                    <div className="message-header">
                      <span className="sender-name">
                        {msg.isFromDoctor ? '医生' : '患者'}
                      </span>
                      <span className="message-time">{formatTime(msg.createdAt)}</span>
                    </div>
                    <div className="message-body">
                      {msg.messageType === 'Text' && (
                        <div className="text-message">{msg.content}</div>
                      )}
                      {msg.messageType === 'Image' && msg.attachmentUrl && (
                        <div className="image-message">
                          <img
                            src={getFullUrl(msg.attachmentUrl)}
                            alt="图片"
                            onClick={() => handlePreview(getFullUrl(msg.attachmentUrl))}
                            style={{ maxWidth: '300px', cursor: 'pointer', borderRadius: '8px' }}
                          />
                          {msg.content && <div className="image-caption">{msg.content}</div>}
                        </div>
                      )}
                      {msg.messageType === 'Video' && msg.attachmentUrl && (
                        <div className="video-message">
                          <video
                            src={getFullUrl(msg.attachmentUrl)}
                            controls
                            style={{ maxWidth: '300px', borderRadius: '8px' }}
                          />
                          {msg.content && <div className="video-caption">{msg.content}</div>}
                        </div>
                      )}
                      {msg.messageType === 'Voice' && msg.attachmentUrl && (
                        <div className="audio-message">
                          <audio src={getFullUrl(msg.attachmentUrl)} controls />
                          {msg.content && <div className="audio-caption">{msg.content}</div>}
                        </div>
                      )}
                      {msg.messageType === 'File' && msg.attachmentUrl && (
                        <div className="file-message">
                          <FileOutlined style={{ fontSize: '24px', marginRight: '8px' }} />
                          <a
                            href={getFullUrl(msg.attachmentUrl)}
                            target="_blank"
                            rel="noopener noreferrer"
                          >
                            下载文件
                          </a>
                          {msg.content && <div className="file-caption">{msg.content}</div>}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                );
              })}
              <div ref={messagesEndRef} />
            </div>
          )}
        </Spin>
      </Card>

      {/* 输入区域 */}
      <Card className="input-container" size="small">
        <div className="message-type-selector">
          <Button
            type={messageType === 'Text' ? 'primary' : 'default'}
            size="small"
            onClick={() => {
              setMessageType('Text');
              setAttachmentFile(null);
            }}
          >
            文本
          </Button>
          <Upload
            accept="image/*"
            showUploadList={false}
            beforeUpload={(file) => {
              handleFileSelect(file, 'Image');
              return false;
            }}
          >
            <Button
              type={messageType === 'Image' ? 'primary' : 'default'}
              size="small"
              icon={<PictureOutlined />}
            >
              图片
            </Button>
          </Upload>
          <Upload
            accept="video/*"
            showUploadList={false}
            beforeUpload={(file) => {
              handleFileSelect(file, 'Video');
              return false;
            }}
          >
            <Button
              type={messageType === 'Video' ? 'primary' : 'default'}
              size="small"
              icon={<VideoCameraOutlined />}
            >
              视频
            </Button>
          </Upload>
          <Upload
            accept="audio/*"
            showUploadList={false}
            beforeUpload={(file) => {
              handleFileSelect(file, 'Voice');
              return false;
            }}
          >
            <Button
              type={messageType === 'Voice' ? 'primary' : 'default'}
              size="small"
              icon={<AudioOutlined />}
            >
              语音
            </Button>
          </Upload>
          <Upload
            accept="*/*"
            showUploadList={false}
            beforeUpload={(file) => {
              handleFileSelect(file, 'File');
              return false;
            }}
          >
            <Button
              type={messageType === 'File' ? 'primary' : 'default'}
              size="small"
              icon={<FileOutlined />}
            >
              文件
            </Button>
          </Upload>
        </div>
        {attachmentFile && (
          <div className="attachment-preview">
            <Tag closable onClose={() => setAttachmentFile(null)}>
              {attachmentFile.name}
            </Tag>
          </div>
        )}
        <div className="input-row">
          {messageType === 'Text' ? (
            <TextArea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="输入消息..."
              autoSize={{ minRows: 1, maxRows: 4 }}
              onPressEnter={(e) => {
                if (!e.shiftKey) {
                  e.preventDefault();
                  handleSend();
                }
              }}
            />
          ) : (
            <Input
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="添加说明（可选）"
            />
          )}
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSend}
            loading={sending}
            disabled={!content.trim() && !attachmentFile && messageType === 'Text'}
          >
            发送
          </Button>
        </div>
      </Card>

      {/* 图片/视频预览模态框 */}
      <Modal
        open={previewVisible}
        footer={null}
        onCancel={() => setPreviewVisible(false)}
        width={800}
        centered
      >
        {previewUrl && (
          previewUrl.match(/\.(jpg|jpeg|png|gif|webp)$/i) ? (
            <Image src={previewUrl} style={{ width: '100%' }} />
          ) : (
            <video src={previewUrl} controls style={{ width: '100%' }} />
          )
        )}
      </Modal>
    </div>
  );
}

function ConsultationMessages() {
  return (
    <App>
      <ConsultationMessagesContent />
    </App>
  );
}

export default ConsultationMessages;

