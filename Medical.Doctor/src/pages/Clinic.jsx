import BottomNav from '../components/BottomNav.jsx';
import './clinic.css';

export default function Clinic() {
  return (
    <div className="page clinic-page">
      <div className="clinic-hero">
        <div className="topbar">
          <div className="title">医生诊所</div>
          <div className="actions">
            <span className="icon">🔍</span>
            <span className="icon">🔔</span>
          </div>
        </div>
        <div className="subtitle">皮肤专科数字诊所 · 国家卫健委认证医疗机构</div>
      </div>

      <div className="section card quick-grid">
        <div className="quick-item small">
          <div className="quick-icon circle">👥</div>
          <div className="quick-title">我的团队</div>
        </div>
        <div className="quick-item small">
          <div className="quick-icon circle">👥</div>
          <div className="quick-title">我的科普</div>
        </div>
        <div className="quick-item small">
          <div className="quick-icon circle">👥</div>
          <div className="quick-title">患友会</div>
        </div>
        <div className="quick-item small">
          <div className="quick-icon circle">📅</div>
          <div className="quick-title">工作日历</div>
        </div>
      </div>
      <BottomNav />
    </div>
  );
}

