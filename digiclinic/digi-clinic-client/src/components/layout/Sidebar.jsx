import { NavLink } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";

const menus = {
  Patient: [
    { to: "/patient/dashboard", label: "Главная" },
    { to: "/patient/doctors", label: "Врачи" },
    { to: "/patient/appointments", label: "Записи" },
    { to: "/patient/chat", label: "Чат" },
    { to: "/patient/profile", label: "Профиль" },
  ],
  Doctor: [
    { to: "/doctor/dashboard", label: "Главная" },
    { to: "/doctor/appointments", label: "Приёмы" },
    { to: "/doctor/patients", label: "Пациенты" },
    { to: "/doctor/chat", label: "Чат" },
    { to: "/doctor/profile", label: "Профиль" },
  ],
  Admin: [
    { to: "/admin/dashboard", label: "Главная" },
    { to: "/admin/doctors", label: "Врачи и услуги" },
    { to: "/admin/schedule", label: "Расписание" },
    { to: "/admin/patients", label: "Пациенты" },
    { to: "/admin/appointments", label: "Записи" },
    { to: "/admin/profile", label: "Профиль" },
  ],
};

function MenuItem({ to, label }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        `relative flex items-center rounded-[18px] px-4 py-3 text-[16px] font-semibold transition ${
          isActive
            ? "bg-[#E8F4EA] text-[#3F8B54]"
            : "text-[#5E7167] hover:bg-[#F3F9F4]"
        }`
      }
    >
      {({ isActive }) => (
        <>
          {isActive ? (
            <span className="absolute left-[-24px] top-1/2 h-10 w-1 -translate-y-1/2 rounded-r-full bg-[#6FC17A]" />
          ) : null}
          <span>{label}</span>
        </>
      )}
    </NavLink>
  );
}

export default function Sidebar() {
  const { user } = useAuth();
  const roleMenus = menus[user?.role] || [];

  return (
    <aside className="w-[300px] shrink-0 border-r border-[#E6EEE7] bg-[#FBFDFB]">
      <div className="flex h-full flex-col">
        <div className="border-b border-[#E6EEE7] px-8 py-7">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-[#6FC17A] text-xl font-extrabold text-white shadow-[0_8px_18px_rgba(111,193,122,0.2)]">
              +
            </div>
            <div>
              <div className="text-[20px] font-extrabold tracking-[-0.03em] text-[#183B2B]">
                Лотос
              </div>
              <div className="text-sm text-[#97A99C]">Цифровая клиника</div>
            </div>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-6">
          <div className="mb-4 px-2 text-xs font-bold uppercase tracking-[0.18em] text-[#8FA095]">
            Основное
          </div>

          <nav className="flex flex-col gap-2">
            {roleMenus.map((item) => (
              <MenuItem key={item.to} to={item.to} label={item.label} />
            ))}
          </nav>
        </div>
      </div>
    </aside>
  );
}
