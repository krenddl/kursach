import API from "../api/axios";

export const getSpecializations = () => API.get("/specializations");
export const createSpecialization = (data) => API.post("/specializations", data);
export const updateSpecialization = (id, data) =>
  API.put(`/specializations/${id}`, data);
export const deleteSpecialization = (id) =>
  API.delete(`/specializations/${id}`);
