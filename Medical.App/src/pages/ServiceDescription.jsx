import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function ServiceDescription() {
  usePageStyles('service-description.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const type = searchParams.get('type') || 'phone'; // phone 或 text

  const isPhoneService = type === 'phone';

  const serviceData = isPhoneService
    ? {
        title: '电话服务说明',
        sections: [
          {
            title: '一、适用情况',
            items: [
              '电话服务可以和医生直接通话',
              '适用于病情相对着急的患者',
              '建议您通话前提交完整的病历资料并准备好自己的问题'
            ]
          },
          {
            title: '二、问诊方式',
            items: ['和医生本人进行一对一电话沟通']
          },
          {
            title: '三、服务流程',
            items: [
              '提交详细的病情信息以及相关检查资料',
              '付费后，需要您耐心等待医生接诊并安排通话时间',
              '医生接诊后会通过平台联系您，请注意接听010来电',
              '通话完毕后医生给出问诊建议，服务结束'
            ]
          },
          {
            title: '四、注意事项',
            items: [
              '您提交的病情资料会经过审核，如不符合医生专业要求将为您退费',
              '医生可以选择不接诊，不接诊将给您退费',
              '通话时间由医生主导确认，网站也会一直跟进尽快通话'
            ]
          }
        ]
      }
    : {
        title: '图文服务说明',
        sections: [
          {
            title: '一、适用情况',
            items: [
              '图文服务通过文字和图片与医生进行交流',
              '适合可以耐心等待医生回复的病情咨询',
              '建议您提交完整的病历资料和检查报告'
            ]
          },
          {
            title: '二、问诊方式',
            items: ['通过平台与医生进行文字、图片交流，可随时补充资料']
          },
          {
            title: '三、服务流程',
            items: [
              '提交详细的病情信息以及相关检查资料',
              '付费后，医生会在2天内回复您的咨询',
              '您可以随时补充病情资料和问题',
              '医生给出问诊建议后，服务结束'
            ]
          },
          {
            title: '四、注意事项',
            items: [
              '您提交的病情资料会经过审核，如不符合医生专业要求将为您退费',
              '医生可以选择不接诊，不接诊将给您退费',
              '请在服务有效期内完成咨询，过期将无法继续使用'
            ]
          }
        ]
      };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="service-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">服务说明</h1>
      </div>

      {/* 内容区域 */}
      <div className="service-content">
        <h2 className="service-title">· {serviceData.title} ·</h2>

        {serviceData.sections.map((section, index) => (
          <div key={index} className="section">
            <h3 className="section-title">{section.title}</h3>
            <ol className="section-list">
              {section.items.map((item, itemIndex) => (
                <li key={itemIndex} className="section-item">
                  {item}
                </li>
              ))}
            </ol>
          </div>
        ))}
      </div>
    </PageLayout>
  );
}

