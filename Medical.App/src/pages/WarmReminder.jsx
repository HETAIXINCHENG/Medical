import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { useEffect, useState } from 'react';

export default function WarmReminder() {
  usePageStyles('warm-reminder.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const doctorId = searchParams.get('doctorId');
  const type = searchParams.get('type');
  const [doctor, setDoctor] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadDoctor = async () => {
      if (doctorId) {
        try {
          const detail = await medicalApi.getDoctorById(doctorId);
          setDoctor(detail);
        } catch (err) {
          console.error('加载医生信息失败', err);
        } finally {
          setLoading(false);
        }
      }
    };
    loadDoctor();
  }, [doctorId]);

  const handleAccept = () => {
    navigate(`/pre-consultation?doctorId=${doctorId}&type=${type}`);
  };

  const handleDecline = () => {
    navigate(-1);
  };

  const consultationTypeName = type === 'phone' ? '电话问诊' : '图文问诊';
  const waitingTime = type === 'phone' ? '2小时' : '2小时';

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">{consultationTypeName}</h1>
      </div>

      {/* 警告提示 */}
      <div className="warning-banner">
        <span className="warning-icon">⚠</span>
        <span className="warning-text">
          提示:急重症患者不适合网上诊疗/咨询,请即刻前往当地医院急诊。
        </span>
      </div>

      {/* 主要内容 */}
      <div className="content-area">
        <h2 className="reminder-title">温馨提示</h2>
        {!loading && doctor && (
          <div className="reminder-message">
            {doctor.name}医生的问诊预计等待时间为
            <span className="time-highlight">{waitingTime}</span>
            ,您能接受吗?
          </div>
        )}
        
        {/* 按钮区域 */}
        <div className="action-buttons">
          <button className="btn-decline" onClick={handleDecline}>
            不接受
          </button>
          <button className="btn-accept" onClick={handleAccept}>
            接受
          </button>
        </div>
      </div>
    </PageLayout>
  );
}

