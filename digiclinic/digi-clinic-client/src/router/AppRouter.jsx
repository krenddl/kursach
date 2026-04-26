import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";
import AppLayout from "../components/layout/AppLayout";
import { useAuth } from "../auth/AuthContext";

import LoginPage from "../pages/auth/LoginPage";
import RegisterPage from "../pages/auth/RegisterPage";

import PatientDashboardPage from "../pages/patient/PatientDashboardPage";
import PatientDoctorsPage from "../pages/patient/PatientDoctorsPage";
import PatientAppointmentsPage from "../pages/patient/PatientAppointmentsPage";
import PatientBookPage from "../pages/patient/PatientBookPage";

import DoctorDashboardPage from "../pages/doctor/DoctorDashboardPage";
import DoctorAppointmentsPage from "../pages/doctor/DoctorAppointmentsPage";
import DoctorPatientsPage from "../pages/doctor/DoctorPatientsPage";

import AdminDashboardPage from "../pages/admin/AdminDashboardPage";
import AdminDoctorsPage from "../pages/admin/AdminDoctorsPage";
import AdminPatientsPage from "../pages/admin/AdminPatientsPage";
import AdminAppointmentsPage from "../pages/admin/AdminAppointmentsPage";
import AdminSchedulePage from "../pages/admin/AdminSchedulePage";
import ChatPage from "../pages/common/ChatPage";
import ProfilePage from "../pages/common/ProfilePage";

function HomeRedirect() {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (user?.role === "Admin") {
    return <Navigate to="/admin/dashboard" replace />;
  }

  if (user?.role === "Doctor") {
    return <Navigate to="/doctor/dashboard" replace />;
  }

  return <Navigate to="/patient/dashboard" replace />;
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomeRedirect />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route element={<ProtectedRoute roles={["Patient", "Doctor", "Admin"]} />}>
          <Route element={<AppLayout />}>
            <Route element={<ProtectedRoute roles={["Patient"]} />}>
              <Route path="/patient/dashboard" element={<PatientDashboardPage />} />
              <Route path="/patient/doctors" element={<PatientDoctorsPage />} />
              <Route path="/patient/appointments" element={<PatientAppointmentsPage />} />
              <Route path="/patient/book" element={<PatientBookPage />} />
              <Route path="/patient/chat" element={<ChatPage />} />
              <Route path="/patient/profile" element={<ProfilePage />} />
            </Route>

            <Route element={<ProtectedRoute roles={["Doctor"]} />}>
              <Route path="/doctor/dashboard" element={<DoctorDashboardPage />} />
              <Route path="/doctor/appointments" element={<DoctorAppointmentsPage />} />
              <Route path="/doctor/patients" element={<DoctorPatientsPage />} />
              <Route path="/doctor/chat" element={<ChatPage />} />
              <Route path="/doctor/profile" element={<ProfilePage />} />
            </Route>

            <Route element={<ProtectedRoute roles={["Admin"]} />}>
              <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
              <Route path="/admin/doctors" element={<AdminDoctorsPage />} />
              <Route path="/admin/patients" element={<AdminPatientsPage />} />
              <Route path="/admin/appointments" element={<AdminAppointmentsPage />} />
              <Route path="/admin/schedule" element={<AdminSchedulePage />} />
              <Route path="/admin/profile" element={<ProfilePage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
