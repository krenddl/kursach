import API from "../api/axios";

export const getDoctorTimeSlots = (doctorId) =>
  API.get(`/timeslots/doctor/${doctorId}`);

export const getDoctorTimeSlotsByDate = (doctorId, date) =>
  API.get(`/timeslots/doctor/${doctorId}/by-date`, {
    params: { date },
  });

export const createTimeSlotsRange = (data) =>
  API.post("/timeslots/range", data);

export const deleteTimeSlot = (id) =>
  API.delete(`/timeslots/${id}`);