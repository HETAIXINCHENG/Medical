import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function CancelAccount() {
  usePageStyles('cancel-account.css');
  const navigate = useNavigate();
  const { t } = useLanguage();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="cancel-account-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">注销账号</h1>
      </div>

      {/* 内容区域 */}
      <div className="cancel-account-content">
        {/* 警告图标 */}
        <div className="warning-icon">
          <div className="warning-circle">!</div>
        </div>

        {/* 主标题 */}
        <h2 className="main-title">申请注销账号</h2>

        {/* 警告提示框 */}
        <div className="warning-box">
          <div className="warning-text">注销后,您与医生的交流记录将丢失!</div>
          <div className="warning-text">注销后,您上传的病历资料将丢失!</div>
          <div className="warning-text">注销后,账户余额等资产权益将丢失!</div>
        </div>

        {/* 按钮 */}
        <div className="button-group">
          <button className="return-btn" onClick={() => navigate(-1)}>
            返回
          </button>
          <button className="apply-btn" onClick={() => navigate('/cancel-account-reason')}>
            申请注销
          </button>
        </div>

        {/* 免责声明 */}
        <div className="disclaimer">
          <p>点击申请注销按钮,即表示您</p>
          <p>
            已阅读并同意
            <span className="notice-link" onClick={() => navigate('/cancel-account-notice')}>
              《重要提醒》
            </span>
          </p>
        </div>
      </div>
    </PageLayout>
  );
}

