import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import {
  Badge,
  Button,
  Card,
  Modal,
  SectionTitle,
  Textarea,
} from "../../components/ui";
import {
  addAppointmentConclusion,
  getDoctorAppointments,
  updateAppointmentStatus,
} from "../../services/appointmentService";
import { getClinicServices } from "../../services/clinicService";
import { createReferral } from "../../services/referralService";
import { createAppointmentConnection } from "../../services/signalrService";
import {
  appointmentStatusOptions,
  formatDate,
  formatTime,
  getAppointmentStatusCode,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
} from "../../utils/clinic";
import { controlClassName, textareaClassName } from "../../utils/formStyles";

const weekDays = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

function normalizeAppointment(item) {
  if (!item) return null;

  return {
    id: item.id,
    patientId: item.patientId,
    patient: item.patient || "Пациент",
    service: item.service || "Консультация",
    startTime: item.startTime,
    endTime: item.endTime,
    status: item.status || "Scheduled",
    conclusion: item.conclusion || "",
  };
}

function toDateKey(value) {
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function getCalendarDays(monthDate) {
  const year = monthDate.getFullYear();
  const month = monthDate.getMonth();
  const firstDay = new Date(year, month, 1);
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const leadingEmptyCells = (firstDay.getDay() + 6) % 7;
  const cells = Array.from({ length: leadingEmptyCells }, () => null);

  for (let day = 1; day <= daysInMonth; day += 1) {
    cells.push(new Date(year, month, day));
  }

  while (cells.length % 7 !== 0) {
    cells.push(null);
  }

  return cells;
}

function addMonths(date, value) {
  return new Date(date.getFullYear(), date.getMonth() + value, 1);
}

function sortByStartTime(left, right) {
  return new Date(left.startTime) - new Date(right.startTime);
}

function CalendarDay({ day, count, isSelected, isToday, onSelect }) {
  if (!day) return <div className="h-12" />;

  return (
    <button
      type="button"
      onClick={() => onSelect(day)}
      className={`relative flex h-12 items-center justify-center rounded-2xl text-sm font-bold transition ${
        isSelected
          ? "bg-[#6FC17A] text-white shadow-[0_10px_18px_rgba(111,193,122,0.24)]"
          : isToday
            ? "bg-[#EAF7EC] text-[#3F8B54]"
            : "bg-[#F7FBF7] text-[#5E7167] hover:bg-white hover:shadow-[0_6px_16px_rgba(111,193,122,0.08)]"
      }`}
    >
      {day.getDate()}
      {count > 0 ? (
        <span
          className={`absolute right-1.5 top-1.5 h-2 w-2 rounded-full ${
            isSelected ? "bg-white" : "bg-[#6FC17A]"
          }`}
        />
      ) : null}
    </button>
  );
}

function AppointmentRow({ appointment, onOpen }) {
  return (
    <button
      type="button"
      onClick={onOpen}
      className="group w-full rounded-[24px] border border-[#E2EDE4] bg-white p-4 text-left transition hover:border-[#BFE0C4] hover:shadow-[0_12px_28px_rgba(111,193,122,0.08)]"
    >
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-16 w-24 shrink-0 flex-col items-center justify-center rounded-[20px] bg-[#F1F8F2]">
            <div className="text-[20px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {formatTime(appointment.startTime)}
            </div>
            <div className="text-xs font-semibold text-[#7A8F84]">
              {appointment.endTime ? formatTime(appointment.endTime) : "приём"}
            </div>
          </div>
          <div>
            <div className="text-[18px] font-extrabold tracking-[-0.03em] text-[#183B2B]">
              {appointment.patient}
            </div>
            <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
              {appointment.service}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <Badge variant={getAppointmentStatusVariant(appointment.status)}>
            {getAppointmentStatusLabel(appointment.status)}
          </Badge>
          <span className="rounded-2xl border border-[#DCE9DE] bg-[#FCFEFC] px-4 py-2 text-sm font-bold text-[#4F9A61] transition group-hover:bg-[#EAF7EC]">
            Открыть
          </span>
        </div>
      </div>
    </button>
  );
}

export default function DoctorAppointmentsPage() {
  const { token } = useAuth();
  const connectionRef = useRef(null);
  const [appointments, setAppointments] = useState([]);
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [calendarMonth, setCalendarMonth] = useState(new Date());
  const [selectedAppointmentId, setSelectedAppointmentId] = useState(null);
  const [statusDrafts, setStatusDrafts] = useState({});
  const [conclusionDrafts, setConclusionDrafts] = useState({});
  const [referralDrafts, setReferralDrafts] = useState({});
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [busyKey, setBusyKey] = useState("");
  const [realtimeReady, setRealtimeReady] = useState(false);

  const loadAppointments = useCallback(async ({ silent = false } = {}) => {
    if (!silent) setLoading(true);
    setError("");

    try {
      const response = await getDoctorAppointments();
      const data = Array.isArray(response.data) ? response.data : [];
      const normalized = data.map(normalizeAppointment).filter(Boolean);
      const sorted = [...normalized].sort(sortByStartTime);

      setAppointments(sorted);
      setStatusDrafts(
        Object.fromEntries(sorted.map((item) => [item.id, item.status]))
      );
      setConclusionDrafts(
        Object.fromEntries(sorted.map((item) => [item.id, item.conclusion || ""]))
      );
      setReferralDrafts((prev) => {
        const next = { ...prev };

        sorted.forEach((item) => {
          if (!next[item.id]) {
            next[item.id] = {
              serviceId: "",
              type: "1",
              comment: "",
            };
          }
        });

        return next;
      });
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось загрузить расписание врача."
      );
    } finally {
      if (!silent) setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAppointments();

    getClinicServices()
      .then((response) => {
        setServices(Array.isArray(response.data) ? response.data : []);
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить список услуг."
        );
      });
  }, [loadAppointments]);

  useEffect(() => {
    if (!token) return undefined;

    const connection = createAppointmentConnection(token);
    connectionRef.current = connection;

    connection.on("AppointmentChanged", () => {
      loadAppointments({ silent: true });
      setMessage("Расписание обновилось в реальном времени.");
    });

    connection
      .start()
      .then(() => setRealtimeReady(true))
      .catch(() => setRealtimeReady(false));

    return () => {
      setRealtimeReady(false);
      connection.stop();
      connectionRef.current = null;
    };
  }, [loadAppointments, token]);

  const selectedDateKey = toDateKey(selectedDate);
  const todayKey = toDateKey(new Date());

  const appointmentsByDate = useMemo(() => {
    return appointments.reduce((acc, item) => {
      const key = toDateKey(item.startTime);
      acc[key] = (acc[key] || 0) + 1;
      return acc;
    }, {});
  }, [appointments]);

  const selectedDayAppointments = useMemo(() => {
    return appointments
      .filter((item) => toDateKey(item.startTime) === selectedDateKey)
      .sort(sortByStartTime);
  }, [appointments, selectedDateKey]);

  const calendarDays = useMemo(
    () => getCalendarDays(calendarMonth),
    [calendarMonth]
  );

  const selectedAppointment = useMemo(
    () =>
      appointments.find((appointment) => appointment.id === selectedAppointmentId) ||
      null,
    [appointments, selectedAppointmentId]
  );

  const referralDraft = selectedAppointment
    ? referralDrafts[selectedAppointment.id] || {
        serviceId: "",
        type: "1",
        comment: "",
      }
    : null;

  const handleSelectDay = (day) => {
    setSelectedDate(day);
    setCalendarMonth(new Date(day.getFullYear(), day.getMonth(), 1));
  };

  const handleToday = () => {
    const today = new Date();
    setSelectedDate(today);
    setCalendarMonth(new Date(today.getFullYear(), today.getMonth(), 1));
  };

  const handleSaveStatus = async () => {
    if (!selectedAppointment) return;

    const statusValue = statusDrafts[selectedAppointment.id];
    const statusCode = getAppointmentStatusCode(statusValue);

    try {
      setBusyKey(`status-${selectedAppointment.id}`);
      setMessage("");
      setError("");
      await updateAppointmentStatus(selectedAppointment.id, { status: statusCode });
      await loadAppointments({ silent: true });
      setMessage("Статус приёма сохранён.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось обновить статус."
      );
    } finally {
      setBusyKey("");
    }
  };

  const handleSaveConclusion = async () => {
    if (!selectedAppointment) return;

    try {
      setBusyKey(`conclusion-${selectedAppointment.id}`);
      setMessage("");
      setError("");
      await addAppointmentConclusion(selectedAppointment.id, {
        doctorConclusion: conclusionDrafts[selectedAppointment.id],
      });
      await loadAppointments({ silent: true });
      setMessage("Заключение сохранено.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сохранить заключение."
      );
    } finally {
      setBusyKey("");
    }
  };

  const handleCreateReferral = async () => {
    if (!selectedAppointment || !referralDraft) return;

    if (!referralDraft.serviceId) {
      setError("Выберите услугу для направления.");
      return;
    }

    try {
      setBusyKey(`referral-${selectedAppointment.id}`);
      setMessage("");
      setError("");
      await createReferral({
        sourceAppointmentId: selectedAppointment.id,
        serviceId: Number(referralDraft.serviceId),
        comment: referralDraft.comment,
        type: Number(referralDraft.type),
      });

      setReferralDrafts((prev) => ({
        ...prev,
        [selectedAppointment.id]: {
          serviceId: "",
          type: "1",
          comment: "",
        },
      }));
      setMessage("Направление создано.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось создать направление."
      );
    } finally {
      setBusyKey("");
    }
  };

  const modalStatus =
    selectedAppointment &&
    (statusDrafts[selectedAppointment.id] || selectedAppointment.status);

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Расписание"
        subtitle="Календарь приёмов врача: выбирайте день, открывайте нужный визит и обновляйте только его."
      />

      <div className="flex flex-wrap items-center gap-3">
        <Badge variant={realtimeReady ? "green" : "gray"}>
          {realtimeReady ? "Realtime включён" : "Realtime подключается"}
        </Badge>
        <Badge variant="gray">
          {formatDate(selectedDate)}
        </Badge>
      </div>

      {message ? (
        <div className="rounded-[18px] border border-[#D5ECD9] bg-[#EAF7EC] px-4 py-3 text-sm font-medium text-[#4F9A61]">
          {message}
        </div>
      ) : null}

      {error ? (
        <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
          {error}
        </div>
      ) : null}

      <Card padding="p-0" className="overflow-hidden">
        <div className="grid min-h-[680px] grid-cols-1 xl:grid-cols-[340px_1fr]">
          <div className="border-b border-[#E6EEE7] bg-[#FCFEFC] p-6 xl:border-r xl:border-b-0">
            <div className="flex items-center justify-between gap-3">
              <button
                type="button"
                onClick={() => setCalendarMonth((prev) => addMonths(prev, -1))}
                className="h-11 w-11 rounded-2xl border border-[#DCE9DE] bg-white text-lg font-bold text-[#5E7167] transition hover:bg-[#F7FBF7]"
              >
                {"<"}
              </button>
              <div className="text-center">
                <div className="text-[20px] font-extrabold tracking-[-0.03em] text-[#183B2B]">
                  {calendarMonth.toLocaleDateString("ru-RU", {
                    month: "long",
                    year: "numeric",
                  })}
                </div>
                <button
                  type="button"
                  onClick={handleToday}
                  className="mt-1 text-sm font-bold text-[#4F9A61] transition hover:text-[#3F8B54]"
                >
                  Сегодня
                </button>
              </div>
              <button
                type="button"
                onClick={() => setCalendarMonth((prev) => addMonths(prev, 1))}
                className="h-11 w-11 rounded-2xl border border-[#DCE9DE] bg-white text-lg font-bold text-[#5E7167] transition hover:bg-[#F7FBF7]"
              >
                {">"}
              </button>
            </div>

            <div className="mt-6 grid grid-cols-7 gap-2 text-center text-xs font-extrabold uppercase tracking-[0.08em] text-[#8FA095]">
              {weekDays.map((day) => (
                <div key={day}>{day}</div>
              ))}
            </div>

            <div className="mt-3 grid grid-cols-7 gap-2">
              {calendarDays.map((day, index) => {
                const key = day ? toDateKey(day) : `empty-${index}`;

                return (
                  <CalendarDay
                    key={key}
                    day={day}
                    count={day ? appointmentsByDate[toDateKey(day)] || 0 : 0}
                    isSelected={day ? toDateKey(day) === selectedDateKey : false}
                    isToday={day ? toDateKey(day) === todayKey : false}
                    onSelect={handleSelectDay}
                  />
                );
              })}
            </div>

            <div className="mt-6 rounded-[22px] border border-[#E6EEE7] bg-white p-4 text-sm leading-6 text-[#6F8278]">
              Зелёная точка показывает, что в этот день есть приёмы.
            </div>
          </div>

          <div className="p-6">
            <div className="flex flex-col gap-3 border-b border-[#EDF3EE] pb-5 lg:flex-row lg:items-center lg:justify-between">
              <div>
                <div className="text-[28px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                  {formatDate(selectedDate)}
                </div>
                <div className="mt-1 text-[15px] font-medium text-[#7A8F84]">
                  {selectedDayAppointments.length > 0
                    ? `Записей на день: ${selectedDayAppointments.length}`
                    : "На выбранный день записей нет"}
                </div>
              </div>

              <Button variant="secondary" onClick={loadAppointments}>
                Обновить
              </Button>
            </div>

            <div className="mt-6">
              {loading ? (
                <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                  Загружаем расписание...
                </div>
              ) : selectedDayAppointments.length === 0 ? (
                <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-8 text-center text-[#8A94A6]">
                  Приёмов нет. Выберите дату с зелёной точкой в календаре.
                </div>
              ) : (
                <div className="space-y-4">
                  {selectedDayAppointments.map((appointment) => (
                    <AppointmentRow
                      key={appointment.id}
                      appointment={appointment}
                      onOpen={() => setSelectedAppointmentId(appointment.id)}
                    />
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </Card>

      <Modal
        open={Boolean(selectedAppointment)}
        title="Приём пациента"
        onClose={() => setSelectedAppointmentId(null)}
        className="max-w-[820px]"
      >
        {selectedAppointment ? (
          <div className="space-y-5">
            <div className="rounded-[24px] border border-[#E6EEE7] bg-[#FAFCFB] p-5">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div>
                  <div className="text-[15px] font-bold text-[#7A8F84]">
                    {formatDate(selectedAppointment.startTime)}
                  </div>
                  <div className="mt-2 text-[30px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                    {formatTime(selectedAppointment.startTime)}
                    {selectedAppointment.endTime
                      ? ` - ${formatTime(selectedAppointment.endTime)}`
                      : ""}
                  </div>
                  <div className="mt-3 text-[20px] font-extrabold text-[#183B2B]">
                    {selectedAppointment.patient}
                  </div>
                  <div className="mt-1 text-[16px] font-medium text-[#6F8278]">
                    {selectedAppointment.service}
                  </div>
                </div>

                <Badge variant={getAppointmentStatusVariant(selectedAppointment.status)}>
                  {getAppointmentStatusLabel(selectedAppointment.status)}
                </Badge>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-3 lg:grid-cols-[1fr_190px]">
              <select
                value={modalStatus || "Scheduled"}
                onChange={(event) =>
                  setStatusDrafts((prev) => ({
                    ...prev,
                    [selectedAppointment.id]: event.target.value,
                  }))
                }
                className={controlClassName}
              >
                {appointmentStatusOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              <Button
                onClick={handleSaveStatus}
                disabled={busyKey === `status-${selectedAppointment.id}`}
              >
                {busyKey === `status-${selectedAppointment.id}`
                  ? "Сохраняем..."
                  : "Сохранить"}
              </Button>
            </div>

            {modalStatus === "Completed" ? (
              <div className="rounded-[24px] border border-[#E6EEE7] bg-white p-5">
                <div className="text-[17px] font-extrabold text-[#183B2B]">
                  Заключение
                </div>
                <Textarea
                  value={conclusionDrafts[selectedAppointment.id] || ""}
                  onChange={(event) =>
                    setConclusionDrafts((prev) => ({
                      ...prev,
                      [selectedAppointment.id]: event.target.value,
                    }))
                  }
                  className={`${textareaClassName} mt-3`}
                  placeholder="Кратко опишите результат приёма и рекомендации."
                />
                <div className="mt-3 flex justify-end">
                  <Button
                    variant="secondary"
                    onClick={handleSaveConclusion}
                    disabled={busyKey === `conclusion-${selectedAppointment.id}`}
                  >
                    {busyKey === `conclusion-${selectedAppointment.id}`
                      ? "Сохраняем..."
                      : "Сохранить заключение"}
                  </Button>
                </div>
              </div>
            ) : null}

            {modalStatus === "Completed" && referralDraft ? (
              <div className="rounded-[24px] border border-[#E6EEE7] bg-[#F9FCF9] p-5">
                <div className="text-[17px] font-extrabold text-[#183B2B]">
                  Направление
                </div>
                <div className="mt-1 text-sm leading-6 text-[#6F8278]">
                  Можно назначить услугу внутри клиники или оформить внешнюю рекомендацию.
                </div>

                <div className="mt-4 grid grid-cols-1 gap-3 lg:grid-cols-[1fr_190px]">
                  <select
                    value={referralDraft.serviceId}
                    onChange={(event) =>
                      setReferralDrafts((prev) => ({
                        ...prev,
                        [selectedAppointment.id]: {
                          ...referralDraft,
                          serviceId: event.target.value,
                        },
                      }))
                    }
                    className={controlClassName}
                  >
                    <option value="">Выберите услугу</option>
                    {services.map((service) => (
                      <option key={service.id} value={service.id}>
                        {service.name}
                      </option>
                    ))}
                  </select>

                  <select
                    value={referralDraft.type}
                    onChange={(event) =>
                      setReferralDrafts((prev) => ({
                        ...prev,
                        [selectedAppointment.id]: {
                          ...referralDraft,
                          type: event.target.value,
                        },
                      }))
                    }
                    className={controlClassName}
                  >
                    <option value="1">Внутреннее</option>
                    <option value="2">Внешнее</option>
                  </select>
                </div>

                <Textarea
                  value={referralDraft.comment}
                  onChange={(event) =>
                    setReferralDrafts((prev) => ({
                      ...prev,
                      [selectedAppointment.id]: {
                        ...referralDraft,
                        comment: event.target.value,
                      },
                    }))
                  }
                  className={`${textareaClassName} mt-3`}
                  placeholder="Комментарий к направлению."
                />

                <div className="mt-3 flex justify-end">
                  <Button
                    onClick={handleCreateReferral}
                    disabled={busyKey === `referral-${selectedAppointment.id}`}
                  >
                    {busyKey === `referral-${selectedAppointment.id}`
                      ? "Создаём..."
                      : "Создать направление"}
                  </Button>
                </div>
              </div>
            ) : null}
          </div>
        ) : null}
      </Modal>
    </div>
  );
}
