import API from "../api/axios";

export const getDoctors = (specializationId) =>
  API.get("/doctors", {
    params: specializationId ? { specializationId } : {},
  });

export const getDoctorById = (id) => API.get(`/doctors/${id}`);
export const createDoctor = (data) => API.post("/doctors", data);
export const updateDoctor = (id, data) => API.put(`/doctors/${id}`, data);
export const deleteDoctor = (id) => API.delete(`/doctors/${id}`);