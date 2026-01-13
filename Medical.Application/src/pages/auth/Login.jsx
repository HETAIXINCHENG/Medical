import { Button, Card, Form, Input, Typography, message } from 'antd';
import { useNavigate, useLocation } from 'react-router-dom';
import { setToken } from '../../utils/auth.js';
import httpClient from '../../api/httpClient.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';
import './login.css';

const { Title, Paragraph } = Typography;

export default function Login() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useLanguage();

  const onFinish = async (values) => {
    const { username, password } = values;
    try {
      const response = await httpClient.post('/api/adminauth/login', {
        username,
        password
      });
      
      if (response.token) {
        setToken(response.token);
        message.success(t('system.loginSuccess'));
        const redirectPath = location.state?.from?.pathname ?? '/dashboard';
        navigate(redirectPath, { replace: true });
      } else {
        message.error(t('login.failed'));
      }
    } catch (error) {
      message.error(error.message || t('login.error'));
    }
  };

  return (
    <div className="login-root">
      <Card className="login-card">
        <div className="login-header">
          <Title level={3}>{t('login.title')}</Title>
          <Paragraph type="secondary">{t('login.subtitle')}</Paragraph>
        </div>
        <Form layout="vertical" onFinish={onFinish}>
          <Form.Item
            name="username"
            label={t('login.username')}
            rules={[{ required: true, message: t('login.usernameRequired') }]}
            initialValue="admin"
          >
            <Input placeholder={t('login.usernamePlaceholder')} />
          </Form.Item>
          <Form.Item
            name="password"
            label={t('login.password')}
            rules={[{ required: true, message: t('login.passwordRequired') }]}
            initialValue="123456"
          >
            <Input.Password placeholder={t('login.passwordPlaceholder')} />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" block>
              {t('login.submit')}
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}


