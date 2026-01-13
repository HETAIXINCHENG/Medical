import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function OrderDetail() {
  usePageStyles('order-detail.css');
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const shopName = searchParams.get('shopName') || '京药房旗舰店';
  const shopId = searchParams.get('shopId') || '';

  // 模拟订单详情数据
  const orderDetail = {
    id: id || 'o1',
    orderNo: '7905256897',
    status: '交易完成',
    statusDesc: '该订单已交易完成。',
    recipient: {
      name: '于明',
      phone: '18800066509',
      address: '广东省广州市番禺区东环街道领秀公馆 B625'
    },
    prescriptionInfo: {
      hasPrescription: true
    },
    store: {
      name: '京药房旗舰店',
      rating: 95
    },
    items: [
      {
        id: 'item1',
        type: 'product',
        image: '/Img/Director.png', // 使用默认图片
        name: '他克莫司软膏(普特彼)',
        spec: '0.1%*10g/支',
        manufacturer: 'LEO Pharma A/S',
        price: 149.00,
        quantity: 1
      },
      {
        id: 'item2',
        type: 'shipping',
        image: '/Img/Director.png', // 配送图标
        name: '顺丰京东当天急速广州发货',
        price: 15.00,
        quantity: 1
      }
    ],
    transaction: {
      totalAmount: 164.00,
      payAmount: 164.00
    }
  };

  const handleComplain = () => {
    navigate(`/customer-service?orderId=${id}&shopName=${encodeURIComponent(shopName)}`);
  };

  return (
    <PageLayout>
      <div className="order-detail-page">
        {/* 顶部导航 */}
        <div className="order-detail-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="返回" className="back-icon" />
          </button>
          <div className="header-title">订单详情</div>
          <div style={{ width: '32px' }}></div>
        </div>

        {/* 订单状态 */}
        <div className="order-status-section">
          <div className="status-title">{orderDetail.status}</div>
          <div className="status-desc">{orderDetail.statusDesc}</div>
        </div>

        {/* 收货信息 */}
        <div className="recipient-section">
          <div className="recipient-name-phone">
            {orderDetail.recipient.name} {orderDetail.recipient.phone}
          </div>
          <div className="recipient-address">
            地址: {orderDetail.recipient.address}
          </div>
        </div>

        {/* 处方信息 */}
        {orderDetail.prescriptionInfo.hasPrescription && (
          <div className="prescription-section">
            <div className="section-header">
              <span className="section-title">处方信息</span>
              <button className="view-link">查看&gt;</button>
            </div>
          </div>
        )}

        {/* 店铺信息 */}
        <div className="store-section">
          <div className="store-name">{orderDetail.store.name}</div>
          <div className="store-rating">
            用户好评率 {orderDetail.store.rating}% &gt;
          </div>
        </div>

        {/* 商品列表 */}
        <div className="items-section">
          {orderDetail.items.map((item) => (
            <div key={item.id} className="item-card">
              <img src={item.image} alt={item.name} className="item-image" />
              <div className="item-info">
                <div className="item-name">{item.name}</div>
                {item.spec && (
                  <div className="item-spec">{item.spec}</div>
                )}
                {item.manufacturer && (
                  <div className="item-manufacturer">{item.manufacturer}</div>
                )}
              </div>
              <div className="item-price-qty">
                <div className="item-price">¥{item.price.toFixed(2)}</div>
                <div className="item-qty">x{item.quantity}</div>
              </div>
            </div>
          ))}
        </div>

        {/* 交易信息 */}
        <div className="transaction-section">
          <div className="section-title">交易信息</div>
          <div className="transaction-row">
            <span className="transaction-label">订单总金额:</span>
            <span className="transaction-value">¥{orderDetail.transaction.totalAmount.toFixed(2)}</span>
          </div>
          <div className="transaction-row">
            <span className="transaction-label">实付金额:</span>
            <span className="transaction-value">¥{orderDetail.transaction.payAmount.toFixed(2)}</span>
          </div>
        </div>

        {/* 投诉商家浮动按钮 */}
        <button className="complain-float-btn" onClick={handleComplain}>
          投诉商家
        </button>
      </div>
    </PageLayout>
  );
}
