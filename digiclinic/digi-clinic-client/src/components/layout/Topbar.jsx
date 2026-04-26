import { useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { getMyProfile } from "../../services/profileService";
import {
  getDisplayNameFromEmail,
  getInitials,
  getRoleLabel,
} from "../../utils/clinic";

const pageTitles = [
  ["/patient/dashboard", "Главная"],
  ["/patient/doctors", "Врачи"],
  ["/patient/appointments", "Мои записи"],
  ["/patient/book", "Запись на приём"],
  ["/patient/chat", "Чат"],
  ["/patient/profile", "Профиль"],
  ["/doctor/dashboard", "Аналитика врача"],
  ["/doctor/appointments", "Расписание"],
  ["/doctor/patients", "Пациенты"],
  ["/doctor/chat", "Чат"],
  ["/doctor/profile", "Профиль"],
  ["/admin/dashboard", "Панель администратора"],
  ["/admin/doctors", "Врачи и услуги"],
  ["/admin/patients", "Пациенты"],
  ["/admin/appointments", "Записи"],
  ["/admin/schedule", "Расписание"],
  ["/admin/profile", "Профиль"],
];

function getWorkspaceTitle(role) {
  if (role === "Admin") return "Кабинет администратора";
  if (role === "Doctor") return "Кабинет врача";
  return "Кабинет пациента";
}

function getPageTitle(pathname) {
  const match = pageTitles.find(([path]) => pathname.startsWith(path));
  return match?.[1] || "Лотос";
}

export default function Topbar() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [profile, setProfile] = useState(null);

  useEffect(() => {
    if (!user?.id) {
      setProfile(null);
      return;
    }

    let cancelled = false;

    getMyProfile()
      .then((response) => {
        if (!cancelled) {
          setProfile(response.data);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setProfile(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [user?.id]);

  const fullName = useMemo(() => {
    if (profile?.firstName || profile?.lastName) {
      return [profile?.firstName, profile?.lastName].filter(Boolean).join(" ");
    }

    return getDisplayNameFromEmail(user?.email);
  }, [profile?.firstName, profile?.lastName, user?.email]);

  const initials = useMemo(() => {
    return getInitials(fullName, user?.role?.[0] || "Л");
  }, [fullName, user?.role]);

  const roleLabel = getRoleLabel(profile?.role || user?.role);
  const pageTitle = getPageTitle(location.pathname);
  const workspaceTitle = getWorkspaceTitle(profile?.role || user?.role);

  return (
    <div className="flex h-[78px] items-center justify-between gap-5 border-b border-[#E6EEE7] bg-[#FBFDFB] px-6 lg:px-8">
      <div className="min-w-0">
        <div className="text-xs font-bold uppercase tracking-[0.14em] text-[#8FA095]">
          {workspaceTitle}
        </div>
        <div className="mt-1 truncate text-[18px] font-extrabold text-[#183B2B]">
          {pageTitle}
        </div>
      </div>

      <div className="flex shrink-0 items-center gap-3">
        <div className="flex items-center gap-3 rounded-[20px] border border-[#E2EAE3] bg-white px-3 py-2 shadow-[0_4px_12px_rgba(111,193,122,0.03)]">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[#EAF7EC] text-sm font-extrabold text-[#4F9A61]">
            {initials}
          </div>
          <div className="hidden min-w-[160px] sm:block">
            <div className="truncate text-[15px] font-bold text-[#355244]">
              {fullName}
            </div>
            <div className="text-[12px] font-medium text-[#8FA095]">
              {roleLabel}
            </div>
          </div>
        </div>

        <button
          type="button"
          onClick={logout}
          className="rounded-2xl bg-[#6FC17A] px-4 py-2.5 text-sm font-bold text-white shadow-[0_10px_20px_rgba(111,193,122,0.18)] transition hover:bg-[#5FB06A]"
        >
          Выйти
        </button>
      </div>
    </div>
  );
}
