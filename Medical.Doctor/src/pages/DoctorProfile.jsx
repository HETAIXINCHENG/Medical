import BottomNav from '../components/BottomNav.jsx';
import './doctor-profile.css';

export default function DoctorProfile() {
  return (
    <div className="page profile-page">
      <div className="profile-hero">
        <div className="avatar">👤</div>
        <div>
          <div className="login-title">立即登录</div>
          <div className="login-sub">登录后完成体验功能</div>
        </div>
      </div>

      <div className="card menu-card">
        <div className="menu-item">
          <span>投诉建议</span>
          <span>›</span>
        </div>
        <div className="menu-item">
          <span>联系我们</span>
          <span>›</span>
        </div>
        <div className="menu-item">
          <span>设置</span>
          <span>›</span>
        </div>
      </div>

      <BottomNav />
    </div>
  );
}

