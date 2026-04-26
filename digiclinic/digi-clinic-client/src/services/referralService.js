import API from "../api/axios";

export const createReferral = (data) => API.post("/referrals", data);
export const getMyReferrals = () => API.get("/referrals/my");
export const getDoctorReferrals = () => API.get("/referrals/doctor");
export const markReferralBooked = (id) => API.patch(`/referrals/${id}/booked`);
