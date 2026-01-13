import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function VerificationCodeHelp() {
  usePageStyles('verification-code-help.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="help-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">无法收到验证码</h1>
      </div>

      {/* 内容区域 */}
      <div className="help-content">
        <div className="help-title">手机号正常使用,无法收到验证码</div>
        <p className="help-greeting">您好,请您确认:</p>
        <ul className="help-list">
          <li>手机验证码是否已发送超5次,超过需要第二天再试</li>
          <li>手机号码是否写错/已停机,导致无法接收</li>
          <li>手机是否安装拦截软件,导致短信被拦截</li>
        </ul>
        <p className="help-footer">
          如果排除以上情况,仍然无法收到短信,请在电脑端访问好医生在线网站-首页-点击底部"意见和建议"反馈
        </p>
      </div>
    </PageLayout>
  );
}

