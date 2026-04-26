import { useEffect, useMemo, useState } from "react";
import {
  Badge,
  Button,
  Card,
  Input,
  SectionTitle,
  StatCard,
} from "../../components/ui";
import { getAllAppointments } from "../../services/appointmentService";
import {
  formatDate,
  formatTime,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
} from "../../utils/clinic";
import { controlClassName } from "../../utils/formStyles";

const PAGE_SIZE = 8;

export default function AdminAppointmentsPage() {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [page, setPage] = useState(1);

  useEffect(() => {
    getAllAppointments()
      .then((response) => {
        setAppointments(Array.isArray(response.data) ? response.data : []);
      })
      .catch((error) => {
        console.error("Ошибка загрузки всех записей:", error);
      })
      .finally(() => setLoading(false));
  }, []);

  const filteredAppointments = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return appointments.filter((item) => {
      const matchesStatus =
        statusFilter === "all" || item.status === statusFilter;

      const matchesQuery =
        !normalizedQuery ||
        item.patient?.toLowerCase().includes(normalizedQuery) ||
        item.doctor?.toLowerCase().includes(normalizedQuery) ||
        item.service?.toLowerCase().includes(normalizedQuery);

      const date = item.startTime ? new Date(item.startTime) : null;
      const matchesFrom =
        !dateFrom ||
        (date && new Date(dateFrom) <= new Date(date.getFullYear(), date.getMonth(), date.getDate()));
      const matchesTo =
        !dateTo ||
        (date && new Date(dateTo) >= new Date(date.getFullYear(), date.getMonth(), date.getDate()));

      return matchesStatus && matchesQuery && matchesFrom && matchesTo;
    });
  }, [appointments, query, statusFilter, dateFrom, dateTo]);

  useEffect(() => {
    setPage(1);
  }, [query, statusFilter, dateFrom, dateTo]);

  const totalPages = Math.max(
    1,
    Math.ceil(filteredAppointments.length / PAGE_SIZE)
  );

  useEffect(() => {
    setPage((currentPage) => Math.min(currentPage, totalPages));
  }, [totalPages]);

  const visibleAppointments = useMemo(() => {
    const start = (page - 1) * PAGE_SIZE;
    return filteredAppointments.slice(start, start + PAGE_SIZE);
  }, [filteredAppointments, page]);

  const visibleRange = useMemo(() => {
    if (filteredAppointments.length === 0) return "0 из 0";

    const from = (page - 1) * PAGE_SIZE + 1;
    const to = Math.min(page * PAGE_SIZE, filteredAppointments.length);
    return `${from}-${to} из ${filteredAppointments.length}`;
  }, [filteredAppointments.length, page]);

  const stats = useMemo(() => {
    return {
      total: appointments.length,
      scheduled: appointments.filter((item) => item.status === "Scheduled").length,
      completed: appointments.filter((item) => item.status === "Completed").length,
      cancelled: appointments.filter((item) => item.status === "Cancelled").length,
    };
  }, [appointments]);

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Все записи"
        subtitle="Просматривайте расписание клиники целиком, фильтруйте записи по статусу, дате и участникам визита."
      />

      <div className="grid grid-cols-1 gap-5 md:grid-cols-4">
        <StatCard title="Всего записей" value={stats.total} />
        <StatCard title="Запланировано" value={stats.scheduled} accent="gray" />
        <StatCard title="Завершено" value={stats.completed} />
        <StatCard title="Отменено" value={stats.cancelled} accent="red" />
      </div>

      <Card padding="p-5">
        <div className="grid grid-cols-1 gap-4 xl:grid-cols-[1fr_220px_180px_180px]">
          <Input
            leftIcon="⌕"
            placeholder="Поиск по пациенту, врачу или услуге"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />

          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            className={controlClassName}
          >
            <option value="all">Все статусы</option>
            <option value="Scheduled">Запланировано</option>
            <option value="Completed">Завершено</option>
            <option value="Cancelled">Отменено</option>
            <option value="NoShow">Неявка</option>
          </select>

          <input
            type="date"
            value={dateFrom}
            onChange={(event) => setDateFrom(event.target.value)}
            className={controlClassName}
          />

          <input
            type="date"
            value={dateTo}
            onChange={(event) => setDateTo(event.target.value)}
            className={controlClassName}
          />
        </div>
      </Card>

      <Card padding="p-6">
        {!loading && filteredAppointments.length > 0 ? (
          <div className="mb-5 flex flex-col gap-3 border-b border-[#E6EEE7] pb-5 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <div className="text-[20px] font-extrabold tracking-[-0.03em] text-[#183B2B]">
                Журнал записей
              </div>
              <div className="mt-1 text-[14px] font-medium text-[#7A8F84]">
                Показано {visibleRange}. На странице не больше {PAGE_SIZE} записей.
              </div>
            </div>

            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="secondary"
                className="px-4 py-2"
                disabled={page === 1}
                onClick={() =>
                  setPage((currentPage) => Math.max(1, currentPage - 1))
                }
              >
                Назад
              </Button>
              <div className="rounded-[16px] border border-[#DCE9DE] bg-[#F7FBF7] px-4 py-2 text-[14px] font-bold text-[#4F9A61]">
                {page} / {totalPages}
              </div>
              <Button
                type="button"
                variant="secondary"
                className="px-4 py-2"
                disabled={page === totalPages}
                onClick={() =>
                  setPage((currentPage) =>
                    Math.min(totalPages, currentPage + 1)
                  )
                }
              >
                Вперёд
              </Button>
            </div>
          </div>
        ) : null}

        {loading ? (
          <div className="text-[#8A94A6]">Загрузка записей...</div>
        ) : filteredAppointments.length === 0 ? (
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            По выбранным условиям записи не найдены.
          </div>
        ) : (
          <div className="space-y-4">
            {visibleAppointments.map((item) => (
              <div
                key={item.id}
                className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
              >
                <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <div className="text-[18px] font-bold text-[#183B2B]">
                      {item.patient}
                    </div>
                    <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                      Врач: {item.doctor}
                    </div>
                    <div className="mt-1 text-[15px] font-medium text-[#7A8F84]">
                      {item.service}
                    </div>
                    <div className="mt-3 text-[15px] font-semibold text-[#355244]">
                      {formatDate(item.startTime)} · {formatTime(item.startTime)}
                      {item.endTime ? ` — ${formatTime(item.endTime)}` : ""}
                    </div>
                  </div>

                  <Badge variant={getAppointmentStatusVariant(item.status)}>
                    {getAppointmentStatusLabel(item.status)}
                  </Badge>
                </div>

                {item.conclusion ? (
                  <div className="mt-4 rounded-[18px] border border-[#E6EEE7] bg-[#F9FCF9] p-4 text-[15px] leading-7 font-medium text-[#4A6055]">
                    {item.conclusion}
                  </div>
                ) : null}
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
