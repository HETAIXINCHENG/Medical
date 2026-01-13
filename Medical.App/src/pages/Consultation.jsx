import { Link } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Consultation() {
  usePageStyles('consultation.css');
  const { t } = useLanguage();

  const messages = [
    { type: 'received', text: '您好，请问有什么可以帮助您的？', time: '10:00' },
    { type: 'sent', text: '医生您好，我最近总是感觉头晕', time: '10:01' },
    { type: 'received', text: '请问您头晕多久了？还有其他症状吗？', time: '10:02' },
    { type: 'sent', text: '大概有一周了，有时候还会恶心', time: '10:03' },
    { type: 'received', text: '建议您先测量一下血压，看看是否正常。同时注意休息，避免熬夜。', time: '10:05' }
  ];

  return (
    <PageLayout>
      <div className="navbar">
        <Link to="/doctors" className="back-arrow-link">
          <img src="/Img/10-咨询问诊/返回.png" alt="返回" className="back-arrow" />
        </Link>
        <div className="doctor-info-bar">
          <img src="/Img/10-咨询问诊/1.png" alt="王医生" className="message-avatar doctor-avatar" />
          <div className="doctor-name-section">
            <span className="doctor-name">{t('doctor-wang')}</span>
            <span className="online-status">{t('online')}</span>
          </div>
        </div>
        <img src="/Img/10-咨询问诊/更多.png" alt={t('more')} className="more-icon" />
      </div>

      <div className="messages-container">
        {messages.map((message, index) => (
          <div key={index} className={`message-wrapper ${message.type}`}>
            {message.type === 'received' && (
              <img src="/Img/10-咨询问诊/1.png" alt={t('doctor')} className="message-avatar" />
            )}
            <div className="message-bubble">
              <div className="message-text">{message.text}</div>
              <div className="message-time">{message.time}</div>
            </div>
            {message.type === 'sent' && (
              <img src="/Img/10-咨询问诊/我的头像.png" alt={t('me')} className="message-avatar" />
            )}
          </div>
        ))}
      </div>

      <div className="input-container">
        <button className="attach-btn">
          <img src="/Img/10-咨询问诊/图片.png" alt={t('image')} />
        </button>
        <input type="text" className="message-input" placeholder={t('input-message')} />
        <button className="send-btn">{t('send')}</button>
      </div>
    </PageLayout>
  );
}
