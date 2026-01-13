import { useRef } from 'react';
import html2canvas from 'html2canvas';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useNavigate, useParams } from 'react-router-dom';

export default function PrescriptionDetail() {
  usePageStyles('my-prescription-detail.css');
  const navigate = useNavigate();
  const { id } = useParams();
  const cardRef = useRef(null);

  // 模拟详情数据
  const detail = {
    hospital: 'XXXXXX互联网医院',
    patientName: '于明',
    gender: '男',
    age: '40岁',
    weight: '85kg',
    department: '皮肤科',
    diagnose: '脂溢性皮炎',
    date: '2024-01-22',
    prescriptionNo: 'NO.8959993902',
    items: [
      {
        name: '他克莫司软膏 0.1%*10g/支',
        qty: '1盒',
        usage: '外用 适量 每日两次 用药30天',
        tips: '每天二次，坚持10天以上有效。'
      }
    ],
    doctor: '兰海梅',
    review: '汪少南',
    remark: [
      '处方有效期为3天。',
      '本互联网医院无自建药房。',
      '目前与本院对接的药房有379家，凭本处方您可自行选择线上药房购药。',
      '支持凭本处方到线下药店购药或将本处方上传至其他网上购药平台（如京东、阿里等）找医生抄方后购药。',
      '如需医保报销，您需凭本处方到附近线下实体医院抄方购药。'
    ]
  };

  const handleDownload = async () => {
    if (!cardRef.current) return;

    try {
      // 使用html2canvas将处方卡片转换为canvas
      const canvas = await html2canvas(cardRef.current, {
        backgroundColor: '#ffffff',
        scale: 2, // 提高图片清晰度
        useCORS: true,
        logging: false
      });

      // 将canvas转换为blob
      canvas.toBlob((blob) => {
        if (!blob) {
          alert('生成图片失败，请重试');
          return;
        }

        // 创建下载链接
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `处方_${detail.prescriptionNo}_${detail.date}.jpg`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
      }, 'image/jpeg', 0.95);
    } catch (error) {
      console.error('下载失败:', error);
      alert('下载失败，请重试');
    }
  };

  return (
    <PageLayout>
      <div className="rx-detail-page">
        <div className="rx-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="back" />
          </button>
          <div className="header-title">医生建议</div>
          <button className="header-link" onClick={handleDownload}>下载</button>
        </div>

        <div className="rx-card" ref={cardRef}>
          <div className="rx-hospital">{detail.hospital}</div>
          <div className="rx-meta">
            <span className="meta-id">{detail.prescriptionNo}</span>
            <span className="meta-date">{detail.date}</span>
          </div>

          <div className="rx-row">
            <span className="label">姓名：</span>
            <span className="value">{detail.patientName}</span>
            <span className="label">性别：</span>
            <span className="value">{detail.gender}</span>
          </div>
          <div className="rx-row">
            <span className="label">年龄：</span>
            <span className="value">{detail.age}</span>
            <span className="label">体重：</span>
            <span className="value">{detail.weight}</span>
          </div>
          <div className="rx-row">
            <span className="label">科室：</span>
            <span className="value">{detail.department}</span>
          </div>
          <div className="rx-row">
            <span className="label">诊断：</span>
            <span className="value">{detail.diagnose}</span>
          </div>

          <div className="rx-section-title">Rp:</div>
          <div className="rx-items">
            {detail.items.map((item, idx) => (
              <div key={idx} className="rx-item">
                <div className="item-name">{idx + 1}. {item.name}</div>
                <div className="item-row">数量：{item.qty}</div>
                <div className="item-row">用法用量：{item.usage}</div>
                <div className="item-row">用药医嘱：{item.tips}</div>
              </div>
            ))}
          </div>

          <div className="rx-row">
            <span className="label">处方医生：</span>
            <span className="value badge">{detail.doctor}</span>
          </div>
          <div className="rx-row">
            <span className="label">审核：</span>
            <span className="value badge">{detail.review}</span>
          </div>

          <div className="rx-remark-title">备注：</div>
          <ol className="rx-remark">
            {detail.remark.map((t, idx) => (
              <li key={idx}>{t}</li>
            ))}
          </ol>
        </div>
      </div>
    </PageLayout>
  );
}

