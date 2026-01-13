import { Link, useLocation } from 'react-router-dom';
import { useLanguage } from '../contexts/LanguageContext.jsx';

/**
 * 页面公共底部导航，负责路由跳转和当前 tab 高亮。
 */
export default function BottomNav() {
  const location = useLocation();
  const { t } = useLanguage();

  // 底部导航的配置项，包含路由信息、图标和活跃状态判断
  const navItems = [
    {
      key: 'home',
      to: '/',
      label: t('home'),
      match: (pathname) => pathname === '/',
      defaultIcon: '/Img/home_1.png',
      activeIcon: '/Img/home.png',
      isCenter: false
    },
    {
      key: 'discover',
      to: '/discover',
      label: t('science'),
      match: (pathname) =>
        ['/discover', '/doctors', '/department'].includes(pathname) || pathname.startsWith('/doctor/'),
      defaultIcon: '/Img/discover_1.png',
      activeIcon: '/Img/discover.png',
      isCenter: false
    },
    {
      key: 'health-ask',
      to: '/health-ask',
      label: t('ai-diagnosis'),
      match: (pathname) => pathname === '/health-ask',
      defaultIcon: '/Img/search.png',
      activeIcon: '/Img/search.png',
      isCenter: true
    },
    {
      key: 'information',
      to: '/information',
      label: t('community'),
      match: (pathname) => ['/information', '/consultation'].includes(pathname),
      defaultIcon: '/Img/consult_1.png',
      activeIcon: '/Img/consult.png',
      isCenter: false
    },
    {
      key: 'profile',
      to: '/profile',
      label: t('my'),
      match: (pathname) => pathname === '/profile',
      defaultIcon: '/Img/mine_1.png',
      activeIcon: '/Img/mine.png',
      isCenter: false
    }
  ];

  return (
    <div className="bottom-nav">
      {navItems.map((item) => {
        const isActive =
          typeof item.match === 'function'
            ? item.match(location.pathname)
            : location.pathname === item.to;
        return (
          <Link 
            key={item.key} 
            to={item.to} 
            className={`nav-item${isActive ? ' active' : ''}`}
          >
            <img
              src={isActive ? item.activeIcon : item.defaultIcon}
              alt={item.label}
              className="nav-icon"
            />
            <span>{item.label}</span>
          </Link>
        );
      })}
    </div>
  );
}
