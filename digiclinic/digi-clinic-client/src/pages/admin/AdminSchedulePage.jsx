import { useEffect, useMemo, useState } from "react";
import { Badge, Button, Card, SectionTitle } from "../../components/ui";
import { getDoctors } from "../../services/doctorService";
import {
  createTimeSlotsRange,
  deleteTimeSlot,
  getDoctorTimeSlotsByDate,
} from "../../services/timeSlotService";
import { formatDate, formatTime } from "../../utils/clinic";
import {
  controlClassName,
  helperCardClassName,
  labelClassName,
} from "../../utils/formStyles";

function normalizeDoctor(item) {
  if (!item) return null;

  return {
    id: item.id,
    fullName:
      item.fullName ||
      `${item.firstName || ""} ${item.lastName || ""}`.trim() ||
      "Врач клиники",
    specialization:
      item.specializationName ||
      item.specialization ||
      "Специалист",
    isActive: item.isActive ?? true,
  };
}

function normalizeSlot(item) {
  if (!item) return null;

  return {
    id: item.id,
    startTime: item.startTime || item.start || null,
    endTime: item.endTime || item.end || null,
    status: item.status || "Available",
  };
}

function getSlotBadge(status) {
  if (status === "Booked") {
    return { label: "Занят", variant: "yellow" };
  }

  if (status === "Cancelled") {
    return { label: "Отменён", variant: "red" };
  }

  return { label: "Свободен", variant: "green" };
}

export default function AdminSchedulePage() {
  const [doctors, setDoctors] = useState([]);
  const [selectedDoctorId, setSelectedDoctorId] = useState("");
  const [selectedDate, setSelectedDate] = useState(
    new Date().toISOString().slice(0, 10)
  );
  const [form, setForm] = useState({
    startDate: new Date().toISOString().slice(0, 10),
    endDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30)
      .toISOString()
      .slice(0, 10),
    workStart: "09:00:00",
    workEnd: "18:00:00",
    durationMinutes: 30,
    breakStart: "13:00:00",
    breakEnd: "14:00:00",
  });
  const [slots, setSlots] = useState([]);
  const [loading, setLoading] = useState(true);
  const [slotsLoading, setSlotsLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    getDoctors()
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        const normalized = data
          .map(normalizeDoctor)
          .filter(Boolean)
          .filter((doctor) => doctor.isActive);

        setDoctors(normalized);
        if (normalized.length > 0) {
          setSelectedDoctorId(String(normalized[0].id));
        }
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить список врачей."
        );
      })
      .finally(() => setLoading(false));
  }, []);

  const loadSlots = async () => {
    if (!selectedDoctorId || !selectedDate) return;

    setSlotsLoading(true);

    try {
      const response = await getDoctorTimeSlotsByDate(selectedDoctorId, selectedDate);
      const data = Array.isArray(response.data) ? response.data : [];
      setSlots(data.map(normalizeSlot).filter(Boolean));
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось загрузить слоты врача."
      );
    } finally {
      setSlotsLoading(false);
    }
  };

  useEffect(() => {
    if (selectedDoctorId && selectedDate) {
      loadSlots();
    }
  }, [selectedDoctorId, selectedDate]);

  const selectedDoctor = useMemo(() => {
    return doctors.find((doctor) => String(doctor.id) === String(selectedDoctorId));
  }, [doctors, selectedDoctorId]);

  const handleCreateRange = async () => {
    if (!selectedDoctorId) {
      setError("Сначала выберите врача.");
      return;
    }

    setSubmitting(true);
    setMessage("");
    setError("");

    try {
      await createTimeSlotsRange({
        doctorProfileId: Number(selectedDoctorId),
        startDate: `${form.startDate}T00:00:00Z`,
        endDate: `${form.endDate}T00:00:00Z`,
        workStart: form.workStart,
        workEnd: form.workEnd,
        durationMinutes: Number(form.durationMinutes),
        breakStart: form.breakStart,
        breakEnd: form.breakEnd,
      });

      setMessage("Слоты успешно сгенерированы.");
      await loadSlots();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сгенерировать слоты."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteSlot = async (slotId) => {
    const confirmed = window.confirm("Удалить выбранный слот?");
    if (!confirmed) return;

    try {
      await deleteTimeSlot(slotId);
      setSlots((prev) => prev.filter((item) => item.id !== slotId));
      setMessage("Слот удалён.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось удалить слот."
      );
    }
  };

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Расписание врачей"
        subtitle="Создавайте диапазоны слотов, проверяйте занятость по конкретной дате и поддерживайте расписание в актуальном состоянии."
      />

      <div className="grid grid-cols-1 gap-6 2xl:grid-cols-[1.05fr_1.35fr]">
        <Card padding="p-6">
          <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Генерация слотов
          </div>

          {loading ? (
            <div className="text-[#8A94A6]">Загрузка врачей...</div>
          ) : (
            <div className="space-y-5">
              <div>
                <label className={labelClassName}>Врач</label>
                <select
                  value={selectedDoctorId}
                  onChange={(event) => setSelectedDoctorId(event.target.value)}
                  className={controlClassName}
                >
                  {doctors.map((doctor) => (
                    <option key={doctor.id} value={doctor.id}>
                      {doctor.fullName} — {doctor.specialization}
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div>
                  <label className={labelClassName}>Дата начала</label>
                  <input
                    type="date"
                    value={form.startDate}
                    onChange={(event) =>
                      setForm((prev) => ({ ...prev, startDate: event.target.value }))
                    }
                    className={controlClassName}
                  />
                </div>
                <div>
                  <label className={labelClassName}>Дата окончания</label>
                  <input
                    type="date"
                    value={form.endDate}
                    onChange={(event) =>
                      setForm((prev) => ({ ...prev, endDate: event.target.value }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div>
                  <label className={labelClassName}>Начало рабочего дня</label>
                  <input
                    type="time"
                    value={form.workStart.slice(0, 5)}
                    onChange={(event) =>
                      setForm((prev) => ({
                        ...prev,
                        workStart: `${event.target.value}:00`,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
                <div>
                  <label className={labelClassName}>Конец рабочего дня</label>
                  <input
                    type="time"
                    value={form.workEnd.slice(0, 5)}
                    onChange={(event) =>
                      setForm((prev) => ({
                        ...prev,
                        workEnd: `${event.target.value}:00`,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-[220px_1fr_1fr]">
                <div>
                  <label className={labelClassName}>Длительность слота</label>
                  <input
                    type="number"
                    min="5"
                    value={form.durationMinutes}
                    onChange={(event) =>
                      setForm((prev) => ({
                        ...prev,
                        durationMinutes: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
                <div>
                  <label className={labelClassName}>Перерыв с</label>
                  <input
                    type="time"
                    value={form.breakStart.slice(0, 5)}
                    onChange={(event) =>
                      setForm((prev) => ({
                        ...prev,
                        breakStart: `${event.target.value}:00`,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
                <div>
                  <label className={labelClassName}>Перерыв до</label>
                  <input
                    type="time"
                    value={form.breakEnd.slice(0, 5)}
                    onChange={(event) =>
                      setForm((prev) => ({
                        ...prev,
                        breakEnd: `${event.target.value}:00`,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className={helperCardClassName}>
                Слоты создаются только в свободных промежутках. Пересечения не дублируются, а интервал обеда автоматически исключается из расписания.
              </div>

              <Button onClick={handleCreateRange} disabled={submitting} className="w-full">
                {submitting ? "Генерируем..." : "Сгенерировать слоты"}
              </Button>
            </div>
          )}
        </Card>

        <Card padding="p-6">
          <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
            <div>
              <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                Слоты на выбранную дату
              </div>
              <div className="mt-2 text-[16px] font-medium text-[#7A8F84]">
                {selectedDoctor
                  ? `${selectedDoctor.fullName} — ${selectedDoctor.specialization}`
                  : "Выберите врача"}
              </div>
            </div>

            <div className="w-full xl:max-w-[240px]">
              <label className={labelClassName}>Дата просмотра</label>
              <input
                type="date"
                value={selectedDate}
                onChange={(event) => setSelectedDate(event.target.value)}
                className={controlClassName}
              />
            </div>
          </div>

          <div className="mt-6 space-y-4">
            {slotsLoading ? (
              <div className="text-[#8A94A6]">Загрузка слотов...</div>
            ) : slots.length === 0 ? (
              <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                На выбранную дату слотов нет.
              </div>
            ) : (
              slots.map((slot) => {
                const badge = getSlotBadge(slot.status);

                return (
                  <div
                    key={slot.id}
                    className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                  >
                    <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
                      <div>
                        <div className="text-[22px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                          {formatTime(slot.startTime)} — {formatTime(slot.endTime)}
                        </div>
                        <div className="mt-2 text-[15px] font-medium text-[#7A8F84]">
                          {formatDate(slot.startTime)}
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-3">
                        <Badge variant={badge.variant}>{badge.label}</Badge>
                        {slot.status === "Available" ? (
                          <Button
                            variant="danger"
                            onClick={() => handleDeleteSlot(slot.id)}
                          >
                            Удалить
                          </Button>
                        ) : null}
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </Card>
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
    </div>
  );
}
