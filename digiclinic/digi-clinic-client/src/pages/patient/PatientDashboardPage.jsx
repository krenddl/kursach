import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Badge,
  Button,
  Card,
  SectionTitle,
  StatCard,
} from "../../components/ui";
import { getPatientDashboard } from "../../services/dashboardService";
import {
  formatDate,
  formatDateTime,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
  getInitials,
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
    status: item.status || "Scheduled",
  };
}

function ActionCard({ title, description, onClick, actionLabel }) {
  return (
    <button
      onClick={onClick}
      className="w-full rounded-[24px] border border-[#E6EEE7] bg-white p-5 text-left transition hover:shadow-[0_12px_28px_rgba(111,193,122,0.09)]"
    >
      <div className="text-[18px] font-bold text-[#183B2B]">{title}</div>
      <div className="mt-2 text-[15px] leading-7 font-medium text-[#6F8278]">
        {description}
      </div>
      <div className="mt-5 text-[15px] font-bold text-[#4F9A61]">
        {actionLabel}
      </div>
    </button>
  );
}

export default function PatientDashboardPage() {
  const navigate = useNavigate();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getPatientDashboard()
      .then((response) => setData(response.data))
      .catch((error) => {
        console.error("Ошибка загрузки дашборда пациента:", error);
      })
      .finally(() => setLoading(false));
  }, []);

  const nextAppointment = useMemo(() => {
    return normalizeAppointment(
      data?.nextAppointment ||
        data?.upcomingAppointment ||
        data?.nearestAppointment ||
        null
    );
  }, [data]);

  const history = useMemo(() => {
    const items = Array.isArray(data?.history) ? data.history : [];
    return items.map(normalizeAppointment).filter(Boolean).slice(0, 5);
  }, [data]);

  if (loading) {
    return <div className="text-[#8A94A6]">Загрузка личного кабинета...</div>;
  }

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Личный кабинет"
        subtitle="Следите за ближайшей записью, историей приёмов и быстрым доступом к ключевым действиям."
      />

      <div className="grid grid-cols-1 gap-5 md:grid-cols-3">
        <StatCard title="Всего записей" value={data?.total ?? 0} />
        <StatCard
          title="Предстоящие"
          value={data?.upcomingCount ?? 0}
          accent="gray"
        />
        <StatCard
          title="Завершённые"
          value={data?.completed ?? 0}
          subtitle={`Отменено: ${data?.cancelled ?? 0}`}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1.35fr_0.95fr]">
        <Card padding="p-6">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Ближайший приём
            </div>
            <Badge variant="green">Пациент</Badge>
          </div>

          {nextAppointment ? (
            <div className="rounded-[26px] border border-[#E6EEE7] bg-[#FCFEFC] p-5">
              <div className="flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
                <div className="flex min-w-0 items-center gap-4">
                  <div className="flex h-[72px] w-[72px] shrink-0 items-center justify-center rounded-[22px] bg-[#EAF7EC] text-[22px] font-extrabold text-[#4F9A61]">
                    {getInitials(nextAppointment.doctor, "В")}
                  </div>
                  <div className="min-w-0">
                    <div className="truncate text-[28px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                      {nextAppointment.doctor}
                    </div>
                    <div className="mt-1 text-[17px] font-medium text-[#6F8278]">
                      {nextAppointment.service}
                    </div>
                    <div className="mt-3 text-[16px] font-semibold text-[#355244]">
                      {formatDateTime(nextAppointment.startTime)}
                    </div>
                  </div>
                </div>

                <Badge variant={getAppointmentStatusVariant(nextAppointment.status)}>
                  {getAppointmentStatusLabel(nextAppointment.status, true)}
                </Badge>
              </div>

              <div className="mt-5 flex flex-wrap gap-3 border-t border-[#EDF1ED] pt-5">
                <Button onClick={() => navigate("/patient/appointments")}>
                  Мои записи
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => navigate("/patient/doctors")}
                >
                  Найти врача
                </Button>
              </div>
            </div>
          ) : (
            <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
              У вас пока нет предстоящих записей. Выберите врача и запишитесь на удобное время.
            </div>
          )}
        </Card>

        <Card padding="p-6">
          <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Быстрые действия
          </div>

          <div className="mt-5 space-y-4">
            <ActionCard
              title="Записаться на приём"
              description="Откройте список врачей, выберите услугу и свободный слот."
              actionLabel="Перейти к врачам"
              onClick={() => navigate("/patient/doctors")}
            />
            <ActionCard
              title="Открыть мои записи"
              description="Проверьте будущие и прошлые приёмы, при необходимости отмените запись."
              actionLabel="Открыть список"
              onClick={() => navigate("/patient/appointments")}
            />
            <ActionCard
              title="Обновить профиль"
              description="Проверьте контакты, дату рождения и данные для экстренной связи."
              actionLabel="Перейти в профиль"
              onClick={() => navigate("/patient/profile")}
            />
          </div>
        </Card>
      </div>

      <Card padding="p-6">
        <div className="mb-4 flex items-center justify-between gap-3">
          <div className="text-[28px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Последние приёмы
          </div>
          <Button variant="ghost" onClick={() => navigate("/patient/appointments")}>
            Открыть историю
          </Button>
        </div>

        {history.length > 0 ? (
          <div className="space-y-4">
            {history.map((item) => (
              <div
                key={item.id}
                className="flex flex-col gap-4 rounded-[22px] border border-[#E6EEE7] bg-white p-5 lg:flex-row lg:items-center lg:justify-between"
              >
                <div className="flex min-w-0 items-center gap-4">
                  <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-[#EEF4EF] text-sm font-extrabold text-[#4F9A61]">
                    {getInitials(item.doctor, "В")}
                  </div>
                  <div className="min-w-0">
                    <div className="truncate text-[18px] font-bold text-[#183B2B]">
                      {item.doctor}
                    </div>
                    <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                      {item.service}
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                  <div className="text-[15px] font-medium text-[#7A8F84]">
                    {formatDate(item.startTime)}
                  </div>
                  <Badge variant={getAppointmentStatusVariant(item.status)}>
                    {getAppointmentStatusLabel(item.status, true)}
                  </Badge>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            История посещений пока отсутствует.
          </div>
        )}
      </Card>
    </div>
  );
}
