import { NavLink } from 'react-router-dom';
import './BottomNav.css';

export default function BottomNav() {
  const navs = [
    { to: '/clinic', label: '我的诊所', icon: '🏥' },
    { to: '/patients', label: '患者管理', icon: '📊' },
    { to: '/profile', label: '个人中心', icon: '👤' }
  ];

  return (
    <div className="doc-bottom-nav">
      {navs.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
        >
          <span className="icon">{item.icon}</span>
          <span className="label">{item.label}</span>
        </NavLink>
      ))}
    </div>
  );
}

