import { useEffect, useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import {
  Button,
  Card,
  Input,
  SectionTitle,
} from "../../components/ui";
import { getChatContacts } from "../../services/chatService";
import { createChatConnection } from "../../services/signalrService";
import {
  formatDateTime,
  formatTime,
  getInitials,
} from "../../utils/clinic";

function normalizeContact(item) {
  if (!item) return null;

  return {
    userId: Number(item.userId),
    fullName: item.fullName || "Собеседник",
    subtitle: item.subtitle || "",
    role: item.role || "",
    lastMessageText: item.lastMessageText || "",
    lastMessageAt: item.lastMessageAt || item.lastAppointmentAt || null,
    lastAppointmentAt: item.lastAppointmentAt || null,
  };
}

function normalizeMessage(item) {
  if (!item) return null;

  return {
    id: item.id,
    senderUserId: Number(item.senderUserId),
    receiverUserId: Number(item.receiverUserId),
    senderName: item.senderName || "Отправитель",
    receiverName: item.receiverName || "Получатель",
    text: item.text || "",
    createdAt: item.createdAt || null,
    isEdited: Boolean(item.isEdited),
  };
}

function formatContactTime(value) {
  if (!value) return "";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  const today = new Date();
  const isToday =
    date.getFullYear() === today.getFullYear() &&
    date.getMonth() === today.getMonth() &&
    date.getDate() === today.getDate();

  return isToday
    ? formatTime(value)
    : date.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
      });
}

function upsertContactPreview(contacts, currentUserId, message) {
  const otherUserId =
    message.senderUserId === currentUserId
      ? message.receiverUserId
      : message.senderUserId;

  const exists = contacts.some((item) => item.userId === otherUserId);
  if (!exists) return contacts;

  return contacts
    .map((item) =>
      item.userId === otherUserId
        ? {
            ...item,
            lastMessageText: message.text,
            lastMessageAt: message.createdAt,
          }
        : item
    )
    .sort(
      (left, right) =>
        new Date(right.lastMessageAt || right.lastAppointmentAt || 0) -
        new Date(left.lastMessageAt || left.lastAppointmentAt || 0)
    );
}

export default function ChatPage() {
  const { user, token } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const [contacts, setContacts] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [messages, setMessages] = useState([]);
  const [query, setQuery] = useState("");
  const [draft, setDraft] = useState("");
  const [loading, setLoading] = useState(true);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [sending, setSending] = useState(false);
  const [connected, setConnected] = useState(false);
  const [error, setError] = useState("");

  const connectionRef = useRef(null);
  const selectedUserIdRef = useRef(null);
  const activeChatRef = useRef(null);
  const currentUserId = Number(user?.id || 0);

  useEffect(() => {
    selectedUserIdRef.current = selectedUserId;
  }, [selectedUserId]);

  useEffect(() => {
    getChatContacts()
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        const normalized = data.map(normalizeContact).filter(Boolean);
        const requestedUserId = Number(searchParams.get("user"));

        setContacts(normalized);
        setSelectedUserId(
          normalized.some((item) => item.userId === requestedUserId)
            ? requestedUserId
            : normalized[0]?.userId || null
        );
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить список диалогов."
        );
      })
      .finally(() => setLoading(false));
  }, [searchParams]);

  useEffect(() => {
    if (!token || !currentUserId) return undefined;

    const connection = createChatConnection(token);
    connectionRef.current = connection;

    connection.on("ReceivePrivateMessage", (payload) => {
      const message = normalizeMessage(payload);
      if (!message) return;

      setContacts((prev) => upsertContactPreview(prev, currentUserId, message));

      const otherUserId =
        message.senderUserId === currentUserId
          ? message.receiverUserId
          : message.senderUserId;

      if (selectedUserIdRef.current === otherUserId) {
        setMessages((prev) =>
          prev.some((item) => item.id === message.id) ? prev : [...prev, message]
        );
      }
    });

    connection.on("ChatError", (message) => {
      setError(String(message || "Произошла ошибка чата."));
    });

    connection
      .start()
      .then(() => setConnected(true))
      .catch(() => setError("Не удалось подключиться к чату."));

    return () => {
      setConnected(false);
      activeChatRef.current = null;
      connection.stop();
      connectionRef.current = null;
    };
  }, [token, currentUserId]);

  useEffect(() => {
    if (!connected || !selectedUserId || !connectionRef.current || !currentUserId) {
      return undefined;
    }

    let cancelled = false;

    const loadMessages = async () => {
      setMessagesLoading(true);
      setError("");

      try {
        if (activeChatRef.current && activeChatRef.current !== selectedUserId) {
          await connectionRef.current.invoke(
            "LeavePrivateChat",
            currentUserId,
            activeChatRef.current
          );
        }

        await connectionRef.current.invoke(
          "JoinPrivateChat",
          currentUserId,
          selectedUserId
        );

        activeChatRef.current = selectedUserId;

        const history = await connectionRef.current.invoke(
          "GetPrivateMessagesAsync",
          currentUserId,
          selectedUserId
        );

        if (!cancelled) {
          setMessages(
            Array.isArray(history) ? history.map(normalizeMessage).filter(Boolean) : []
          );
        }
      } catch {
        if (!cancelled) {
          setError("Не удалось загрузить историю переписки.");
        }
      } finally {
        if (!cancelled) {
          setMessagesLoading(false);
        }
      }
    };

    loadMessages();

    return () => {
      cancelled = true;
    };
  }, [connected, currentUserId, selectedUserId]);

  const filteredContacts = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();
    if (!normalizedQuery) return contacts;

    return contacts.filter((item) => {
      return (
        item.fullName.toLowerCase().includes(normalizedQuery) ||
        item.subtitle.toLowerCase().includes(normalizedQuery)
      );
    });
  }, [contacts, query]);

  const selectedContact = useMemo(
    () => contacts.find((item) => item.userId === selectedUserId) || null,
    [contacts, selectedUserId]
  );

  const title =
    user?.role === "Doctor" ? "Чат с пациентами" : "Чат с врачами";
  const subtitle =
    user?.role === "Doctor"
      ? "Общайтесь с пациентами в реальном времени по защищённому каналу."
      : "Задавайте вопросы врачу и получайте ответы в одном диалоге.";

  const handleSelectContact = (contactUserId) => {
    setSelectedUserId(contactUserId);
    setSearchParams({ user: String(contactUserId) });
  };

  const handleSendMessage = async () => {
    if (!draft.trim() || !selectedContact || !connectionRef.current) return;

    setSending(true);
    setError("");

    try {
      await connectionRef.current.invoke("SendPrivateMessage", {
        receiverUserId: selectedContact.userId,
        text: draft.trim(),
      });

      setDraft("");
    } catch {
      setError("Не удалось отправить сообщение.");
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="space-y-7">
      <SectionTitle title={title} subtitle={subtitle} />

      <Card padding="p-0" className="overflow-hidden">
        <div className="grid min-h-[720px] grid-cols-1 xl:grid-cols-[340px_1fr]">
          <div className="border-b border-[#E6EEE7] bg-[#FCFEFC] xl:border-r xl:border-b-0">
            <div className="border-b border-[#E6EEE7] p-6">
              <Input
                leftIcon="⌕"
                placeholder="Поиск по диалогам"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
              />
            </div>

            <div className="max-h-[620px] overflow-y-auto p-4">
              {loading ? (
                <div className="p-4 text-[#8A94A6]">Загрузка диалогов...</div>
              ) : filteredContacts.length === 0 ? (
                <div className="rounded-[22px] border border-dashed border-[#DFE6E0] bg-white p-5 text-[#8A94A6]">
                  Доступных диалогов пока нет.
                </div>
              ) : (
                <div className="space-y-3">
                  {filteredContacts.map((contact) => (
                    <button
                      key={contact.userId}
                      type="button"
                      onClick={() => handleSelectContact(contact.userId)}
                      className={`w-full rounded-[22px] border p-4 text-left transition ${
                        selectedUserId === contact.userId
                          ? "border-[#D5ECD9] bg-[#EAF7EC]"
                          : "border-[#E6EEE7] bg-white hover:bg-[#F7FBF7]"
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-[#EAF7EC] text-sm font-extrabold text-[#4F9A61]">
                          {getInitials(contact.fullName, "С")}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="flex items-center justify-between gap-3">
                            <div className="truncate text-[16px] font-bold text-[#183B2B]">
                              {contact.fullName}
                            </div>
                            <div className="text-xs font-semibold text-[#8FA095]">
                              {formatContactTime(contact.lastMessageAt || contact.lastAppointmentAt)}
                            </div>
                          </div>
                          <div className="mt-1 text-[14px] font-medium text-[#7A8F84]">
                            {contact.subtitle}
                          </div>
                          <div className="mt-2 truncate text-[14px] text-[#8FA095]">
                            {contact.lastMessageText || "Диалог ещё не начат"}
                          </div>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className="flex min-h-0 flex-col">
            {selectedContact ? (
              <>
                <div className="flex items-center justify-between gap-4 border-b border-[#E6EEE7] px-6 py-5">
                  <div className="flex min-w-0 items-center gap-4">
                    <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-[#EAF7EC] text-base font-extrabold text-[#4F9A61]">
                      {getInitials(selectedContact.fullName, "С")}
                    </div>
                    <div className="min-w-0">
                      <div className="truncate text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                        {selectedContact.fullName}
                      </div>
                      <div className="mt-1 text-[15px] font-medium text-[#7A8F84]">
                        {selectedContact.subtitle}
                      </div>
                    </div>
                  </div>

                  <div className="rounded-full border border-[#E3ECE4] bg-[#F7FBF7] px-4 py-2 text-sm font-semibold text-[#4F9A61]">
                    {connected ? "Онлайн" : "Подключение..."}
                  </div>
                </div>

                <div className="flex-1 overflow-y-auto bg-[#FBFDFB] px-6 py-6">
                  {messagesLoading ? (
                    <div className="text-[#8A94A6]">Загрузка сообщений...</div>
                  ) : messages.length === 0 ? (
                    <div className="flex h-full items-center justify-center">
                      <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-white px-6 py-5 text-center text-[#8A94A6]">
                        Начните диалог первым сообщением.
                      </div>
                    </div>
                  ) : (
                    <div className="space-y-4">
                      {messages.map((message) => {
                        const isOwn = message.senderUserId === currentUserId;

                        return (
                          <div
                            key={message.id}
                            className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                          >
                            <div
                              className={`max-w-[75%] rounded-[24px] px-5 py-4 shadow-[0_8px_20px_rgba(111,193,122,0.05)] ${
                                isOwn
                                  ? "bg-[#EAF7EC] text-[#183B2B]"
                                  : "border border-[#E6EEE7] bg-white text-[#264336]"
                              }`}
                            >
                              <div className="text-[15px] leading-7 font-medium">
                                {message.text}
                              </div>
                              <div className="mt-3 text-right text-xs font-semibold text-[#7A8F84]">
                                {formatDateTime(message.createdAt)}
                                {message.isEdited ? " · изменено" : ""}
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>

                <div className="border-t border-[#E6EEE7] bg-white px-6 py-5">
                  <div className="flex items-end gap-3">
                    <div className="flex-1">
                      <Input
                        placeholder="Введите сообщение..."
                        value={draft}
                        onChange={(event) => setDraft(event.target.value)}
                        onKeyDown={(event) => {
                          if (event.key === "Enter" && !event.shiftKey) {
                            event.preventDefault();
                            handleSendMessage();
                          }
                        }}
                      />
                    </div>
                    <Button
                      onClick={handleSendMessage}
                      disabled={!draft.trim() || sending || !connected}
                      className="min-w-[150px]"
                    >
                      {sending ? "Отправляем..." : "Отправить"}
                    </Button>
                  </div>
                </div>
              </>
            ) : (
              <div className="flex h-full items-center justify-center px-6 py-12">
                <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-white px-8 py-6 text-center text-[#8A94A6]">
                  Выберите собеседника из списка слева.
                </div>
              </div>
            )}
          </div>
        </div>
      </Card>

      {error ? (
        <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
          {error}
        </div>
      ) : null}
    </div>
  );
}
