import { useMemo, useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

const tabList = [
  { key: 'pending', label: '待支付' },
  { key: 'in-progress', label: '进行中' },
  { key: 'completed', label: '已完成' },
  { key: 'cancelled', label: '已取消' }
];

const mockOrders = {
  pending: [
    {
      id: 'p1',
      title: '图文问诊',
      doctor: '林泳医生',
      patient: '于明',
      amount: 21.0,
      time: '2025-10-21 12:50:14',
      statusText: '待支付'
    }
  ],
  'in-progress': [
    {
      id: 'ip1',
      title: '图文问诊',
      doctor: '林泳医生',
      patient: '于明',
      amount: 21.0,
      time: '2025-10-21 12:50:14',
      statusText: '进行中'
    }
  ],
  completed: [
    {
      id: 'c1',
      title: '图文问诊',
      doctor: '林泳医生',
      patient: '于明',
      amount: 21.0,
      time: '2025-10-21 12:50:14',
      statusText: '已完成',
      canComment: true
    },
    {
      id: 'c2',
      title: '图文问诊',
      doctor: '林泳医生',
      patient: '于明',
      amount: 21.0,
      time: '2025-09-16 18:37:32',
      statusText: '已完成',
      hasComment: true
    }
  ],
  cancelled: [
    {
      id: 'x1',
      title: '医生赠送回复',
      doctor: '林泳医生',
      patient: '于明',
      amount: 0.0,
      time: '2025-07-14 21:09:27',
      statusText: '已取消'
    },
    {
      id: 'x2',
      title: '图文附赠回复',
      doctor: '张亚东医生',
      patient: '于明',
      amount: 0.0,
      time: '2025-04-06 10:10:23',
      statusText: '已取消'
    },
    {
      id: 'x3',
      title: '图文附赠回复',
      doctor: '黄晓雯医生',
      patient: '于明',
      amount: 0.0,
      time: '2024-10-21 11:17:51',
      statusText: '已取消'
    }
  ]
};

export default function MyOrders() {
  usePageStyles('my-orders.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialStatus = (() => {
    const s = searchParams.get('status');
    if (['pending', 'in-progress', 'completed', 'cancelled'].includes(s)) return s;
    return 'pending';
  })();
  const [selectedType, setSelectedType] = useState(initialStatus);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteOrderId, setDeleteOrderId] = useState(null);

  useEffect(() => {
    setSelectedType(initialStatus);
  }, [initialStatus]);

  const orders = useMemo(() => {
    return mockOrders[selectedType] || [];
  }, [selectedType]);

  const handleDeleteClick = (orderId) => {
    setDeleteOrderId(orderId);
    setShowDeleteModal(true);
  };

  const handleDeleteConfirm = () => {
    // 这里应该调用API删除订单
    // 暂时只是关闭对话框
    console.log('删除订单:', deleteOrderId);
    setShowDeleteModal(false);
    setDeleteOrderId(null);
    // TODO: 调用API删除订单后，刷新列表
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
    setDeleteOrderId(null);
  };

  const handleDetailClick = (orderId) => {
    navigate(`/order-detail/${orderId}`);
  };

  return (
    <PageLayout>
      <div className="my-orders-page">
        {/* 顶部导航 */}
        <div className="orders-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="back" />
          </button>
          <div className="header-title">订单列表</div>
          <button className="header-link">开发票</button>
        </div>

        {/* 顶部Tab */}
        <div className="orders-tabs">
          {tabList.map((tab) => (
            <button
              key={tab.key}
              className={`orders-tab-btn ${selectedType === tab.key ? 'active' : ''}`}
              onClick={() => setSelectedType(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* 订单列表/空状态 */}
        {orders.length === 0 ? (
          <div className="orders-empty">
            <div className="empty-icon">
              <img src="/Img/empty-order.png" alt="empty" />
            </div>
            <div className="empty-text">暂时还没有订单</div>
          </div>
        ) : (
          <div className="orders-list">
            {orders.map((order) => (
              <div key={order.id} className="order-card">
                <div className="order-card-header">
                  <div className="order-title">{order.title}</div>
                  <div className="order-status">{order.statusText}</div>
                </div>
                <div className="order-body">
                  <div className="order-row">
                    <span className="label">医生：</span>
                    <span className="value">{order.doctor}</span>
                  </div>
                  <div className="order-row">
                    <span className="label">患者：</span>
                    <span className="value">{order.patient}</span>
                  </div>
                  <div className="order-row">
                    <span className="label">实付金额：</span>
                    <span className="value highlight">¥{order.amount.toFixed(2)}</span>
                  </div>
                  <div className="order-row">
                    <span className="label">提交时间：</span>
                    <span className="value">{order.time}</span>
                  </div>
                </div>
                <div className="order-footer">
                  <button className="link-btn" onClick={() => handleDetailClick(order.id)}>详情</button>
                  {order.statusText === '已完成' && !order.hasComment && (
                    <button className="primary-btn">去评价</button>
                  )}
                  {order.statusText === '已完成' && order.hasComment && (
                    <button className="primary-btn">查看评价</button>
                  )}
                  {order.statusText === '已取消' && (
                    <button className="link-btn delete-btn" onClick={() => handleDeleteClick(order.id)}>删除</button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* 删除确认对话框 */}
        {showDeleteModal && (
          <div className="delete-modal-overlay" onClick={handleDeleteCancel}>
            <div className="delete-modal" onClick={(e) => e.stopPropagation()}>
              <div className="delete-modal-title">确认删除此订单?</div>
              <div className="delete-modal-buttons">
                <button className="delete-modal-cancel" onClick={handleDeleteCancel}>取消</button>
                <div className="delete-modal-divider"></div>
                <button className="delete-modal-confirm" onClick={handleDeleteConfirm}>删除</button>
              </div>
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
}

