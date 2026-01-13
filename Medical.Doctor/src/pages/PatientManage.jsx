import BottomNav from '../components/BottomNav.jsx';
import './patient-manage.css';

export default function PatientManage() {
  return (
    <div className="page patient-page">
      <div className="patient-topbar">
        <div className="placeholder" />
        <div className="top-title">患者管理</div>
        <div className="placeholder" />
      </div>

      <div className="card quick-actions">
        <div className="quick-item">
          <div className="quick-icon">➕</div>
          <div className="quick-text">群发信息</div>
        </div>
        <div className="quick-item">
          <div className="quick-icon">🗂️</div>
          <div className="quick-text">标签管理</div>
        </div>
      </div>

      <div className="card stats-card">
        <div className="tabs within">
          <button className="tab active">本周</button>
          <button className="tab">本月</button>
          <button className="tab">累计</button>
        </div>

        <div className="stats-group">
          <div className="group-title">患者统计</div>
          <div className="stats-row">
            <div className="stat">
              <div className="stat-label">新增患者</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">复诊人数</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">复诊率</div>
              <div className="stat-value teal">0%</div>
            </div>
          </div>
        </div>

        <div className="stats-group">
          <div className="group-title">购药统计</div>
          <div className="stats-row">
            <div className="stat">
              <div className="stat-label">购药次数</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">购药金额</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">购药客单价</div>
              <div className="stat-value">0</div>
            </div>
          </div>
        </div>

        <div className="stats-group">
          <div className="group-title">服务包人数</div>
          <div className="stats-row">
            <div className="stat">
              <div className="stat-label">新增人数</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">续费人数</div>
              <div className="stat-value">0</div>
            </div>
            <div className="stat">
              <div className="stat-label">失效人数</div>
              <div className="stat-value">0</div>
            </div>
          </div>
        </div>

        <button className="primary-btn wide">展开查看更多数据</button>
      </div>

      <div className="card filter-card">
        <div className="filter-tabs">
          <button className="tab active">全部</button>
          <button className="tab ghost">会员患者</button>
        </div>
        <div className="search-box">
          <input type="text" placeholder="搜索患者姓名" />
        </div>
      </div>

      <BottomNav />
    </div>
  );
}

