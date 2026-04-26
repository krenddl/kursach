import API from "../api/axios";

export const getChatContacts = () => API.get("/chat/contacts");
