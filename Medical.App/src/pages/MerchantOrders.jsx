import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

// 模拟订单数据
const mockOrders = [
  {
    id: 'o1',
    orderNo: '7905256897',
    statusText: '已收货',
    productSummary: '他克莫司软膏(普特彼)、顺丰京东当...',
    amount: 164.00,
    time: '2024-01-22 22:50:51'
  }
];

export default function MerchantOrders() {
  usePageStyles('merchant-orders.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const shopName = searchParams.get('shopName') || '商家';
  const shopId = searchParams.get('shopId') || '';

  const [orders] = useState(mockOrders);

  return (
    <PageLayout>
      <div className="merchant-orders-page">
        {/* 顶部导航 */}
        <div className="merchant-orders-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="返回" className="back-icon" />
          </button>
          <div className="header-title">{shopName}</div>
          <div style={{ width: '32px' }}></div>
        </div>

        {/* 订单列表标题 */}
        <div className="orders-list-title">
          <div className="title-bar"></div>
          <span className="title-text">我的订单列表</span>
        </div>

        {/* 订单列表 */}
        <div className="orders-content">
          {orders.length === 0 ? (
            <div className="orders-empty">
              <div className="empty-text">暂无订单</div>
            </div>
          ) : (
            <div className="orders-list">
              {orders.map((order) => (
                <div key={order.id} className="order-card">
                  <div className="order-header">
                    <div className="order-info">
                      <span className="order-label">订单号:</span>
                      <span className="order-value">{order.orderNo}</span>
                      <button className="copy-btn">复制</button>
                    </div>
                    <div className="order-status">{order.statusText}</div>
                  </div>
                  <div className="order-body">
                    <div className="order-row">
                      <span className="order-label">商品:</span>
                      <span className="order-value">{order.productSummary}</span>
                    </div>
                    <div className="order-row">
                      <span className="order-label">金额:</span>
                      <span className="order-value highlight">¥{order.amount.toFixed(2)}</span>
                    </div>
                    <div className="order-row">
                      <span className="order-label">下单时间:</span>
                      <span className="order-value">{order.time}</span>
                    </div>
                  </div>
                  <div className="order-footer">
                    <button
                      className="view-detail-btn"
                      onClick={() => navigate(`/order-detail/${order.id}?shopName=${encodeURIComponent(shopName)}&shopId=${shopId}`)}
                    >
                      查看详情
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* 底部操作栏 */}
        <div className="bottom-actions">
          <div className="action-left">
            <span className="service-desk">服务台</span>
            <span className="complain-link">投诉商家&gt;</span>
          </div>
          <button
            className="chat-btn"
            onClick={() => navigate(`/merchant-chat?shopId=${shopId}&shopName=${encodeURIComponent(shopName)}`)}
          >
            去交流
          </button>
        </div>
      </div>
    </PageLayout>
  );
}

