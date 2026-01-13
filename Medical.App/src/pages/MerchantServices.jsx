import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

// 模拟商家服务数据
const mockMerchants = [
  {
    id: 'merchant1',
    shopName: '京药房旗舰店',
    time: '2024-01-22 22:51',
    patientMessage: '患者:有问题可以联系我~'
  },
  {
    id: 'merchant2',
    shopName: '好大夫合作药房-贝美森',
    time: '2023-08-19 22:56',
    patientMessage: '患者:有问题可以联系我~'
  },
  {
    id: 'merchant3',
    shopName: '五立大药房',
    time: '2023-05-30 09:30',
    patientMessage: '患者:有问题可以联系我~'
  },
  {
    id: 'merchant4',
    shopName: '家之和连锁药店钟村分店',
    time: '2022-12-18 21:45',
    patientMessage: '患者:现在药还没有送到.'
  }
];

export default function MerchantServices() {
  usePageStyles('merchant-services.css');
  const navigate = useNavigate();

  const handleMerchantClick = (merchant) => {
    navigate(`/merchant-orders?shopId=${merchant.id}&shopName=${encodeURIComponent(merchant.shopName)}`);
  };

  const handleChatClick = (e, merchant) => {
    e.stopPropagation();
    navigate(`/merchant-chat?shopId=${merchant.id}&shopName=${encodeURIComponent(merchant.shopName)}`);
  };

  return (
    <PageLayout>
      <div className="merchant-services-page">
        {/* 顶部导航 */}
        <div className="merchant-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="返回" className="back-icon" />
          </button>
          <div className="header-title">商家服务</div>
          <div style={{ width: '32px' }}></div>
        </div>

        {/* 商家服务列表 */}
        <div className="merchant-list">
          {mockMerchants.map((merchant) => (
            <div
              key={merchant.id}
              className="merchant-card"
              onClick={() => handleMerchantClick(merchant)}
            >
              <div className="merchant-card-header">
                <span className="merchant-title">商家服务</span>
                <span className="merchant-time">{merchant.time}</span>
              </div>
              <div className="merchant-shop-name">{merchant.shopName}</div>
              <div className="merchant-patient-message">{merchant.patientMessage}</div>
              <button
                className="merchant-chat-btn"
                onClick={(e) => handleChatClick(e, merchant)}
              >
                去交流
              </button>
            </div>
          ))}
        </div>
      </div>
    </PageLayout>
  );
}

