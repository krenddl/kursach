import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { APP_BASE_URL } from "../api/axios";

export function createChatConnection(token) {
  return new HubConnectionBuilder()
    .withUrl(`${APP_BASE_URL}/hubs/chat`, {
      accessTokenFactory: () => token,
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}

export function createAppointmentConnection(token) {
  return new HubConnectionBuilder()
    .withUrl(`${APP_BASE_URL}/hubs/appointments`, {
      accessTokenFactory: () => token,
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
