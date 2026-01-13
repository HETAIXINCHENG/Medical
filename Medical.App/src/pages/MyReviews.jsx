import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useNavigate } from 'react-router-dom';

const reviews = [
  {
    id: 'r1',
    time: '2022-10-16',
    doctor: '周元平',
    hospital: '南方医科大学第三附属医院 肝病科',
    disease: '乙肝小三阳，重度脂肪肝...',
    content: '',
    status: '发表成功',
    avatar: '/Img/doctor-50.png'
  },
  {
    id: 'r2',
    time: '2020-12-19',
    doctor: '青海涛',
    hospital: '南方医科大学南方医院 消化内科',
    disease: '',
    content: '医生很有耐心，对症下药',
    status: '发表成功',
    avatar: '/Img/doctor-50.png'
  }
];

export default function MyReviews() {
  usePageStyles('my-reviews.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      <div className="reviews-page">
        <div className="reviews-header">
          <button className="header-btn" onClick={() => navigate(-1)}>
            <img src="/Img/return.png" alt="back" />
          </button>
          <div className="header-title">我的诊后评价</div>
          <div className="header-placeholder" />
        </div>

        <div className="reviews-list">
          {reviews.map((item) => (
            <div key={item.id} className="review-card">
              <div className="card-top">
                <div className="time">评价时间: {item.time}</div>
                <div className="status success">{item.status}</div>
              </div>

              <div className="doctor-box">
                <img src={item.avatar} alt="avatar" className="avatar" />
                <div className="doctor-info">
                  <div className="doctor-name">{item.doctor}</div>
                  <div className="doctor-hospital">{item.hospital}</div>
                </div>
              </div>

              {item.disease && (
                <div className="row">
                  <div className="label">病情描述：</div>
                  <div className="value">{item.disease}</div>
                </div>
              )}

              {item.content && (
                <div className="row">
                  <div className="value">{item.content}</div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </PageLayout>
  );
}

