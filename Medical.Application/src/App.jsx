import { Navigate, Route, Routes } from 'react-router-dom';
import AdminLayout from './layouts/AdminLayout.jsx';
import Dashboard from './pages/dashboard/Dashboard.jsx';
import ResourcePage from './pages/resources/ResourcePage.jsx';
import ConsultationMessages from './pages/consultations/ConsultationMessages.jsx';
import PermissionConfig from './pages/permission/PermissionConfig.jsx';
import ChangePassword from './pages/auth/ChangePassword.jsx';
import ProvinceCityPage from './pages/basic/ProvinceCityPage.jsx';
import DiseaseCategoryTreePage from './pages/basic/DiseaseCategoryTreePage.jsx';
import CartTreePage from './pages/mall/CartTreePage.jsx';
import resourceConfig from './config/resourceConfig.js';
import ProtectedRoute from './routes/ProtectedRoute.jsx';
import Login from './pages/auth/Login.jsx';
import './App.css';

function App() {
  return (
    <Routes>
      {/* 登录页为公开路由，未登录时可直接访问 */}
      <Route path="/login" element={<Login />} />

      {/* 受保护路由，必须登录后才能访问 */}
      <Route element={<ProtectedRoute />}>
        {/* 后台布局，承载具体页面 */}
        <Route element={<AdminLayout />}>
          {/* 默认跳转至仪表盘 */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<Dashboard />} />

          {/* 根据 resourceConfig 自动生成 CRUD 菜单对应的路由 */}
          {resourceConfig.map((resource) => (
            <Route
              key={resource.key}
              path={`/resources/${resource.key}`}
              element={<ResourcePage resource={resource} />}
            />
          ))}

          {/* 咨询消息管理路由 */}
          <Route
            path="/consultations/messages/:consultationId"
            element={<ConsultationMessages />}
          />

          {/* 权限配置路由 */}
          <Route
            path="/permission-config"
            element={<PermissionConfig />}
          />

          {/* 省市信息路由 */}
          <Route
            path="/province-city"
            element={<ProvinceCityPage />}
          />

          {/* 疾病分类（树形） */}
          <Route
            path="/disease-categories-tree"
            element={<DiseaseCategoryTreePage />}
          />

          {/* 购物车明细（树形） */}
          <Route
            path="/cart-tree"
            element={<CartTreePage />}
          />

          {/* 修改密码路由 */}
          <Route
            path="/change-password"
            element={<ChangePassword />}
          />

          {/* 兜底：未知路径重定向到仪表盘 */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
