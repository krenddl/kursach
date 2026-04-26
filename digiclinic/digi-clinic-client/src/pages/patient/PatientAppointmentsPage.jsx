import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Badge,
  Button,
  Card,
  SectionTitle,
} from "../../components/ui";
import {
  cancelAppointment,
  getMyAppointments,
} from "../../services/appointmentService";
import {
  getMyReferrals,
  markReferralBooked,
} from "../../services/referralService";
import {
  formatDate,
  formatTime,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
  getInitials,
  isUpcomingAppointment,
} from "../../utils/clinic";

function normalizeAppointment(item) {
  if (!item) return null;

  return {
    id: item.id,
    doctor:
      item.doctor ||
      item.doctorName ||
      item.doctorFullName ||
      "Врач клиники",
    service:
      item.service ||
      item.serviceName ||
      item.specialization ||
      "Консультация",
    startTime: item.startTime || item.date || item.appointmentDate || null,
    endTime: item.endTime || null,
    status: item.status || "Scheduled",
    conclusion: item.conclusion || "",
  };
}

function normalizeReferral(item) {
  if (!item) return null;

  return {
    id: item.id,
    serviceId: item.serviceId,
    serviceName: item.serviceName || "Услуга",
    createdByDoctor: item.createdByDoctor || "Врач клиники",
    comment: item.comment || "",
    status: item.status || "Pending",
    type: item.type || "Internal",
    createdAt: item.createdAt || null,
    sourceAppointmentDate: item.sourceAppointmentDate || null,
  };
}

function AppointmentCard({ item, onCancel, cancelLoading }) {
  return (
    <div className="rounded-[26px] border border-[#E6EEE7] bg-white p-5 shadow-[0_8px_24px_rgba(111,193,122,0.04)]">
      <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
        <div className="flex min-w-0 items-center gap-4">
          <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-[22px] bg-[#EAF7EC] text-xl font-bold text-[#4F9A61]">
            {getInitials(item.doctor, "В")}
          </div>

          <div className="min-w-0">
            <div className="text-[18px] font-medium text-[#7A8F84]">
              {formatDate(item.startTime)}
            </div>
            <div className="mt-2 text-[30px] leading-tight font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {formatTime(item.startTime)}
              {item.endTime ? ` — ${formatTime(item.endTime)}` : ""}
            </div>
            <div className="mt-2 text-[18px] font-bold text-[#183B2B]">
              {item.doctor}
            </div>
            <div className="mt-1 text-[16px] font-medium text-[#7A8F84]">
              {item.service}
            </div>
          </div>
        </div>

        <Badge variant={getAppointmentStatusVariant(item.status)}>
          {getAppointmentStatusLabel(item.status, true)}
        </Badge>
      </div>

      {item.conclusion ? (
        <div className="mt-5 rounded-[20px] border border-[#E6EEE7] bg-[#F9FCF9] p-4">
          <div className="text-[14px] font-bold uppercase tracking-[0.12em] text-[#8FA095]">
            Заключение врача
          </div>
          <div className="mt-2 text-[15px] leading-7 font-medium text-[#4A6055]">
            {item.conclusion}
          </div>
        </div>
      ) : null}

      <div className="mt-5 flex flex-wrap gap-3 border-t border-[#EDF1ED] pt-5">
        {item.status === "Scheduled" ? (
          <Button
            variant="danger"
            disabled={cancelLoading}
            onClick={() => onCancel(item.id)}
          >
            {cancelLoading ? "Отменяем..." : "Отменить запись"}
          </Button>
        ) : null}
      </div>
    </div>
  );
}

function ReferralCard({ item, onBookInternal, onMarkExternal, busy }) {
  const isInternal = item.type === "Internal";

  return (
    <div className="rounded-[24px] border border-[#E6EEE7] bg-white p-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <div className="text-[18px] font-bold text-[#183B2B]">
            {item.serviceName}
          </div>
          <div className="mt-2 text-[15px] font-medium text-[#6F8278]">
            Назначил: {item.createdByDoctor}
          </div>
          {item.sourceAppointmentDate ? (
            <div className="mt-2 text-[14px] font-medium text-[#8A94A6]">
              После приёма: {formatDate(item.sourceAppointmentDate)}
            </div>
          ) : null}
          {item.comment ? (
            <div className="mt-3 text-[14px] leading-6 text-[#7A8F84]">
              {item.comment}
            </div>
          ) : null}
        </div>

        <div className="flex flex-wrap gap-2">
          <Badge variant={isInternal ? "green" : "gray"}>
            {isInternal ? "Внутреннее" : "Внешнее"}
          </Badge>
          <Badge variant={item.status === "Pending" ? "gray" : "green"}>
            {item.status === "Pending" ? "Ожидает действия" : "Запланировано"}
          </Badge>
        </div>
      </div>

      {item.status === "Pending" ? (
        <div className="mt-5 flex flex-wrap gap-3 border-t border-[#EDF1ED] pt-5">
          {isInternal ? (
            <Button onClick={() => onBookInternal(item)}>
              Подобрать врача
            </Button>
          ) : (
            <Button
              variant="secondary"
              disabled={busy}
              onClick={() => onMarkExternal(item.id)}
            >
              {busy ? "Сохраняем..." : "Отметить как внешнюю запись"}
            </Button>
          )}
        </div>
      ) : null}
    </div>
  );
}

export default function PatientAppointmentsPage() {
  const navigate = useNavigate();
  const [appointments, setAppointments] = useState([]);
  const [referrals, setReferrals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [referralsLoading, setReferralsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("upcoming");
  const [cancelLoadingId, setCancelLoadingId] = useState(null);
  const [referralBusyId, setReferralBusyId] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    getMyAppointments()
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        setAppointments(data.map(normalizeAppointment).filter(Boolean));
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить список записей."
        );
      })
      .finally(() => setLoading(false));

    getMyReferrals()
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        setReferrals(data.map(normalizeReferral).filter(Boolean));
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить направления."
        );
      })
      .finally(() => setReferralsLoading(false));
  }, []);

  const upcomingAppointments = useMemo(
    () => appointments.filter(isUpcomingAppointment),
    [appointments]
  );

  const pastAppointments = useMemo(
    () => appointments.filter((item) => !isUpcomingAppointment(item)),
    [appointments]
  );

  const visibleAppointments =
    activeTab === "upcoming" ? upcomingAppointments : pastAppointments;

  const handleCancel = async (id) => {
    try {
      setCancelLoadingId(id);
      await cancelAppointment(id);
      setAppointments((prev) =>
        prev.map((item) =>
          item.id === id ? { ...item, status: "Cancelled" } : item
        )
      );
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось отменить запись."
      );
    } finally {
      setCancelLoadingId(null);
    }
  };

  const handleMarkExternal = async (referralId) => {
    try {
      setReferralBusyId(referralId);
      await markReferralBooked(referralId);
      setReferrals((prev) =>
        prev.map((item) =>
          item.id === referralId ? { ...item, status: "Booked" } : item
        )
      );
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось обновить направление."
      );
    } finally {
      setReferralBusyId(null);
    }
  };

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Мои записи"
        subtitle="Просматривайте будущие и завершённые приёмы, а также управляйте направлениями от врача."
      />

      <Card padding="p-6">
        <div className="mb-4 flex items-center justify-between gap-3">
          <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Направления
          </div>
          <Badge variant="gray">
            {referrals.filter((item) => item.status === "Pending").length} активных
          </Badge>
        </div>

        {referralsLoading ? (
          <div className="text-[#8A94A6]">Загрузка направлений...</div>
        ) : referrals.length === 0 ? (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            У вас пока нет активных направлений.
          </div>
        ) : (
          <div className="space-y-4">
            {referrals.map((item) => (
              <ReferralCard
                key={item.id}
                item={item}
                busy={referralBusyId === item.id}
                onBookInternal={(referral) =>
                  navigate("/patient/doctors", { state: { referral } })
                }
                onMarkExternal={handleMarkExternal}
              />
            ))}
          </div>
        )}
      </Card>

      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => setActiveTab("upcoming")}
          className={`rounded-[18px] px-6 py-3 text-[16px] font-semibold transition ${
            activeTab === "upcoming"
              ? "border border-[#D5ECD9] bg-[#EAF7EC] text-[#4F9A61]"
              : "border border-[#E3ECE4] bg-white text-[#6F8278] hover:bg-[#F7FBF7]"
          }`}
        >
          Предстоящие
        </button>

        <button
          type="button"
          onClick={() => setActiveTab("past")}
          className={`rounded-[18px] px-6 py-3 text-[16px] font-semibold transition ${
            activeTab === "past"
              ? "border border-[#D5ECD9] bg-[#EAF7EC] text-[#4F9A61]"
              : "border border-[#E3ECE4] bg-white text-[#6F8278] hover:bg-[#F7FBF7]"
          }`}
        >
          История
        </button>
      </div>

      <Card padding="p-6">
        {loading ? (
          <div className="text-[#8A94A6]">Загрузка записей...</div>
        ) : visibleAppointments.length === 0 ? (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            {activeTab === "upcoming"
              ? "У вас нет предстоящих записей."
              : "История посещений пока пуста."}
          </div>
        ) : (
          <div className="space-y-5">
            {visibleAppointments.map((item) => (
              <AppointmentCard
                key={item.id}
                item={item}
                onCancel={handleCancel}
                cancelLoading={cancelLoadingId === item.id}
              />
            ))}
          </div>
        )}
      </Card>

      {error ? (
        <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
          {error}
        </div>
      ) : null}
    </div>
  );
}
