import API from "../api/axios";

export const getClinicServices = () => API.get("/services");
export const createClinicService = (data) => API.post("/services", data);
export const updateClinicService = (id, data) =>
  API.put(`/services/${id}`, data);
export const deleteClinicService = (id) => API.delete(`/services/${id}`);
