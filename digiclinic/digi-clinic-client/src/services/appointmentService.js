import API from "../api/axios";

export const getMyAppointments = () => API.get("/appointments/my");
export const getDoctorAppointments = () => API.get("/appointments/doctor");
export const getDoctorAppointmentsFiltered = (from, to) =>
  API.get("/appointments/doctor/filter", {
    params: { from, to },
  });

export const getAllAppointments = () => API.get("/appointments/all");

export const createAppointment = (data) => API.post("/appointments", data);
export const cancelAppointment = (id) =>
  API.patch(`/appointments/${id}/cancel`);

export const updateAppointmentStatus = (id, data) =>
  API.patch(`/appointments/${id}/status`, data);

export const addAppointmentConclusion = (id, data) =>
  API.patch(`/appointments/${id}/conclusion`, data);