import { useEffect, useMemo, useState } from "react";
import {
  BarChart,
  Card,
  SectionTitle,
  StatCard,
} from "../../components/ui";
import { getDoctorAppointments } from "../../services/appointmentService";
import {
  buildCompletedAppointmentsSeries,
  formatDateTime,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
  getPeriodStart,
} from "../../utils/clinic";

const periodOptions = [
  { value: "7d", label: "7 дней", days: 7 },
  { value: "30d", label: "30 дней", days: 30 },
  { value: "90d", label: "90 дней", days: 90 },
];

function isValidDate(value) {
  return value instanceof Date && !Number.isNaN(value.getTime());
}

export default function DoctorDashboardPage() {
  const [period, setPeriod] = useState("30d");
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);

    getDoctorAppointments()
      .then((appointmentsResponse) => {
        setAppointments(
          Array.isArray(appointmentsResponse.data) ? appointmentsResponse.data : []
        );
      })
      .catch((error) => {
        console.error("Ошибка загрузки аналитики врача:", error);
      })
      .finally(() => setLoading(false));
  }, []);

  const selectedPeriod = useMemo(
    () => periodOptions.find((item) => item.value === period) || periodOptions[1],
    [period]
  );

  const periodAppointments = useMemo(() => {
    const start = getPeriodStart(period);
    const now = new Date();

    return appointments.filter((item) => {
      const startTime = item?.startTime ? new Date(item.startTime) : null;
      if (!isValidDate(startTime)) return false;

      return startTime >= start && startTime <= now;
    });
  }, [appointments, period]);

  const chartData = useMemo(() => {
    return buildCompletedAppointmentsSeries(appointments, period);
  }, [appointments, period]);

  const stats = useMemo(() => {
    return {
      total: periodAppointments.length,
      scheduled: periodAppointments.filter((item) => item.status === "Scheduled").length,
      completed: periodAppointments.filter((item) => item.status === "Completed").length,
      cancelled: periodAppointments.filter((item) => item.status === "Cancelled").length,
      noShow: periodAppointments.filter((item) => item.status === "NoShow").length,
    };
  }, [periodAppointments]);

  const nearestAppointments = useMemo(() => {
    const now = new Date();

    return appointments
      .filter((item) => {
        const startTime = item?.startTime ? new Date(item.startTime) : null;
        return (
          item.status === "Scheduled" &&
          isValidDate(startTime) &&
          startTime >= now
        );
      })
      .sort((left, right) => new Date(left.startTime) - new Date(right.startTime))
      .slice(0, 4);
  }, [appointments]);

  if (loading) {
    return <div className="text-[#8A94A6]">Загрузка аналитики врача...</div>;
  }

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Аналитика врача"
        subtitle="Отслеживайте свои приёмы, загрузку и завершённые визиты за выбранный период."
      />

      <div className="flex flex-wrap gap-3">
        {periodOptions.map((option) => (
          <button
            key={option.value}
            type="button"
            onClick={() => setPeriod(option.value)}
            className={`rounded-[18px] px-6 py-3 text-[16px] font-semibold transition ${
              period === option.value
                ? "border border-[#D5ECD9] bg-[#EAF7EC] text-[#4F9A61]"
                : "border border-[#E3ECE4] bg-white text-[#6F8278] hover:bg-[#F7FBF7]"
            }`}
          >
            {option.label}
          </button>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-5 md:grid-cols-5">
        <StatCard
          title="Всего за период"
          value={stats.total}
          subtitle={`За ${selectedPeriod.label.toLowerCase()}`}
        />
        <StatCard title="Запланировано" value={stats.scheduled} accent="gray" />
        <StatCard title="Завершено" value={stats.completed} />
        <StatCard title="Отменено" value={stats.cancelled} accent="red" />
        <StatCard title="Неявка" value={stats.noShow} accent="yellow" />
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1.35fr_0.95fr]">
        <Card padding="p-6">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Завершённые приёмы
            </div>
            <div className="text-[15px] font-medium text-[#7A8F84]">
              Период: {selectedPeriod.label}
            </div>
          </div>

          <BarChart
            data={chartData}
            heightClass="h-[280px]"
            emptyMessage="За выбранный период пока нет завершённых приёмов."
            series={[
              {
                key: "count",
                label: "Завершённые приёмы",
                colorClass: "bg-gradient-to-t from-[#4F9A61] to-[#9FD5A8]",
              },
            ]}
          />
        </Card>

        <Card padding="p-6">
          <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Ближайшие приёмы
          </div>

          <div className="mt-5 space-y-4">
            {nearestAppointments.length > 0 ? (
              nearestAppointments.map((item) => (
                <div
                  key={item.id}
                  className="rounded-[22px] border border-[#E6EEE7] bg-white p-4"
                >
                  <div className="flex items-center justify-between gap-3">
                    <div className="text-[17px] font-bold text-[#183B2B]">
                      {item.patient}
                    </div>
                    <span
                      className={`rounded-full px-3 py-1.5 text-sm font-semibold ${
                        getAppointmentStatusVariant(item.status) === "green"
                          ? "bg-[#EAF7EC] text-[#4F9A61]"
                          : "bg-[#F4F7F4] text-[#7A8F84]"
                      }`}
                    >
                      {getAppointmentStatusLabel(item.status)}
                    </span>
                  </div>
                  <div className="mt-2 text-[15px] font-medium text-[#6F8278]">
                    {item.service}
                  </div>
                  <div className="mt-2 text-[15px] font-semibold text-[#355244]">
                    {formatDateTime(item.startTime)}
                  </div>
                </div>
              ))
            ) : (
              <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                Предстоящих приёмов пока нет.
              </div>
            )}
          </div>
        </Card>
      </div>
    </div>
  );
}
