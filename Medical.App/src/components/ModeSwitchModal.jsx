import { useTheme } from '../contexts/ThemeContext.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function ModeSwitchModal({ visible, onClose }) {
  usePageStyles('mode-switch-modal.css');
  const { mode, setMode } = useTheme();

  if (!visible) return null;

  const handleSelectMode = (selectedMode) => {
    setMode(selectedMode);
    onClose();
  };

  return (
    <>
      <div className="mode-modal-overlay" onClick={onClose} />
      <div className="mode-modal">
        <div 
          className={`mode-option ${mode === 'elder' ? 'selected' : ''}`}
          onClick={() => handleSelectMode('elder')}
        >
          <div className="mode-option-content">
            <div className="mode-title">
              长辈模式
              {mode === 'elder' && <span className="current-badge">当前</span>}
            </div>
            <div className="mode-subtitle">顾问协助,适合老年人使用</div>
          </div>
          <span className="mode-arrow">›</span>
        </div>
        <div 
          className={`mode-option ${mode === 'standard' ? 'selected' : ''}`}
          onClick={() => handleSelectMode('standard')}
        >
          <div className="mode-option-content">
            <div className="mode-title">
              标准模式
              {mode === 'standard' && <span className="current-badge">当前</span>}
            </div>
          </div>
          <span className="mode-arrow">›</span>
        </div>
      </div>
    </>
  );
}
