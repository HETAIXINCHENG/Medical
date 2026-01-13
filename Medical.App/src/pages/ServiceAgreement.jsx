import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function ServiceAgreement() {
  usePageStyles('service-agreement.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="agreement-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">服务协议</h1>
      </div>

      {/* 内容区域 */}
      <div className="agreement-content">
        <div className="content-wrapper">
          <h2 className="main-title">好医生在线互联网医院服务协议</h2>
          <p className="update-time">最后更新时间：2024年12月</p>

          <section className="agreement-section">
            <h3 className="section-title">一、协议的接受与修改</h3>
            <p className="section-text">
              1.1 欢迎使用好医生在线互联网医院（以下简称"本平台"）提供的医疗服务。本协议是您与本平台之间关于使用本平台服务的法律协议。
            </p>
            <p className="section-text">
              1.2 在使用本平台服务前，请您仔细阅读本协议的全部内容。当您点击"同意"或开始使用本平台服务时，即表示您已充分理解并同意接受本协议的全部内容。
            </p>
            <p className="section-text">
              1.3 本平台有权根据法律法规的变化及业务发展需要，对本协议进行修改。修改后的协议将在本平台公布，自公布之日起生效。如您不同意修改后的协议，请停止使用本平台服务。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">二、服务内容</h3>
            <p className="section-text">
              2.1 本平台提供互联网医疗健康咨询服务，包括但不限于：在线问诊、图文咨询、电话咨询、健康知识分享等服务。
            </p>
            <p className="section-text">
              2.2 本平台提供的医疗服务为健康咨询和信息服务，不能替代线下医疗机构的面诊、检查、诊断和治疗。对于急重症患者，请立即前往当地医院急诊科就诊。
            </p>
            <p className="section-text">
              2.3 本平台不对任何医疗建议、诊断结果或治疗方案的准确性、完整性、及时性做出任何明示或暗示的保证。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">三、用户权利与义务</h3>
            <p className="section-text">
              3.1 您有权使用本平台提供的各项服务，但必须遵守相关法律法规和本协议的规定。
            </p>
            <p className="section-text">
              3.2 您应当提供真实、准确、完整的个人信息和病情资料，不得隐瞒或提供虚假信息。
            </p>
            <p className="section-text">
              3.3 您应当妥善保管账户信息，对账户下的所有行为负责。如发现账户被盗用，请立即通知本平台。
            </p>
            <p className="section-text">
              3.4 您不得利用本平台从事任何违法违规活动，不得干扰、破坏本平台的正常运行。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">四、服务费用</h3>
            <p className="section-text">
              4.1 本平台提供的部分服务需要付费，具体收费标准以页面显示为准。
            </p>
            <p className="section-text">
              4.2 您支付的服务费用，在服务未开始前可以申请退款；服务开始后，如因医生未接诊或平台原因导致服务无法完成，将为您全额退款。
            </p>
            <p className="section-text">
              4.3 本平台保留调整服务价格的权利，但不会对已付费的服务进行价格调整。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">五、免责声明</h3>
            <p className="section-text">
              5.1 本平台提供的医疗服务仅供参考，不能替代医疗机构的面诊。对于任何医疗决策，请咨询线下医疗机构。
            </p>
            <p className="section-text">
              5.2 因不可抗力、网络故障、系统维护等原因导致服务中断或延迟，本平台不承担责任。
            </p>
            <p className="section-text">
              5.3 因您提供的信息不真实、不完整或隐瞒重要信息导致的任何后果，由您自行承担。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">六、知识产权</h3>
            <p className="section-text">
              6.1 本平台的所有内容，包括但不限于文字、图片、音频、视频、软件、程序等，均受知识产权法保护。
            </p>
            <p className="section-text">
              6.2 未经本平台书面许可，您不得复制、传播、修改、使用本平台的任何内容。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">七、争议解决</h3>
            <p className="section-text">
              7.1 本协议的签订、履行、解释及争议解决均适用中华人民共和国法律。
            </p>
            <p className="section-text">
              7.2 如因本协议产生争议，双方应友好协商解决；协商不成的，任何一方均可向本平台所在地有管辖权的人民法院提起诉讼。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">八、其他</h3>
            <p className="section-text">
              8.1 如本协议的任何条款被认定为无效或不可执行，不影响其他条款的效力。
            </p>
            <p className="section-text">
              8.2 本平台保留对本协议的最终解释权。
            </p>
            <p className="section-text">
              8.3 如有疑问，请联系客服：400-XXX-XXXX
            </p>
          </section>
        </div>
      </div>
    </PageLayout>
  );
}

