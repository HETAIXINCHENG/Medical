import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';

export default function PrivacyPolicy() {
  usePageStyles('privacy-policy.css');
  const navigate = useNavigate();

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="agreement-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/08-专家主页详情/返回.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">隐私政策</h1>
      </div>

      {/* 内容区域 */}
      <div className="agreement-content">
        <div className="content-wrapper">
          <h2 className="main-title">好医生在线互联网医院隐私政策</h2>
          <p className="update-time">最后更新时间：2024年12月</p>

          <section className="agreement-section">
            <h3 className="section-title">一、引言</h3>
            <p className="section-text">
              好医生在线互联网医院（以下简称"我们"或"本平台"）深知个人信息对您的重要性，我们将严格遵守《中华人民共和国个人信息保护法》《中华人民共和国网络安全法》等相关法律法规，采取相应的安全保护措施，保护您的个人信息安全可控。
            </p>
            <p className="section-text">
              本隐私政策将帮助您了解我们如何收集、使用、存储、共享、转让、公开披露您的个人信息，以及我们为您提供的访问、更新、删除和保护这些信息的方式。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">二、我们收集的信息</h3>
            <p className="section-text">
              2.1 为了向您提供医疗服务，我们需要收集以下信息：
            </p>
            <p className="section-text">
              （1）账户信息：手机号码、密码、昵称等注册信息；
            </p>
            <p className="section-text">
              （2）身份信息：姓名、性别、年龄、身份证号码等身份识别信息；
            </p>
            <p className="section-text">
              （3）健康信息：病情描述、病历资料、检查报告、用药记录等医疗健康信息；
            </p>
            <p className="section-text">
              （4）设备信息：设备型号、操作系统、设备标识符、IP地址等；
            </p>
            <p className="section-text">
              （5）日志信息：访问时间、访问页面、操作记录等。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">三、信息的使用</h3>
            <p className="section-text">
              3.1 我们使用收集的信息用于以下目的：
            </p>
            <p className="section-text">
              （1）提供、维护、改进我们的服务；
            </p>
            <p className="section-text">
              （2）处理您的问诊请求，匹配医生，提供医疗咨询服务；
            </p>
            <p className="section-text">
              （3）向您发送服务通知、健康提醒等信息；
            </p>
            <p className="section-text">
              （4）进行数据分析，改进服务质量；
            </p>
            <p className="section-text">
              （5）遵守法律法规要求，配合监管部门的监督检查。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">四、信息的存储</h3>
            <p className="section-text">
              4.1 我们将在中华人民共和国境内存储您的个人信息。如需跨境传输，我们将严格按照法律法规要求，获得您的明确同意，并采取必要的安全措施。
            </p>
            <p className="section-text">
              4.2 我们采用行业标准的安全技术和措施，包括数据加密、访问控制、安全审计等，保护您的个人信息安全。
            </p>
            <p className="section-text">
              4.3 我们仅在为实现本政策所述目的所必需的期间内保留您的个人信息，法律法规另有规定的除外。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">五、信息的共享与披露</h3>
            <p className="section-text">
              5.1 我们不会向第三方出售、出租或以其他方式披露您的个人信息，但以下情况除外：
            </p>
            <p className="section-text">
              （1）获得您的明确同意；
            </p>
            <p className="section-text">
              （2）法律法规规定或司法机关、行政机关依法要求提供；
            </p>
            <p className="section-text">
              （3）为提供服务需要，与我们的合作伙伴（如医生、医疗机构）共享必要信息，但我们会要求其承担保密义务；
            </p>
            <p className="section-text">
              （4）在紧急情况下，为保护用户或公众的生命、财产安全。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">六、您的权利</h3>
            <p className="section-text">
              6.1 您有权访问、更正、删除您的个人信息，或要求我们停止处理您的个人信息。
            </p>
            <p className="section-text">
              6.2 您有权撤回对个人信息处理的同意，但撤回同意不影响撤回前基于同意进行的个人信息处理活动的效力。
            </p>
            <p className="section-text">
              6.3 您有权要求我们解释个人信息处理规则，了解我们如何处理您的个人信息。
            </p>
            <p className="section-text">
              6.4 您可以通过本平台提供的功能或联系客服行使上述权利。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">七、未成年人保护</h3>
            <p className="section-text">
              7.1 我们非常重视对未成年人个人信息的保护。如果您是18周岁以下的未成年人，请在监护人的陪同下阅读本政策，并在征得监护人同意后使用我们的服务。
            </p>
            <p className="section-text">
              7.2 如果我们发现自己在未事先获得可证实的监护人同意的情况下收集了未成年人的个人信息，我们会设法尽快删除相关数据。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">八、隐私政策的更新</h3>
            <p className="section-text">
              8.1 我们可能会适时更新本隐私政策。更新后的隐私政策将在本平台公布，并在显著位置提示您阅读。
            </p>
            <p className="section-text">
              8.2 对于重大变更，我们还会通过推送通知、邮件等方式告知您。
            </p>
          </section>

          <section className="agreement-section">
            <h3 className="section-title">九、联系我们</h3>
            <p className="section-text">
              9.1 如您对本隐私政策有任何疑问、意见或建议，或需要行使相关权利，可以通过以下方式联系我们：
            </p>
            <p className="section-text">
              客服电话：400-XXX-XXXX
            </p>
            <p className="section-text">
              客服邮箱：privacy@haodoctor.com
            </p>
            <p className="section-text">
              9.2 我们将在15个工作日内回复您的请求。
            </p>
          </section>
        </div>
      </div>
    </PageLayout>
  );
}

