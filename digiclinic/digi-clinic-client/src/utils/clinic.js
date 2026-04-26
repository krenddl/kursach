const appointmentStatusMeta = {
  Scheduled: {
    code: 1,
    label: "Запланирован",
    patientLabel: "Предстоящий",
    variant: "gray",
  },
  Completed: {
    code: 2,
    label: "Завершён",
    patientLabel: "Завершён",
    variant: "green",
  },
  Cancelled: {
    code: 3,
    label: "Отменён",
    patientLabel: "Отменён",
    variant: "red",
  },
  NoShow: {
    code: 4,
    label: "Неявка",
    patientLabel: "Неявка",
    variant: "red",
  },
};

export const appointmentStatusOptions = Object.entries(appointmentStatusMeta).map(
  ([value, item]) => ({
    value,
    code: item.code,
    label: item.label,
  })
);

export function formatDate(value, withYear = true) {
  if (!value) return "—";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";

  return date.toLocaleDateString("ru-RU", {
    day: "numeric",
    month: "long",
    ...(withYear ? { year: "numeric" } : {}),
  });
}

export function formatTime(value) {
  if (!value) return "—";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";

  return date.toLocaleTimeString("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function formatDateTime(value) {
  if (!value) return "—";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";

  return date.toLocaleString("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function toDateInputValue(value) {
  if (!value) return "";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

export function getInitials(name, fallback = "Л") {
  if (!name) return fallback;

  const initials = String(name)
    .trim()
    .split(/\s+/)
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return initials || fallback;
}

export function formatCurrency(value) {
  if (value == null || Number.isNaN(Number(value))) return "—";

  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "RUB",
    maximumFractionDigits: 0,
  }).format(Number(value));
}

export function calculateAge(value) {
  if (!value) return "—";

  const birthDate = new Date(value);
  if (Number.isNaN(birthDate.getTime())) return "—";

  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDiff = today.getMonth() - birthDate.getMonth();

  if (
    monthDiff < 0 ||
    (monthDiff === 0 && today.getDate() < birthDate.getDate())
  ) {
    age -= 1;
  }

  return age;
}

export function getGenderLabel(value) {
  if (!value) return "Не указан";

  const normalized = String(value).toLowerCase();
  if (normalized === "male" || normalized === "мужской") return "Мужской";
  if (normalized === "female" || normalized === "женский") return "Женский";

  return value;
}

export function getRoleLabel(role) {
  if (role === "Admin") return "Администратор";
  if (role === "Doctor") return "Врач";
  return "Пациент";
}

export function getDisplayNameFromEmail(email) {
  if (!email) return "Пользователь";
  return email.split("@")[0];
}

export function getAppointmentStatusLabel(status, patientView = false) {
  const item = appointmentStatusMeta[status] || appointmentStatusMeta.Scheduled;
  return patientView ? item.patientLabel : item.label;
}

export function getAppointmentStatusVariant(status) {
  return (appointmentStatusMeta[status] || appointmentStatusMeta.Scheduled)
    .variant;
}

export function getAppointmentStatusCode(status) {
  return (appointmentStatusMeta[status] || appointmentStatusMeta.Scheduled).code;
}

export function isUpcomingAppointment(item) {
  if (!item?.startTime) return item?.status === "Scheduled";

  const start = new Date(item.startTime);
  if (Number.isNaN(start.getTime())) return item?.status === "Scheduled";

  return item.status === "Scheduled" && start >= new Date();
}

export function groupByDay(items, getDate) {
  return items.reduce((acc, item) => {
    const raw = getDate(item);
    const date = raw ? new Date(raw) : null;
    if (!date || Number.isNaN(date.getTime())) return acc;

    const key = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    ).toISOString();

    if (!acc[key]) acc[key] = [];
    acc[key].push(item);
    return acc;
  }, {});
}

export function getPeriodStart(period) {
  const end = new Date();
  const start = new Date();

  if (period === "7d") {
    start.setDate(end.getDate() - 6);
  } else if (period === "90d") {
    start.setDate(end.getDate() - 89);
  } else {
    start.setDate(end.getDate() - 29);
  }

  start.setHours(0, 0, 0, 0);
  return start;
}

export function buildDailyStatusSeries(appointments, days) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const normalizedDates = appointments
    .map((appointment) => {
      const date = appointment?.startTime ? new Date(appointment.startTime) : null;
      if (!date || Number.isNaN(date.getTime())) return null;

      return new Date(date.getFullYear(), date.getMonth(), date.getDate());
    })
    .filter(Boolean)
    .sort((left, right) => left - right);

  let end = new Date(today);
  const defaultStart = new Date(end);
  defaultStart.setDate(end.getDate() - (days - 1));
  defaultStart.setHours(0, 0, 0, 0);

  const hasItemsInDefaultWindow = normalizedDates.some(
    (date) => date >= defaultStart && date <= end
  );

  if (!hasItemsInDefaultWindow && normalizedDates.length > 0) {
    end = new Date(normalizedDates[normalizedDates.length - 1]);
  }

  const start = new Date(end);
  start.setDate(end.getDate() - (days - 1));
  start.setHours(0, 0, 0, 0);

  const items = Array.from({ length: days }, (_, index) => {
    const current = new Date(start);
    current.setDate(start.getDate() + index);

    const label = current.toLocaleDateString("ru-RU", {
      day: "numeric",
      month: days > 14 ? "short" : undefined,
    });

    return {
      key: current.toISOString(),
      label,
      completed: 0,
      cancelled: 0,
      scheduled: 0,
      noShow: 0,
    };
  });

  appointments.forEach((appointment) => {
    const date = appointment?.startTime ? new Date(appointment.startTime) : null;
    if (!date || Number.isNaN(date.getTime())) return;

    const normalized = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    ).toISOString();

    const bucket = items.find((item) => item.key === normalized);
    if (!bucket) return;

    if (appointment.status === "Completed") bucket.completed += 1;
    else if (appointment.status === "Cancelled") bucket.cancelled += 1;
    else if (appointment.status === "NoShow") bucket.noShow += 1;
    else bucket.scheduled += 1;
  });

  return items;
}

export function buildDailyScheduledSeries(appointments, days) {
  const start = new Date();
  start.setHours(0, 0, 0, 0);

  const items = Array.from({ length: days }, (_, index) => {
    const current = new Date(start);
    current.setDate(start.getDate() + index);

    const label = current.toLocaleDateString("ru-RU", {
      day: "numeric",
      month: days > 14 ? "short" : undefined,
    });

    return {
      key: current.toISOString(),
      label,
      count: 0,
    };
  });

  appointments.forEach((appointment) => {
    if (appointment?.status !== "Scheduled" || !appointment?.startTime) return;

    const date = new Date(appointment.startTime);
    if (Number.isNaN(date.getTime())) return;

    const normalized = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    ).toISOString();

    const bucket = items.find((item) => item.key === normalized);
    if (!bucket) return;

    bucket.count += 1;
  });

  return items;
}

export function buildCompletedAppointmentsSeries(appointments, period) {
  const days = period === "7d" ? 7 : period === "90d" ? 90 : 30;
  const end = new Date();
  end.setHours(23, 59, 59, 999);

  const start = new Date(end);
  start.setDate(end.getDate() - (days - 1));
  start.setHours(0, 0, 0, 0);

  const completedDates = appointments
    .filter((appointment) => appointment?.status === "Completed")
    .map((appointment) =>
      appointment?.startTime ? new Date(appointment.startTime) : null
    )
    .filter(
      (date) =>
        date &&
        !Number.isNaN(date.getTime()) &&
        date >= start &&
        date <= end
    );

  const formatShortDate = (date) =>
    date.toLocaleDateString("ru-RU", {
      day: "numeric",
      month: "short",
    });

  const countInBucket = (bucketStart, bucketEnd) =>
    completedDates.filter((date) => date >= bucketStart && date <= bucketEnd)
      .length;

  const makeBucket = (bucketStart, bucketEnd, label) => ({
    key: bucketStart.toISOString(),
    label,
    count: countInBucket(bucketStart, bucketEnd),
  });

  if (period === "90d") {
    const items = [];
    let monthCursor = new Date(start.getFullYear(), start.getMonth(), 1);

    while (monthCursor <= end) {
      const nextMonth = new Date(
        monthCursor.getFullYear(),
        monthCursor.getMonth() + 1,
        1
      );
      const bucketStart = new Date(
        Math.max(monthCursor.getTime(), start.getTime())
      );
      const bucketEnd = new Date(Math.min(nextMonth.getTime() - 1, end.getTime()));
      const label = monthCursor.toLocaleDateString("ru-RU", {
        month: "short",
      });

      items.push(makeBucket(bucketStart, bucketEnd, label));
      monthCursor = nextMonth;
    }

    return items;
  }

  const bucketSize = period === "7d" ? 1 : 7;
  const items = [];
  let cursor = new Date(start);

  while (cursor <= end) {
    const bucketStart = new Date(cursor);
    const bucketEnd = new Date(bucketStart);
    bucketEnd.setDate(bucketStart.getDate() + bucketSize - 1);
    bucketEnd.setHours(23, 59, 59, 999);

    if (bucketEnd > end) {
      bucketEnd.setTime(end.getTime());
    }

    items.push(makeBucket(bucketStart, bucketEnd, formatShortDate(bucketStart)));
    cursor.setDate(cursor.getDate() + bucketSize);
  }

  return items;
}
