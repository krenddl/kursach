import API from "../api/axios";

export const createTelegramLinkCode = () => API.post("/telegram/link-code");
export const unlinkTelegram = () => API.delete("/telegram/link");
