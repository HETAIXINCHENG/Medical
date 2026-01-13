import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function CancelAccountNotice() {
  usePageStyles('cancel-account-notice.css');
  const navigate = useNavigate();
  const { t } = useLanguage();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="cancel-notice-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <span className="back-arrow">←</span>
        </button>
        <h1 className="header-title">注销账号须知</h1>
      </div>

      {/* 内容区域 */}
      <div className="cancel-notice-content">
        {/* 重要提示 */}
        <div className="important-warning">
          <span className="warning-label">【重要】</span>
          <span className="warning-text">
            注销账号是不可恢复的操作。注销前请确认您的资料已妥善处理。
          </span>
        </div>

        {/* 条款列表 */}
        <div className="notice-list">
          <div className="notice-item">
            <span className="notice-number">1.</span>
            <span className="notice-text">
              您将无法再登录和使用本账号(包括但不限于PC端、微信小程序、移动客户端等),也无法通过忘记密码操作来找回。
            </span>
          </div>

          <div className="notice-item">
            <span className="notice-number">2.</span>
            <span className="notice-text">
              您未完成的订单及服务将无法继续,也无法退款。
            </span>
          </div>

          <div className="notice-item">
            <span className="notice-number">3.</span>
            <span className="notice-text">
              账号中的余额、好豆、公益补贴券,进行中的退款或提现将视为您自行放弃。
            </span>
          </div>

          <div className="notice-item">
            <span className="notice-number">4.</span>
            <span className="notice-text">
              除法律法规要求必须保存的信息以外,您的用户信息我们将予以删除。您账号下所有行为信息记录,您将无法找回。
            </span>
          </div>

          <div className="notice-item">
            <span className="notice-number">5.</span>
            <span className="notice-text">
              注销账号并不代表本账号注销前的行为和相关责任得到豁免或减轻。
            </span>
          </div>
        </div>

        {/* 特别提示 */}
        <div className="special-notice">
          <div className="special-label">【特别提示】</div>
          <div className="special-text">
            当您按照注销页面提示填写信息、阅读并同意本《注销须知》及相关条款与条件且完成全部注销程序后,即表示您已充分阅读、理解并接受本《注销须知》的全部内容。阅读本《注销须知》的过程中,如果您不同意相关任何条款和条件约定,请您立即停止账号注销。
          </div>
        </div>
      </div>
    </PageLayout>
  );
}

