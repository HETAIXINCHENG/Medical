import { useState, useEffect, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function MerchantChat() {
  usePageStyles('merchant-chat.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const shopName = searchParams.get('shopName') || '商家服务';
  const shopId = searchParams.get('shopId') || '';

  const [messages, setMessages] = useState([]);
  const [inputText, setInputText] = useState('');
  const messagesEndRef = useRef(null);
  const fileInputRef = useRef(null);

  useEffect(() => {
    // 滚动到底部
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const createMessageBase = () => ({
    id: Date.now(),
    sender: 'user',
    time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
  });

  const handleSend = () => {
    if (!inputText.trim()) return;

    const newMessage = {
      ...createMessageBase(),
      text: inputText.trim()
    };

    setMessages((prev) => [...prev, newMessage]);
    setInputText('');
  };

  const handleUploadClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleFileChange = (e) => {
    const file = e.target.files && e.target.files[0];
    if (!file) return;

    const imageUrl = URL.createObjectURL(file);

    const newMessage = {
      ...createMessageBase(),
      imageUrl
    };

    setMessages((prev) => [...prev, newMessage]);

    // 清空 input，方便再次选择同一文件
    e.target.value = '';
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <PageLayout>
      <div className="merchant-chat-page">
        {/* 顶部导航 */}
        <div className="chat-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <span className="collapse-icon">收起 &gt;</span>
          </button>
          <div className="header-title">{shopName}</div>
          <div style={{ width: '40px' }}></div>
        </div>

        {/* 聊天内容区域 */}
        <div className="chat-messages">
          {messages.length === 0 ? (
            <div className="empty-chat">
              <p>暂无消息</p>
            </div>
          ) : (
            messages.map((msg) => (
              <div
                key={msg.id}
                className={`message-item ${msg.sender === 'user' ? 'user-message' : 'merchant-message'}`}
              >
                <div className="message-bubble">
                  {msg.imageUrl && (
                    <img src={msg.imageUrl} alt="上传图片" className="message-image" />
                  )}
                  {msg.text && <div className="message-text">{msg.text}</div>}
                  <div className="message-time">{msg.time}</div>
                </div>
              </div>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* 底部输入栏：加号图标 + 文本框 + 纸飞机发送图标 */}
        <div className="chat-input-bar">
          <div className="input-container">
            <button className="plus-btn" onClick={handleUploadClick} title="上传资料">
              <span className="plus-icon">+</span>
            </button>
            <input
              type="text"
              className="chat-input"
              placeholder="请输入您的问题"
              value={inputText}
              onChange={(e) => setInputText(e.target.value)}
              onKeyPress={handleKeyPress}
            />
            <button className="send-btn" onClick={handleSend} title="发送">
              <svg
                className="send-icon"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M2.01 21L23 12L2.01 3L2 10L17 12L2 14L2.01 21Z"
                  fill="#00DDBA"
                />
              </svg>
            </button>
          </div>
          <input
            type="file"
            accept="image/*"
            ref={fileInputRef}
            style={{ display: 'none' }}
            onChange={handleFileChange}
          />
        </div>
      </div>
    </PageLayout>
  );
}

