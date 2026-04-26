import API from "../api/axios";

export const getPatientDashboard = () => API.get("/dashboard/patient");
export const getDoctorDashboard = () => API.get("/dashboard/doctor");
export const getAdminDashboard = () => API.get("/dashboard/admin");