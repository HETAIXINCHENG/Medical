import { useState, useRef, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function CustomerService() {
  usePageStyles('customer-service.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const orderId = searchParams.get('orderId');
  const shopName = searchParams.get('shopName') || '商家';

  const [messages, setMessages] = useState([]);
  const [inputText, setInputText] = useState('');
  const messagesEndRef = useRef(null);
  const fileInputRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = () => {
    if (!inputText.trim()) return;

    const newMessage = {
      id: Date.now(),
      text: inputText.trim(),
      sender: 'user',
      time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
    };

    setMessages([...messages, newMessage]);
    setInputText('');

    // 模拟客服回复
    setTimeout(() => {
      const replyMessage = {
        id: Date.now() + 1,
        text: '收到您的投诉，我们会尽快处理。',
        sender: 'service',
        time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
      };
      setMessages((prev) => [...prev, replyMessage]);
    }, 1000);
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const faqItems = [
    {
      id: 1,
      title: '好大夫购药售后问题',
      content: '购药订单售后问题请优先联系售卖的商家，您可以点击订单详情页的"联系商家"按钮与商家沟通。'
    }
  ];

  return (
    <PageLayout>
      <div className="customer-service-page">
        {/* 顶部导航 */}
        <div className="service-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="返回" className="back-icon" />
          </button>
          <div className="header-content">
            <div className="header-title">我的客服</div>
          </div>
          <div className="service-avatar">
            <img src="/Img/Director.png" alt="客服" />
          </div>
        </div>

        {/* 温馨提示卡片 */}
        <div className="warm-reminder-card">
          <div className="reminder-icon">📋</div>
          <div className="reminder-content">
            <div className="reminder-title">温馨提示</div>
            <div className="reminder-main-text">购药售后问题请优先联系商家</div>
            <div className="reminder-desc">
              您好,您的购药订单售后问题需要优先联系售卖的商家,请您尽快去给商家留言。联系方法:点击订单详情页的"联系商家"按钮,或前往"我的订单"找到对应订单后点击"联系商家"。
            </div>
          </div>
        </div>

        {/* FAQ列表 */}
        <div className="faq-list">
          {faqItems.map((item, index) => (
            <div key={item.id} className="faq-item">
              <div className="faq-number">{index + 1}</div>
              <div className="faq-title">{item.title}</div>
            </div>
          ))}
        </div>

        {/* 聊天消息区域 */}
        {messages.length > 0 && (
          <div className="chat-messages">
            {messages.map((msg) => (
              <div
                key={msg.id}
                className={`message-item ${msg.sender === 'user' ? 'user-message' : 'service-message'}`}
              >
                <div className="message-bubble">
                  <div className="message-text">{msg.text}</div>
                  <div className="message-time">{msg.time}</div>
                </div>
              </div>
            ))}
            <div ref={messagesEndRef} />
          </div>
        )}

        {/* 底部输入栏 */}
        <div className="chat-input-bar">
          <div className="input-container">
            <input
              type="text"
              className="chat-input"
              placeholder="找不到答案?点此提问"
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
        </div>
      </div>
    </PageLayout>
  );
}

