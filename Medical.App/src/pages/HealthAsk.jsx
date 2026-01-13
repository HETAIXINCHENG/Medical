import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import apiClient from '../services/apiClient.js';
import { buildImageUrl } from '../utils/imageUtils.js';

export default function HealthAsk() {
  usePageStyles('health-ask.css');
  const navigate = useNavigate();
  const messagesEndRef = useRef(null);
  const imageInputRef = useRef(null);
  
  const [messages, setMessages] = useState([
    {
      type: 'ai',
      text: '您好，我是医生的AI助手。我将协助您梳理病情信息，并将同步您的诊前报告给您的接诊医生~',
      time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
    },
    {
      type: 'ai',
      text: '请问您是有什么问题需要咨询吗？',
      time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
    }
  ]);
  
  const [inputText, setInputText] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [sessionId, setSessionId] = useState(null);
  const typingIntervalRef = useRef(null);
  const [attachments, setAttachments] = useState([]); // 存储待发送的附件 [{type: 'image'|'file', file: File, preview: string, url?: string}]

  // 自动滚动到底部
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, attachments]);

  // 组件卸载时清理定时器
  useEffect(() => {
    return () => {
      if (typingIntervalRef.current) {
        clearInterval(typingIntervalRef.current);
        typingIntervalRef.current = null;
      }
    };
  }, []);

  // 处理图片选择
  const handleImageSelect = (e) => {
    const files = Array.from(e.target.files || []);
    files.forEach(file => {
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (event) => {
          setAttachments(prev => [...prev, {
            type: 'image',
            file: file,
            preview: event.target.result,
            name: file.name
          }]);
        };
        reader.readAsDataURL(file);
      }
    });
    // 清空input，以便可以重复选择同一文件
    e.target.value = '';
  };


  // 删除附件
  const removeAttachment = (index) => {
    setAttachments(prev => prev.filter((_, i) => i !== index));
  };

  // 上传图片到服务器
  const uploadFile = async (file) => {
    try {
      const endpoint = '/api/upload/image?category=aidiagnosis';
      const response = await apiClient.upload(endpoint, file);
      const relativePath = response?.url || response?.path || response?.fileUrl || '';
      
      // 使用 buildImageUrl 确保返回完整的URL
      if (relativePath) {
        return buildImageUrl(relativePath);
      }
      
      return relativePath;
    } catch (error) {
      console.error('图片上传失败:', error);
      throw error;
    }
  };

  // 发送消息
  const handleSend = async () => {
    // 验证：文本不能为空
    if (!inputText.trim()) {
      alert('请输入问题');
      return;
    }

    // 如果正在加载，不发送
    if (isLoading) return;

    // 先上传所有图片（如果有）
    const uploadedImages = [];
    if (attachments.length > 0) {
      try {
        for (const attachment of attachments) {
          const url = await uploadFile(attachment.file);
          uploadedImages.push({
            url: url
          });
        }
      } catch (error) {
        alert('图片上传失败，请重试');
        return;
      }
    }

    // 构建用户消息（用于界面显示）
    const userMessage = {
      type: 'user',
      text: inputText,
      attachments: uploadedImages.length > 0 ? uploadedImages.map(img => ({
        type: 'image',
        url: img.url,
        name: '图片'
      })) : undefined,
      time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
    };

    setMessages(prev => [...prev, userMessage]);
    const currentInputText = inputText; // 保存当前输入文本
    setInputText('');
    setAttachments([]);
    setIsLoading(true);

    try {
      // 构建发送给AI的请求
      // 根据后端代码，如果只有文本（没有图片），需要确保 images 为 null
      const requestData = {
        prompt: currentInputText.trim(),
        images: uploadedImages.length > 0 ? uploadedImages : null
      };
      
      // 如果有sessionId，添加到请求中
      if (sessionId) {
        requestData.sessionId = sessionId;
      }

      console.log('发送AI请求数据:', JSON.stringify(requestData, null, 2));
      const response = await medicalApi.aiDiagnosis(requestData);

      console.log('AI预诊响应:', response);

      if (response && response.text) {
        // 更新sessionId
        if (response.sessionId) {
          setSessionId(response.sessionId);
        }

        // 先添加一个空消息，然后逐步显示文字
        const aiMessage = {
          type: 'ai',
          text: '',
          time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
        };

        setMessages(prev => [...prev, aiMessage]);
        setIsLoading(false);

        // 打字机效果：逐步显示文字
        const fullText = response.text;
        let currentIndex = 0;
        const typingSpeed = 30; // 每个字符的显示间隔（毫秒）

        // 清理之前的定时器
        if (typingIntervalRef.current) {
          clearInterval(typingIntervalRef.current);
        }

        typingIntervalRef.current = setInterval(() => {
          if (currentIndex < fullText.length) {
            currentIndex++;
            const partialText = fullText.substring(0, currentIndex);
            
            // 更新最后一条消息的文本
            setMessages(prev => {
              const newMessages = [...prev];
              if (newMessages.length > 0 && newMessages[newMessages.length - 1].type === 'ai') {
                newMessages[newMessages.length - 1] = {
                  ...newMessages[newMessages.length - 1],
                  text: partialText
                };
              }
              return newMessages;
            });

            // 每次更新后滚动到底部
            setTimeout(() => scrollToBottom(), 0);
          } else {
            clearInterval(typingIntervalRef.current);
            typingIntervalRef.current = null;
          }
        }, typingSpeed);
      } else {
        throw new Error('AI回复为空');
      }
    } catch (error) {
      console.error('AI预诊请求失败:', error);
      setIsLoading(false);
      const errorMessage = {
        type: 'ai',
        text: error.message || '抱歉，AI助手暂时无法响应，请稍后再试。',
        time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
      };
      setMessages(prev => [...prev, errorMessage]);
    }
  };

  // 处理回车键发送
  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="health-ask-page">
      {/* 顶部头部 */}
      <div className="health-ask-header">
        <div className="header-left" onClick={() => navigate('/')}>
          <img src="/Img/return.png" alt="返回" className="return-icon" />
        </div>
        <div className="header-right" onClick={() => navigate('/profile')}>
          <span className="my-profile">我的</span>
        </div>
      </div>

      {/* 聊天消息区域 */}
      <div className="messages-container">
        {messages.map((message, index) => (
          <div 
            key={index} 
            className={`message-wrapper ${message.type === 'ai' ? 'ai-message' : 'user-message'}`}
          >
            {message.type === 'ai' && (
              <img src="/Img/Director.png" alt="AI助手" className="message-avatar"/>
            )}
            <div className={`message-bubble ${message.type === 'ai' ? 'ai-bubble' : 'user-bubble'}`}>
              {message.attachments && message.attachments.length > 0 && (
                <div className="message-attachments">
                  {message.attachments.map((att, attIndex) => (
                    <div key={attIndex} className="attachment-item">
                      <img 
                        src={buildImageUrl(att.url)} 
                        alt={att.name || '图片'} 
                        className="attachment-image"
                      />
                    </div>
                  ))}
                </div>
              )}
              {message.text && <div className="message-text">{message.text}</div>}
            </div>
            {message.type === 'user'}
          </div>
        ))}
        {isLoading && (
          <div className="message-wrapper ai-message">
            <img src="/Img/Director.png" alt="AI助手" className="message-avatar"/>
            <div className="message-bubble ai-bubble">
              <div className="message-text">正在思考中<span className="typing-dots">
                <span>.</span><span>.</span><span>.</span>
              </span></div>
            </div>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* 附件预览区域 */}
      {attachments.length > 0 && (
        <div className="attachments-preview">
          {attachments.map((attachment, index) => (
            <div key={index} className="attachment-preview-item">
              <img src={attachment.preview} alt={attachment.name} className="preview-image" />
              <button 
                className="remove-attachment-btn"
                onClick={() => removeAttachment(index)}
                title="删除"
              >
                ×
              </button>
            </div>
          ))}
        </div>
      )}

      {/* 底部输入框 */}
      <div className="input-container">
        <button 
          className="add-btn" 
          title="添加图片"
          onClick={() => imageInputRef.current?.click()}
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 13H13V19H11V13H5V11H11V5H13V11H19V13Z" fill="#666"/>
          </svg>
        </button>

        {/* 隐藏的图片输入 */}
        <input
          ref={imageInputRef}
          type="file"
          accept="image/jpeg,image/png,image/gif,image/webp"
          multiple
          style={{ display: 'none' }}
          onChange={handleImageSelect}
        />

        <input
          type="text"
          className="message-input"
          placeholder="请输入您的问题"
          value={inputText}
          onChange={(e) => setInputText(e.target.value)}
          onKeyPress={handleKeyPress}
          disabled={isLoading}
        />
        <button 
          className="send-btn" 
          onClick={handleSend}
          disabled={isLoading || !inputText.trim()}
          title="发送"
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M2.01 21L23 12L2.01 3L2 10L17 12L2 14L2.01 21Z" fill="#00DDBA"/>
          </svg>
        </button>
      </div>
    </div>
  );
}
