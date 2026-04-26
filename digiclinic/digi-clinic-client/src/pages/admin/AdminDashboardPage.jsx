import { useEffect, useMemo, useState } from "react";
import {
  BarChart,
  Button,
  Card,
  Modal,
  SectionTitle,
  StatCard,
} from "../../components/ui";
import { getAllAppointments } from "../../services/appointmentService";
import { getAdminDashboard } from "../../services/dashboardService";
import { buildCompletedAppointmentsSeries, formatDateTime } from "../../utils/clinic";

export default function AdminDashboardPage() {
  const [dashboard, setDashboard] = useState(null);
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isRecentModalOpen, setIsRecentModalOpen] = useState(false);

  useEffect(() => {
    Promise.all([getAdminDashboard(), getAllAppointments()])
      .then(([dashboardResponse, appointmentsResponse]) => {
        setDashboard(dashboardResponse.data);
        setAppointments(
          Array.isArray(appointmentsResponse.data) ? appointmentsResponse.data : []
        );
      })
      .catch((error) => {
        console.error("Ошибка загрузки административной аналитики:", error);
      })
      .finally(() => setLoading(false));
  }, []);

  const chartData = useMemo(
    () => buildCompletedAppointmentsSeries(appointments, "30d"),
    [appointments]
  );

  const recentAppointments = useMemo(() => {
    return [...appointments]
      .sort((left, right) => new Date(right.startTime) - new Date(left.startTime))
      .slice(0, 10);
  }, [appointments]);

  const recentAppointmentsPreview = useMemo(
    () => recentAppointments.slice(0, 3),
    [recentAppointments]
  );

  if (loading) {
    return <div className="text-[#8A94A6]">Загрузка аналитики администратора...</div>;
  }

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Панель администратора"
        subtitle="Контролируйте основные метрики клиники, загрузку врачей и завершённые приёмы."
      />

      <div className="grid grid-cols-1 gap-5 md:grid-cols-4">
        <StatCard title="Врачи" value={dashboard?.totalDoctors ?? 0} />
        <StatCard title="Пациенты" value={dashboard?.totalPatients ?? 0} accent="gray" />
        <StatCard title="Записи" value={dashboard?.totalAppointments ?? 0} />
        <StatCard
          title="Завершено"
          value={dashboard?.byStatus?.completed ?? 0}
          subtitle={`Отменено: ${dashboard?.byStatus?.cancelled ?? 0} · Неявка: ${dashboard?.byStatus?.noShow ?? 0}`}
          accent="green"
        />
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1.4fr_0.9fr]">
        <Card padding="p-6">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Завершённые приёмы клиники
            </div>
            <div className="text-[15px] font-medium text-[#7A8F84]">
              Последние 30 дней
            </div>
          </div>

          <BarChart
            data={chartData}
            heightClass="h-[280px]"
            emptyMessage="За последние 30 дней пока нет завершённых приёмов."
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
          <div className="flex items-center justify-between gap-3">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Последние 10 визитов
            </div>
            {recentAppointments.length > recentAppointmentsPreview.length ? (
              <Button
                variant="ghost"
                onClick={() => setIsRecentModalOpen(true)}
                className="px-4 py-2"
              >
                Показать 10
              </Button>
            ) : null}
          </div>

          <div className="mt-5 space-y-4">
            {recentAppointmentsPreview.length > 0 ? (
              recentAppointmentsPreview.map((item) => (
                <div
                  key={item.id}
                  className="rounded-[22px] border border-[#E6EEE7] bg-white p-4"
                >
                  <div className="text-[16px] font-bold text-[#183B2B]">
                    {item.patient}
                  </div>
                  <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                    {item.doctor}
                  </div>
                  <div className="mt-2 text-[14px] font-medium text-[#7A8F84]">
                    {item.service}
                  </div>
                  <div className="mt-3 text-[14px] font-semibold text-[#355244]">
                    {formatDateTime(item.startTime)}
                  </div>
                </div>
              ))
            ) : (
              <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                Недавние записи пока отсутствуют.
              </div>
            )}
          </div>
        </Card>
      </div>

      <Card padding="p-6">
        <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
          Загрузка врачей
        </div>

        <div className="space-y-4">
          {dashboard?.doctorLoad?.length ? (
            dashboard.doctorLoad.map((doctor) => {
              const percent =
                doctor.totalSlots > 0
                  ? Math.round((doctor.bookedSlots / doctor.totalSlots) * 100)
                  : 0;

              return (
                <div
                  key={doctor.doctorId}
                  className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                      <div className="text-[18px] font-bold text-[#183B2B]">
                        {doctor.doctorName}
                      </div>
                      <div className="mt-1 text-[15px] font-medium text-[#7A8F84]">
                        Слотов: {doctor.totalSlots} · Занято: {doctor.bookedSlots} · Свободно: {doctor.freeSlots}
                      </div>
                    </div>
                    <div className="text-[18px] font-extrabold text-[#4F9A61]">
                      {percent}%
                    </div>
                  </div>

                  <div className="mt-4 h-3 rounded-full bg-[#EDF4EF]">
                    <div
                      className="h-3 rounded-full bg-[#86B89B]"
                      style={{ width: `${percent}%` }}
                    />
                  </div>
                </div>
              );
            })
          ) : (
            <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
              Данные по загрузке врачей пока недоступны.
            </div>
          )}
        </div>
      </Card>

      <Modal
        open={isRecentModalOpen}
        title="Последние 10 визитов"
        onClose={() => setIsRecentModalOpen(false)}
      >
        <div className="max-h-[70vh] space-y-4 overflow-y-auto pr-1">
          {recentAppointments.map((item) => (
            <div
              key={item.id}
              className="rounded-[22px] border border-[#E6EEE7] bg-[#FCFEFC] p-4"
            >
              <div className="text-[17px] font-bold text-[#183B2B]">
                {item.patient}
              </div>
              <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                {item.doctor}
              </div>
              <div className="mt-2 text-[14px] font-medium text-[#7A8F84]">
                {item.service}
              </div>
              <div className="mt-3 text-[14px] font-semibold text-[#355244]">
                {formatDateTime(item.startTime)}
              </div>
            </div>
          ))}
        </div>
      </Modal>
    </div>
  );
}
