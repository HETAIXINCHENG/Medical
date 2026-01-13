import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function ForgotPassword() {
  usePageStyles('forgot-password.css');
  const navigate = useNavigate();
  const [phone, setPhone] = useState('');
  const [code, setCode] = useState('');
  const [countdown, setCountdown] = useState(0);
  const [error, setError] = useState('');

  const handleGetCode = async () => {
    if (!phone) {
      setError('请输入手机号');
      return;
    }
    if (!/^1[3-9]\d{9}$/.test(phone)) {
      setError('请输入正确的手机号');
      return;
    }
    
    // TODO: 调用API发送验证码
    // await medicalApi.sendVerificationCode({ phone });
    
    // 开始倒计时
    setCountdown(60);
    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const handleNext = () => {
    if (!phone) {
      setError('请输入手机号');
      return;
    }
    if (!code) {
      setError('请输入验证码');
      return;
    }
    
    // TODO: 验证验证码
    // 验证通过后跳转到重置密码页面
    navigate(`/reset-password?phone=${phone}&code=${code}`);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="password-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">修改密码</h1>
      </div>

      {/* 内容区域 */}
      <div className="password-content">
        <h2 className="form-title">重置密码:</h2>
        
        <div className="form-group">
          <input
            type="text"
            className="form-input"
            placeholder="请输入手机号"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            maxLength={11}
          />
        </div>

        <div className="form-group code-group">
          <input
            type="text"
            className="form-input code-input"
            placeholder="请输入手机验证码"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            maxLength={6}
          />
          <button
            type="button"
            className="code-btn"
            onClick={handleGetCode}
            disabled={countdown > 0}
          >
            {countdown > 0 ? `${countdown}秒` : '获取验证短信'}
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <button
          type="button"
          className="next-btn"
          onClick={handleNext}
          disabled={!phone || !code}
        >
          下一步
        </button>
      </div>
    </PageLayout>
  );
}

