import { Routes, Route } from 'react-router-dom';
import Home from './pages/Home.jsx';
import Discover from './pages/Discover.jsx';
import Information from './pages/Information.jsx';
import Profile from './pages/Profile.jsx';
import Doctors from './pages/Doctors.jsx';
import DoctorDetail from './pages/DoctorDetail.jsx';
import Department from './pages/Department.jsx';
import Consultation from './pages/Consultation.jsx';
import ServiceDescription from './pages/ServiceDescription.jsx';
import WarmReminder from './pages/WarmReminder.jsx';
import PreConsultation from './pages/PreConsultation.jsx';
import MedicalRecordList from './pages/MedicalRecordList.jsx';
import MedicalRecordDetail from './pages/MedicalRecordDetail.jsx';
import Login from './pages/Login.jsx';
import ServiceAgreement from './pages/ServiceAgreement.jsx';
import PrivacyPolicy from './pages/PrivacyPolicy.jsx';
import LoginHelp from './pages/LoginHelp.jsx';
import VerificationCodeHelp from './pages/VerificationCodeHelp.jsx';
import ForgotPassword from './pages/ForgotPassword.jsx';
import ResetPassword from './pages/ResetPassword.jsx';
import MyOrders from './pages/MyOrders.jsx';
import OrderDetail from './pages/OrderDetail.jsx';
import MyPrescriptions from './pages/MyPrescriptions.jsx';
import PrescriptionDetail from './pages/PrescriptionDetail.jsx';
import MyReviews from './pages/MyReviews.jsx';
import MyDoctors from './pages/MyDoctors.jsx';
import Settings from './pages/Settings.jsx';
import AddPatient from './pages/AddPatient.jsx';
import ConsultationForm from './pages/ConsultationForm.jsx';
import SubscriptionManage from './pages/SubscriptionManage.jsx';
import PatientSupportGroupDetail from './pages/PatientSupportGroupDetail.jsx';
import GroupRules from './pages/GroupRules.jsx';
import CreatePost from './pages/CreatePost.jsx';
import AccountSecurity from './pages/AccountSecurity.jsx';
import VerifyPhone from './pages/VerifyPhone.jsx';
import ModifyPassword from './pages/ModifyPassword.jsx';
import CancelAccount from './pages/CancelAccount.jsx';
import CancelAccountReason from './pages/CancelAccountReason.jsx';
import CancelAccountConfirm from './pages/CancelAccountConfirm.jsx';
import CancelAccountNotice from './pages/CancelAccountNotice.jsx';
import NotificationSettings from './pages/NotificationSettings.jsx';
import SystemNotificationSettings from './pages/SystemNotificationSettings.jsx';
import SystemInfo from './pages/SystemInfo.jsx';
import Feedback from './pages/Feedback.jsx';
import HospitalRanking from './pages/HospitalRanking.jsx';
import HospitalSearch from './pages/HospitalSearch.jsx';
import TcmSpecialtyRanking from './pages/TcmSpecialtyRanking.jsx';
import HealthAsk from './pages/HealthAsk.jsx';
import Mall from './pages/Mall.jsx';
import HealthKnowledgeDetail from './pages/HealthKnowledgeDetail.jsx';
import PostDetail from './pages/PostDetail.jsx';
import MerchantServices from './pages/MerchantServices.jsx';
import MerchantOrders from './pages/MerchantOrders.jsx';
import MerchantChat from './pages/MerchantChat.jsx';
import CustomerService from './pages/CustomerService.jsx';

function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/mall" element={<Mall />} />
      <Route path="/discover" element={<Discover />} />
      <Route path="/health-knowledge/:id" element={<HealthKnowledgeDetail />} />
      <Route path="/health-ask" element={<HealthAsk />} />
      <Route path="/information" element={<Information />} />
      <Route path="/profile" element={<Profile />} />
      <Route path="/doctors" element={<Doctors />} />
      <Route path="/doctor/:doctorId" element={<DoctorDetail />} />
      <Route path="/department" element={<Department />} />
      <Route path="/consultation" element={<Consultation />} />
      <Route path="/service-description" element={<ServiceDescription />} />
      <Route path="/warm-reminder" element={<WarmReminder />} />
      <Route path="/pre-consultation" element={<PreConsultation />} />
      <Route path="/medical-record-list" element={<MedicalRecordList />} />
      <Route path="/medical-record-detail" element={<MedicalRecordDetail />} />
      <Route path="/login" element={<Login />} />
      <Route path="/service-agreement" element={<ServiceAgreement />} />
      <Route path="/privacy-policy" element={<PrivacyPolicy />} />
      <Route path="/login-help" element={<LoginHelp />} />
      <Route path="/verification-code-help" element={<VerificationCodeHelp />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/reset-password" element={<ResetPassword />} />
      <Route path="/my-orders" element={<MyOrders />} />
      <Route path="/order-detail/:id" element={<OrderDetail />} />
      <Route path="/my-prescriptions" element={<MyPrescriptions />} />
      <Route path="/prescription-detail/:id" element={<PrescriptionDetail />} />
      <Route path="/my-reviews" element={<MyReviews />} />
      <Route path="/my-doctors" element={<MyDoctors />} />
      <Route path="/settings" element={<Settings />} />
      <Route path="/account-security" element={<AccountSecurity />} />
      <Route path="/verify-phone" element={<VerifyPhone />} />
      <Route path="/modify-password" element={<ModifyPassword />} />
      <Route path="/cancel-account" element={<CancelAccount />} />
      <Route path="/cancel-account-reason" element={<CancelAccountReason />} />
      <Route path="/cancel-account-confirm" element={<CancelAccountConfirm />} />
      <Route path="/cancel-account-notice" element={<CancelAccountNotice />} />
      <Route path="/notification-settings" element={<NotificationSettings />} />
      <Route path="/system-notification-settings" element={<SystemNotificationSettings />} />
      <Route path="/system-info" element={<SystemInfo />} />
      <Route path="/feedback" element={<Feedback />} />
      <Route path="/add-patient" element={<AddPatient />} />
      <Route path="/consultation-form" element={<ConsultationForm />} />
      <Route path="/subscription-manage" element={<SubscriptionManage />} />
      <Route path="/patient-support-group/:doctorId" element={<PatientSupportGroupDetail />} />
      <Route path="/group-rules/:groupId" element={<GroupRules />} />
      <Route path="/create-post/:groupId" element={<CreatePost />} />
      <Route path="/post-detail/:postId" element={<PostDetail />} />
      <Route path="/hospital-ranking" element={<HospitalRanking />} />
      <Route path="/hospital-search" element={<HospitalSearch />} />
      <Route path="/tcm-specialty-ranking" element={<TcmSpecialtyRanking />} />
      <Route path="/merchant-services" element={<MerchantServices />} />
      <Route path="/merchant-orders" element={<MerchantOrders />} />
      <Route path="/merchant-chat" element={<MerchantChat />} />
      <Route path="/customer-service" element={<CustomerService />} />
    </Routes>
  );
}

export default App;
