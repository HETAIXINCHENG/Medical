import BottomNav from './BottomNav.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function PageLayout({ children }) {
  usePageStyles('bottom-nav.css');

  return (
    <div className="page-layout">
      {children}
      <BottomNav />
    </div>
  );
}
