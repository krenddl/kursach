import API from "../api/axios";

export const getMyProfile = () => API.get("/profile/me");
export const updateMyProfile = (data) => API.put("/profile/me", data);