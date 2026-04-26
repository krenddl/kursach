import API from "../api/axios";

export const getPatients = () => API.get("/patients");
export const getPatientById = (id) => API.get(`/patients/${id}`);
export const getPatientHistory = (id) => API.get(`/patients/${id}/history`);
export const updatePatient = (id, data) => API.put(`/patients/${id}`, data);
