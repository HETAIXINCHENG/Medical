import { useState } from 'react';
import { Card, Form, Input, Button, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import httpClient from '../../api/httpClient.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';
import './login.css';

export default function ChangePassword() {
  const navigate = useNavigate();
  const { t } = useLanguage();
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();

  const onFinish = async (values) => {
    const { oldPassword, newPassword, confirmPassword } = values;

    if (newPassword !== confirmPassword) {
      message.error(t('password.mismatch'));
      return;
    }

    if (newPassword.length < 6) {
      message.error(t('password.minLength'));
      return;
    }

    setLoading(true);
    try {
      await httpClient.put('/api/users/me/password', {
        oldPassword,
        newPassword
      });
      message.success(t('password.success'));
      setTimeout(() => {
        navigate('/login', { replace: true });
      }, 1500);
    } catch (error) {
      message.error(error.message || t('password.failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-root">
      <Card className="login-card" style={{ maxWidth: 500 }}>
        <div className="login-header">
          <h2 style={{ margin: 0 }}>{t('password.title')}</h2>
        </div>
        <Form
          form={form}
          layout="vertical"
          onFinish={onFinish}
          style={{ marginTop: 24 }}
        >
          <Form.Item
            name="oldPassword"
            label={t('password.oldPassword')}
            rules={[{ required: true, message: t('password.oldPasswordRequired') }]}
          >
            <Input.Password placeholder={t('password.oldPasswordPlaceholder')} />
          </Form.Item>

          <Form.Item
            name="newPassword"
            label={t('password.newPassword')}
            rules={[
              { required: true, message: t('password.newPasswordRequired') },
              { min: 6, message: t('password.minLength') }
            ]}
          >
            <Input.Password placeholder={t('password.newPasswordPlaceholder')} />
          </Form.Item>

          <Form.Item
            name="confirmPassword"
            label={t('password.confirmPassword')}
            dependencies={['newPassword']}
            rules={[
              { required: true, message: t('password.confirmPasswordRequired') },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error(t('password.mismatch')));
                }
              })
            ]}
          >
            <Input.Password placeholder={t('password.confirmPasswordPlaceholder')} />
          </Form.Item>

          <Form.Item>
            <Button type="primary" htmlType="submit" block loading={loading}>
              {t('password.submit')}
            </Button>
          </Form.Item>

          <Form.Item>
            <Button block onClick={() => navigate(-1)}>
              {t('common.cancel')}
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
}

