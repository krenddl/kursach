import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

function getRoleDashboard(role) {
  if (role === "Admin") return "/admin/dashboard";
  if (role === "Doctor") return "/doctor/dashboard";
  return "/patient/dashboard";
}

export default function ProtectedRoute({ roles }) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && user && !roles.includes(user.role)) {
    return <Navigate to={getRoleDashboard(user.role)} replace />;
  }

  return <Outlet />;
}
