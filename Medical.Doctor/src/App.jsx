import { Routes, Route, Navigate } from 'react-router-dom';
import Clinic from './pages/Clinic.jsx';
import PatientManage from './pages/PatientManage.jsx';
import DoctorProfile from './pages/DoctorProfile.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/clinic" replace />} />
      <Route path="/clinic" element={<Clinic />} />
      <Route path="/patients" element={<PatientManage />} />
      <Route path="/profile" element={<DoctorProfile />} />
    </Routes>
  );
}

