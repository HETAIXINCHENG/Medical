import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useNavigate } from 'react-router-dom';

const prescriptions = [
  {
    id: 'p1',
    doctor: '兰海梅',
    hospital: '南方医科大学南方医院 皮肤科',
    patient: '于明',
    diagnose: '必理通重组人表皮生长因子型医用冷敷凝胶',
    type: '治疗用品单',
    applyDate: '2024-01-22',
    status: '未购买',
    statusType: 'warning',
    actions: ['立即购买', '查看详情']
  },
  {
    id: 'p2',
    doctor: '兰海梅',
    hospital: '南方医科大学南方医院 皮肤科',
    patient: '于明',
    diagnose: '脂溢性皮炎',
    drug: '他克莫司软膏(普特彼)',
    type: '处方',
    applyDate: '2024-01-22',
    status: '审核通过',
    statusType: 'success',
    actions: ['查看详情']
  },
  {
    id: 'p3',
    doctor: '兰海梅',
    hospital: '南方医科大学南方医院 皮肤科',
    patient: '于明',
    diagnose: '毛囊炎',
    drug: '伊曲康唑胶囊(斯皮仁诺)',
    type: '处方',
    applyDate: '2023-08-19',
    status: '审核通过',
    statusType: 'success',
    actions: ['查看详情']
  }
];

export default function MyPrescriptions() {
  usePageStyles('my-prescriptions.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      <div className="prescription-page">
        <div className="prescription-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="back" />
          </button>
          <div className="header-title">我的处方</div>
          <div className="header-placeholder" />
        </div>

        <div className="prescription-list">
          {prescriptions.map((item) => (
            <div key={item.id} className="prescription-card">
              <div className="card-top">
                <div className="title-row">
                  <div className="title">医生：{item.doctor}</div>
                </div>
                <div className="hospital">{item.hospital}</div>
              </div>

              <div className="card-body">
                <div className="row">
                  <span className="label">患者：</span>
                  <span className="value">{item.patient}</span>
                </div>
                <div className="row">
                  <span className="label">诊断：</span>
                  <span className="value">{item.diagnose}</span>
                </div>
                {item.drug && (
                  <div className="row">
                    <span className="label">药品详情：</span>
                    <span className="value">{item.drug}</span>
                  </div>
                )}
                <div className="row">
                  <span className="label">开单时间：</span>
                  <span className="value">{item.applyDate}</span>
                </div>
              </div>

                <div className="card-footer">
                  <span className={`status ${item.statusType}`}>{item.status}</span>
                  <div className="actions">
                    {item.actions.includes('立即购买') && (
                      <button className="primary-btn">立即购买</button>
                    )}
                    <button
                      className="ghost-btn"
                      onClick={() => navigate(`/prescription-detail/${item.id}`)}
                    >
                      查看详情
                    </button>
                  </div>
                </div>
            </div>
          ))}
        </div>
      </div>
    </PageLayout>
  );
}

