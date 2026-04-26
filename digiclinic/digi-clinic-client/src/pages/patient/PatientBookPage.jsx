import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  Badge,
  Button,
  Card,
  SectionTitle,
} from "../../components/ui";
import { getDoctorById } from "../../services/doctorService";
import { getDoctorTimeSlots } from "../../services/timeSlotService";
import { createAppointment } from "../../services/appointmentService";
import {
  formatCurrency,
  formatDate,
  formatTime,
  groupByDay,
} from "../../utils/clinic";

function normalizeSlot(slot) {
  if (!slot) return null;

  return {
    id: slot.id,
    startTime: slot.startTime || slot.start || slot.dateTime || null,
    endTime: slot.endTime || slot.end || null,
    status: slot.status || "Available",
  };
}

export default function PatientBookPage() {
  const { state } = useLocation();
  const navigate = useNavigate();
  const doctorFromState = state?.doctor;
  const referral = state?.referral || null;

  const [doctor, setDoctor] = useState(null);
  const [slots, setSlots] = useState([]);
  const [selectedService, setSelectedService] = useState(null);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [doctorLoading, setDoctorLoading] = useState(true);
  const [slotsLoading, setSlotsLoading] = useState(true);
  const [booking, setBooking] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (!doctorFromState?.id) return;

    getDoctorById(doctorFromState.id)
      .then((response) => {
        const doctorData = response.data;
        setDoctor(doctorData);

        if (referral?.serviceId && Array.isArray(doctorData?.services)) {
          const matchedService = doctorData.services.find(
            (item) => Number(item.id) === Number(referral.serviceId)
          );

          if (matchedService) {
            setSelectedService(matchedService);
          } else {
            setError("Выбранный врач не оказывает услугу из направления.");
          }
        }
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить врача."
        );
      })
      .finally(() => setDoctorLoading(false));
  }, [doctorFromState?.id, referral?.serviceId]);

  useEffect(() => {
    if (!doctorFromState?.id) return;

    getDoctorTimeSlots(doctorFromState.id)
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        const now = new Date();
        const normalized = data
          .map(normalizeSlot)
          .filter(Boolean)
          .filter((slot) => {
            const slotStart = slot.startTime ? new Date(slot.startTime) : null;
            if (!slotStart || Number.isNaN(slotStart.getTime())) return false;

            return slot.status === "Available" && slotStart > now;
          });

        setSlots(normalized);
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить доступные слоты."
        );
      })
      .finally(() => setSlotsLoading(false));
  }, [doctorFromState?.id]);

  const groupedSlots = useMemo(
    () => groupByDay(slots, (item) => item.startTime),
    [slots]
  );

  const visibleServices = useMemo(() => {
    const services = Array.isArray(doctor?.services) ? doctor.services : [];
    if (!referral?.serviceId) return services;

    return services.filter((item) => Number(item.id) === Number(referral.serviceId));
  }, [doctor?.services, referral?.serviceId]);

  const handleBook = async () => {
    if (!selectedService || !selectedSlot) return;

    const slotStart = selectedSlot.startTime
      ? new Date(selectedSlot.startTime)
      : null;

    if (!slotStart || Number.isNaN(slotStart.getTime()) || slotStart <= new Date()) {
      setError("Нельзя записаться на прошедшее время. Выберите другой слот.");
      return;
    }

    setBooking(true);
    setError("");
    setMessage("");

    try {
      await createAppointment({
        timeSlotId: selectedSlot.id,
        serviceId: selectedService.id,
        referralId: referral?.id || undefined,
      });

      setMessage("Запись успешно создана.");
      navigate("/patient/appointments");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось создать запись."
      );
    } finally {
      setBooking(false);
    }
  };

  if (!doctorFromState?.id) {
    return <div className="text-[#8A94A6]">Сначала выберите врача.</div>;
  }

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Запись на приём"
        subtitle={
          referral
            ? "Выберите свободное время для записи по внутреннему направлению."
            : "Выберите услугу, свободную дату и подтвердите бронирование."
        }
      />

      {referral ? (
        <Card padding="p-5">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="text-[18px] font-bold text-[#183B2B]">
                Запись по направлению
              </div>
              <div className="mt-2 text-[15px] font-medium text-[#6F8278]">
                Услуга: {referral.serviceName}
              </div>
              {referral.comment ? (
                <div className="mt-2 text-[14px] leading-6 text-[#8A94A6]">
                  {referral.comment}
                </div>
              ) : null}
            </div>
            <Badge variant="green">Внутри клиники</Badge>
          </div>
        </Card>
      ) : null}

      <Card padding="p-6">
        {doctorLoading ? (
          <div className="text-[#8A94A6]">Загрузка данных врача...</div>
        ) : doctor ? (
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="text-[30px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                {doctor.fullName || `${doctor.firstName} ${doctor.lastName}`}
              </div>
              <div className="mt-2 text-[17px] font-medium text-[#6F8278]">
                {doctor.specialization}
              </div>
            </div>
            <Badge>Клиника «Лотос»</Badge>
          </div>
        ) : (
          <div className="text-[#D87474]">Не удалось загрузить данные врача.</div>
        )}
      </Card>

      <Card padding="p-6">
        <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
          1. Выберите услугу
        </div>

        {!visibleServices.length ? (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            У этого врача пока нет подходящих услуг.
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            {visibleServices.map((service) => {
              const isSelected = selectedService?.id === service.id;

              return (
                <button
                  key={service.id}
                  type="button"
                  onClick={() => {
                    if (!referral) {
                      setSelectedService(service);
                    }
                  }}
                  className={`rounded-[22px] border p-5 text-left transition ${
                    isSelected
                      ? "border-[#6FC17A] bg-[#EAF7EC] shadow-[0_10px_20px_rgba(111,193,122,0.14)]"
                      : "border-[#DCE9DE] bg-white hover:bg-[#F7FBF7]"
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="text-[18px] font-bold text-[#183B2B]">
                      {service.name}
                    </div>
                    {referral ? <Badge variant="green">По направлению</Badge> : null}
                  </div>

                  {service.description ? (
                    <div className="mt-2 text-[14px] leading-6 text-[#7A8F84]">
                      {service.description}
                    </div>
                  ) : null}

                  <div className="mt-4 flex items-center justify-between">
                    <div className="text-[15px] font-medium text-[#6F8278]">
                      {service.durationMinutes} мин
                    </div>
                    <div className="text-[18px] font-extrabold text-[#4F9A61]">
                      {formatCurrency(service.price)}
                    </div>
                  </div>
                </button>
              );
            })}
          </div>
        )}
      </Card>

      <Card padding="p-6">
        <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
          2. Выберите время
        </div>

        {slotsLoading ? (
          <div className="text-[#8A94A6]">Загрузка доступных слотов...</div>
        ) : Object.keys(groupedSlots).length === 0 ? (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            Для этого врача пока нет свободных слотов после текущего времени.
          </div>
        ) : (
          <div className="space-y-6">
            {Object.entries(groupedSlots).map(([dateKey, items]) => (
              <div key={dateKey}>
                <div className="mb-3 text-[20px] font-bold text-[#183B2B]">
                  {formatDate(items[0].startTime)}
                </div>

                <div className="flex flex-wrap gap-3">
                  {items.map((slot) => {
                    const isSelected = selectedSlot?.id === slot.id;

                    return (
                      <button
                        key={slot.id}
                        type="button"
                        onClick={() => setSelectedSlot(slot)}
                        className={`rounded-[18px] border px-5 py-3 text-[15px] font-semibold transition ${
                          isSelected
                            ? "border-[#6FC17A] bg-[#6FC17A] text-white shadow-[0_10px_20px_rgba(111,193,122,0.22)]"
                            : "border-[#DCE9DE] bg-white text-[#355244] hover:bg-[#F7FBF7]"
                        }`}
                      >
                        {formatTime(slot.startTime)}
                      </button>
                    );
                  })}
                </div>
              </div>
            ))}
          </div>
        )}

        {selectedSlot ? (
          <div className="mt-5 text-[16px] font-medium text-[#4F9A61]">
            Вы выбрали {formatDate(selectedSlot.startTime)} в {formatTime(selectedSlot.startTime)}.
          </div>
        ) : null}
      </Card>

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

      <div className="flex justify-end gap-3">
        <Button variant="secondary" onClick={() => navigate("/patient/doctors")}>
          Назад к списку врачей
        </Button>
        <Button
          disabled={!selectedService || !selectedSlot || booking}
          onClick={handleBook}
          className="min-w-[220px]"
        >
          {booking ? "Подтверждаем..." : "Подтвердить запись"}
        </Button>
      </div>
    </div>
  );
}
