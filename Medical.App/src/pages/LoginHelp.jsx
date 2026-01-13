import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function LoginHelp() {
  usePageStyles('login-help.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="help-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">登录遇到问题</h1>
      </div>

      {/* 问题列表 */}
      <div className="help-content">
        <div className="help-item" onClick={() => navigate('/verification-code-help')}>
          <span className="help-text">手机号正常使用,无法收到验证码</span>
          <span className="arrow-icon">›</span>
        </div>
        <div className="help-divider"></div>
        <div className="help-item" onClick={() => navigate('/forgot-password')}>
          <span className="help-text">忘记了账户密码</span>
          <span className="arrow-icon">›</span>
        </div>
      </div>
    </PageLayout>
  );
}

