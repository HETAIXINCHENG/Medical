import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useEffect, useState } from 'react';
import { medicalApi } from '../services/medicalApi.js';

export default function PreConsultation() {
  usePageStyles('pre-consultation.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const doctorId = searchParams.get('doctorId');
  const type = searchParams.get('type');
  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadPatients = async () => {
      try {
        const data = await medicalApi.getMyPatients();
        const patientsList = Array.isArray(data) ? data : [];
        setPatients(patientsList);
        
        // 默认选中"本人"（当前登录用户对应的患者）
        const selfPatient = patientsList.find(p => p.relation === '本人');
        if (selfPatient) {
          setSelectedPatientId(selfPatient.id);
        }
      } catch (err) {
        console.error('加载患者列表失败', err);
        setPatients([]);
      } finally {
        setLoading(false);
      }
    };
    loadPatients();
  }, []);

  const handlePatientSelect = (patientId) => {
    setSelectedPatientId(patientId);
    navigate(`/medical-record-list?doctorId=${doctorId}&type=${type}&patientId=${patientId}`);
  };

  const handleAddPatient = () => {
    navigate(`/add-patient?from=/pre-consultation`);
  };

  const handleOtherAccount = () => {
    // TODO: 跳转到其他账号患者下单页面
    alert('跳转到其他账号患者下单页面');
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">诊前信息收集</h1>
      </div>

      {/* 警告提示 */}
      <div className="warning-banner">
        <span className="warning-icon">⚠</span>
        <span className="warning-text">
          提示: 急重症患者不适合网上诊疗/咨询,请即刻前往当地医院急诊。
        </span>
      </div>

      {/* 患者选择区域 */}
      <div className="content-area">
        <div className="section">
          <h3 className="section-title">请选择患者</h3>
          <p className="section-hint">错误的患者信息会误导医生,请慎重选择</p>
          <a href="#" onClick={(e) => { e.preventDefault(); handleOtherAccount(); }} className="other-account-link">
            为其他账号下的患者下单&gt;
          </a>
        </div>

        {/* 新建患者按钮 */}
        <div className="add-patient-box" onClick={handleAddPatient}>
          <span className="add-icon">+</span>
          <span className="add-text">新建患者</span>
        </div>

        {/* 患者列表 */}
        <div className="patient-list">
          {loading ? (
            <div className="loading">加载中...</div>
          ) : patients.length === 0 ? (
            <div className="empty">暂无患者，请先添加患者</div>
          ) : (
            patients.map((patient) => (
              <div
                key={patient.id}
                className={`patient-card ${selectedPatientId === patient.id ? 'selected' : ''}`}
                onClick={() => handlePatientSelect(patient.id)}
              >
                <div className="patient-avatar">
                  <div className="avatar-placeholder">
                    {patient.name.charAt(0)}
                  </div>
                </div>
                <div className="patient-info">
                  <div className="patient-name-row">
                    <span className="patient-name">{patient.name}</span>
                    <span className={`patient-relation ${patient.relation === '本人' ? 'self' : 'family'}`}>
                      {patient.relation}
                    </span>
                  </div>
                  <div className="patient-details">
                    {patient.gender} {patient.age}岁
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </PageLayout>
  );
}

