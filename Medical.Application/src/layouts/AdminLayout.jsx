import { Layout, Menu, theme, Spin, Dropdown, Avatar, Space, Modal, Select, ColorPicker, Button } from 'antd';
import { UserOutlined, LogoutOutlined, LockOutlined, GlobalOutlined, BgColorsOutlined } from '@ant-design/icons';
import { useMemo, useState, useEffect } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import menuItems from '../config/menuConfig.js';
import httpClient from '../api/httpClient.js';
import { clearToken } from '../utils/auth.js';
import { message } from 'antd';
import { useLanguage } from '../contexts/LanguageContext.jsx';
import { useTheme } from '../contexts/ThemeContext.jsx';

const { Header, Sider, Content } = Layout;

const buildMenu = (navigate, allowedMenuKeys = null, t) => {
  // 菜单键到翻译键的映射
  const menuKeyToTranslation = {
    'dashboard': 'menu.dashboard',
    'basic-management': 'menu.basicManagement',
    'province-city': 'menu.provinceCity',
    'disease-categories': 'menu.diseaseCategories',
    'patient-management': 'menu.patientManagement',
    'patients': 'menu.patientInfo',
    'patient-info': 'menu.patientInfoLib',
    'doctor-management': 'menu.doctorManagement',
    'doctor-info': 'menu.doctorInfo',
    'patient-support-groups': 'menu.patientSupportGroups',
    'department-management': 'menu.departmentManagement',
    'department-info': 'menu.departmentInfo',
    'knowledge-activity': 'menu.knowledgeActivity',
    'activities': 'menu.activities',
    'health-knowledge': 'menu.healthKnowledge',
    'post-management': 'menu.postManagement',
    'posts': 'menu.posts',
    'post-comments': 'menu.postComments',
    'post-likes': 'menu.postLikes',
    'consultation': 'menu.consultation',
    'questions': 'menu.questions',
    'consultations': 'menu.consultations',
    'treatment-management': 'menu.treatmentManagement',
    'visit-records': 'menu.visitRecords',
    'examination-reports': 'menu.examinationReports',
    'prescriptions': 'menu.prescriptions',
    'my-prescriptions': 'menu.myPrescriptions',
    'drug-management': 'menu.drugManagement',
    'drug-categories': 'menu.drugCategories',
    'drugs': 'menu.drugs',
    'drug-inventories': 'menu.drugInventories',
    'drug-stock-ins': 'menu.drugStockIns',
    'system-management': 'menu.systemManagement',
    'system-users': 'menu.systemUsers',
    'system-roles': 'menu.systemRoles',
    'permission-config': 'menu.permissionConfig',
    'system-permissions': 'menu.systemPermissions',
    'feedbacks': 'menu.feedbacks',
    'mall': 'menu.mall',
    'product-categories': 'menu.productCategories',
    'products': 'menu.products',
    'cart-tree': 'menu.cartTree',
    'carts': 'menu.carts',
    'orders': 'menu.orders',
    'shipments': 'menu.shipments',
    'ship-companies': 'menu.shipCompanies',
    'tertiary-hospitals': 'menu.tertiaryHospitals',
    'financial-management': 'menu.financialManagement',
    'financial-receivables': 'menu.financialReceivables',
    'financial-payables': 'menu.financialPayables',
    'financial-fees': 'menu.financialFees',
    'financial-settlements': 'menu.financialSettlements'
  };

  // 如果没有权限限制，显示所有菜单
  const hasPermission = (menuKey) => {
    if (allowedMenuKeys === null) return true; // 未加载权限时显示所有菜单
    return allowedMenuKeys.includes(menuKey);
  };

  const getMenuLabel = (key, defaultLabel) => {
    const translationKey = menuKeyToTranslation[key];
    return translationKey ? t(translationKey) : defaultLabel;
  };

  return menuItems
    .map((item) => {
      const Icon = item.icon;
      if (item.children) {
        // 过滤子菜单
        const filteredChildren = item.children.filter(child => hasPermission(child.key));
        if (filteredChildren.length === 0) {
          return null; // 如果没有可访问的子菜单，不显示父菜单
        }
        return {
          key: item.key,
          icon: Icon ? <Icon /> : null,
          label: getMenuLabel(item.key, item.label),
          children: filteredChildren.map((child) => {
            const ChildIcon = child.icon;
            return {
              key: child.path,
              icon: ChildIcon ? <ChildIcon /> : null,
              label: getMenuLabel(child.key, child.label),
              onClick: () => navigate(child.path)
            };
          })
        };
      }

      // 检查单个菜单项权限
      if (!hasPermission(item.key)) {
        return null;
      }

      return {
        key: item.path,
        icon: Icon ? <Icon /> : null,
        label: getMenuLabel(item.key, item.label),
        onClick: () => navigate(item.path)
      };
    })
    .filter(item => item !== null); // 移除null项
};

export default function AdminLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const {
    token: { colorBgContainer }
  } = theme.useToken();
  const { language, changeLanguage, t } = useLanguage();
  const { primaryColor, changeColor } = useTheme();

  const [allowedMenuKeys, setAllowedMenuKeys] = useState(null);
  const [loading, setLoading] = useState(true);
  const [currentUser, setCurrentUser] = useState(null);
  const [languageModalOpen, setLanguageModalOpen] = useState(false);
  const [themeModalOpen, setThemeModalOpen] = useState(false);
  const [selectedLanguage, setSelectedLanguage] = useState(language);
  const [selectedColor, setSelectedColor] = useState(primaryColor);

  // 获取当前用户信息
  useEffect(() => {
    const fetchCurrentUser = async () => {
      try {
        const user = await httpClient.get('/api/users/me');
        setCurrentUser(user);
      } catch (error) {
        console.error('获取用户信息失败:', error);
      }
    };

    fetchCurrentUser();
  }, []);

  // 获取用户有权限的菜单列表
  useEffect(() => {
    const fetchUserMenus = async () => {
      try {
        const menus = await httpClient.get('/api/menupermissions/my-menus');
        // 如果返回 null，表示是 Admin 或 SuperAdmin，显示所有菜单
        if (menus === null) {
          setAllowedMenuKeys(null);
        } else if (Array.isArray(menus)) {
          const menuKeys = menus.map(m => m.key);
          setAllowedMenuKeys(menuKeys);
        } else {
          setAllowedMenuKeys(null);
        }
      } catch (error) {
        console.error('获取用户菜单权限失败:', error);
        // 如果获取失败，显示所有菜单（降级处理）
        setAllowedMenuKeys(null);
      } finally {
        setLoading(false);
      }
    };

    fetchUserMenus();
  }, []);

  const items = useMemo(() => buildMenu(navigate, allowedMenuKeys, t), [navigate, allowedMenuKeys, t]);

  if (loading) {
    return (
      <Layout className="admin-layout">
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
          <Spin size="large" />
        </div>
      </Layout>
    );
  }

  return (
    <Layout className="admin-layout">
      <Sider width={220} breakpoint="lg" collapsedWidth="0">
        <div className="brand">
          <span className="brand-mark">M</span>
          <span className="brand-name">Medical System</span>
        </div>
        <style>
          {`
            .admin-layout .ant-menu-dark.ant-menu-inline .ant-menu-item-selected,
            .admin-layout .ant-menu-dark.ant-menu-inline .ant-menu-submenu-selected > .ant-menu-submenu-title {
              background-color: ${primaryColor} !important;
            }
            .admin-layout .ant-menu-dark.ant-menu-inline .ant-menu-item:hover,
            .admin-layout .ant-menu-dark.ant-menu-inline .ant-menu-submenu-title:hover {
              background-color: ${primaryColor}33 !important;
            }
          `}
        </style>
        <Menu
          mode="inline"
          theme="dark"
          selectedKeys={[location.pathname]}
          defaultOpenKeys={['management']}
          items={items}
        />
      </Sider>
      <Layout>
        <Header className="admin-header" style={{ 
          background: colorBgContainer,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '0 24px'
        }}>
          <span style={{ fontSize: '20px', fontWeight: 700, letterSpacing: '0.5px' }}>
            {t('system.title')}
          </span>
          <Dropdown
            menu={{
              items: [
                {
                  key: 'language-settings',
                  icon: <GlobalOutlined />,
                  label: t('user.languageSettings')
                },
                {
                  key: 'custom-style',
                  icon: <BgColorsOutlined />,
                  label: t('user.customStyle')
                },
                {
                  type: 'divider'
                },
                {
                  key: 'change-password',
                  icon: <LockOutlined />,
                  label: t('user.changePassword')
                },
                {
                  type: 'divider'
                },
                {
                  key: 'logout',
                  icon: <LogoutOutlined />,
                  label: t('user.logout'),
                  danger: true
                }
              ],
              onClick: ({ key }) => {
                if (key === 'logout') {
                  clearToken();
                  message.success(t('system.logoutSuccess'));
                  navigate('/login', { replace: true });
                } else if (key === 'change-password') {
                  navigate('/change-password');
                } else if (key === 'language-settings') {
                  setSelectedLanguage(language);
                  setLanguageModalOpen(true);
                } else if (key === 'custom-style') {
                  setSelectedColor(primaryColor);
                  setThemeModalOpen(true);
                }
              }
            }}
            placement="bottomRight"
          >
            <Space style={{ cursor: 'pointer' }}>
              <Avatar 
                src={currentUser?.avatarUrl ? 
                  (currentUser.avatarUrl.startsWith('http') ? 
                    currentUser.avatarUrl : 
                    `${import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'}${currentUser.avatarUrl.startsWith('/') ? currentUser.avatarUrl : '/' + currentUser.avatarUrl}`
                  ) : undefined
                }
                icon={!currentUser?.avatarUrl && <UserOutlined />}
                size="small"
              />
              <span>{currentUser?.username || '用户'}</span>
            </Space>
          </Dropdown>
        </Header>
        <Content className="admin-content">
          <Outlet />
        </Content>
      </Layout>
      
      {/* 语言设置模态框 */}
      <Modal
        key={`language-modal-${language}`}
        title={t('language.title')}
        open={languageModalOpen}
        onOk={() => {
          changeLanguage(selectedLanguage);
          setLanguageModalOpen(false);
          message.success(t('system.saveSuccess'));
        }}
        onCancel={() => {
          setLanguageModalOpen(false);
          setSelectedLanguage(language);
        }}
        okText={t('common.confirm')}
        cancelText={t('common.cancel')}
      >
        <Select
          style={{ width: '100%' }}
          value={selectedLanguage}
          onChange={setSelectedLanguage}
          options={[
            { label: '中文简体', value: 'zh-CN' },
            { label: '中文繁體', value: 'zh-TW' },
            { label: 'English', value: 'en' }
          ]}
          placeholder={t('language.select')}
        />
      </Modal>
      
      {/* 主题颜色设置模态框 */}
      <Modal
        key={`theme-modal-${language}`}
        title={t('theme.title')}
        open={themeModalOpen}
        onOk={() => {
          changeColor(selectedColor);
          setThemeModalOpen(false);
          message.success(t('system.saveSuccess'));
        }}
        onCancel={() => {
          setThemeModalOpen(false);
          setSelectedColor(primaryColor);
        }}
        okText={t('common.confirm')}
        cancelText={t('common.cancel')}
      >
        <div style={{ marginBottom: 16 }}>
          <label style={{ display: 'block', marginBottom: 8 }}>{t('theme.primaryColor')}</label>
          <ColorPicker
            value={selectedColor}
            onChange={(color, hex) => {
              setSelectedColor(hex);
            }}
            showText
            format="hex"
            style={{ width: '100%' }}
          />
        </div>
        <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginTop: 16 }}>
          {['#0d9488', '#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1', '#eb2f96', '#000000'].map((color) => (
            <Button
              key={color}
              type={selectedColor === color ? 'primary' : 'default'}
              style={{
                backgroundColor: color,
                borderColor: color,
                width: 40,
                height: 40,
                padding: 0
              }}
              onClick={() => setSelectedColor(color)}
            />
          ))}
        </div>
      </Modal>
    </Layout>
  );
}

