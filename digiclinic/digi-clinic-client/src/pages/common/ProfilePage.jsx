import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  SectionTitle,
  Textarea,
} from "../../components/ui";
import { useAuth } from "../../auth/AuthContext";
import { getMyProfile, updateMyProfile } from "../../services/profileService";
import { getSpecializations } from "../../services/specializationService";
import {
  createTelegramLinkCode,
  unlinkTelegram,
} from "../../services/telegramService";
import {
  getRoleLabel,
  getGenderLabel,
  toDateInputValue,
} from "../../utils/clinic";
import {
  controlClassName,
  labelClassName,
  textareaClassName,
} from "../../utils/formStyles";

function buildInitialForm(profile) {
  return {
    firstName: profile?.firstName || "",
    lastName: profile?.lastName || "",
    phone: profile?.phone || "",
    birthDate: toDateInputValue(profile?.patientProfile?.birthDate),
    gender: profile?.patientProfile?.gender || "",
    address: profile?.patientProfile?.address || "",
    emergencyContact: profile?.patientProfile?.emergencyContact || "",
    specializationId: profile?.doctorProfile?.specializationId || "",
    experienceYears: profile?.doctorProfile?.experienceYears || 0,
    cabinetNumber: profile?.doctorProfile?.cabinetNumber || "",
    bio: profile?.doctorProfile?.bio || "",
  };
}

export default function ProfilePage() {
  const { user } = useAuth();
  const telegramBotUrl = import.meta.env.VITE_TELEGRAM_BOT_URL || "";
  const [profile, setProfile] = useState(null);
  const [form, setForm] = useState(buildInitialForm(null));
  const [specializations, setSpecializations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [telegramLoading, setTelegramLoading] = useState(false);
  const [telegramLink, setTelegramLink] = useState(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const tasks = [getMyProfile()];

    if (user?.role === "Doctor") {
      tasks.push(getSpecializations());
    }

    Promise.all(tasks)
      .then(([profileRes, specializationsRes]) => {
        setProfile(profileRes.data);
        setForm(buildInitialForm(profileRes.data));
        if (specializationsRes?.data) {
          setSpecializations(specializationsRes.data);
        }
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить профиль."
        );
      })
      .finally(() => setLoading(false));
  }, [user?.role]);

  const details = useMemo(() => {
    if (!profile) return [];

    const base = [
      { label: "Роль", value: getRoleLabel(profile.role) },
      { label: "Электронная почта", value: profile.email },
      { label: "Телефон", value: profile.phone || "Не указан" },
    ];

    if (profile.patientProfile) {
      base.push(
        {
          label: "Пол",
          value: getGenderLabel(profile.patientProfile.gender),
        },
        {
          label: "Адрес",
          value: profile.patientProfile.address || "Не указан",
        },
        {
          label: "Telegram",
          value: profile.telegramLinked ? "Привязан" : "Не привязан",
        }
      );
    }

    if (profile.doctorProfile) {
      base.push(
        {
          label: "Специализация",
          value: profile.doctorProfile.specialization || "Не указана",
        },
        {
          label: "Кабинет",
          value: profile.doctorProfile.cabinetNumber || "Не указан",
        }
      );
    }

    return base;
  }, [profile]);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const refreshProfile = async () => {
    const updated = await getMyProfile();
    setProfile(updated.data);
    setForm(buildInitialForm(updated.data));
    return updated.data;
  };

  const handleCreateTelegramCode = async () => {
    setTelegramLoading(true);
    setTelegramLink(null);
    setMessage("");
    setError("");

    try {
      const response = await createTelegramLinkCode();
      setTelegramLink(response.data);
      await refreshProfile();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось получить Telegram-код."
      );
    } finally {
      setTelegramLoading(false);
    }
  };

  const handleUnlinkTelegram = async () => {
    setTelegramLoading(true);
    setTelegramLink(null);
    setMessage("");
    setError("");

    try {
      await unlinkTelegram();
      await refreshProfile();
      setMessage("Telegram отвязан от профиля.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось отвязать Telegram."
      );
    } finally {
      setTelegramLoading(false);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setSaving(true);
    setMessage("");
    setError("");

    try {
      await updateMyProfile({
        firstName: form.firstName,
        lastName: form.lastName,
        phone: form.phone || null,
        birthDate: form.birthDate ? `${form.birthDate}T00:00:00Z` : null,
        gender: form.gender || null,
        address: form.address || null,
        emergencyContact: form.emergencyContact || null,
        specializationId: form.specializationId ? Number(form.specializationId) : null,
        experienceYears:
          user?.role === "Doctor" ? Number(form.experienceYears || 0) : null,
        cabinetNumber: form.cabinetNumber || null,
        bio: form.bio || null,
      });

      await refreshProfile();
      setMessage("Профиль успешно обновлён.");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сохранить изменения."
      );
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="text-[#8A94A6]">Загрузка профиля...</div>;
  }

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Профиль"
        subtitle="Проверьте личные данные и обновите информацию при необходимости."
      />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[0.95fr_1.25fr]">
        <Card padding="p-6" className="h-fit">
          <div className="flex items-center gap-4">
            <div className="flex h-[72px] w-[72px] items-center justify-center rounded-[24px] bg-[#EAF7EC] text-[26px] font-extrabold text-[#4F9A61]">
              {profile?.firstName?.[0] || user?.role?.[0] || "Л"}
            </div>
            <div>
              <div className="text-[28px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                {profile?.firstName} {profile?.lastName}
              </div>
              <div className="mt-1 text-[16px] font-medium text-[#6F8278]">
                {getRoleLabel(profile?.role)}
              </div>
            </div>
          </div>

          <div className="mt-6 space-y-4">
            {details.map((item) => (
              <div
                key={item.label}
                className="rounded-[20px] border border-[#E6EEE7] bg-[#FCFEFC] px-4 py-3"
              >
                <div className="text-sm font-semibold text-[#8FA095]">
                  {item.label}
                </div>
                <div className="mt-1 text-[16px] font-semibold text-[#355244]">
                  {item.value}
                </div>
              </div>
            ))}
          </div>

          {user?.role === "Patient" ? (
            <div className="mt-6 rounded-[24px] border border-[#D9E7DB] bg-[#F7FBF7] p-4">
              <div className="text-[17px] font-extrabold text-[#183B2B]">
                Telegram-бот
              </div>
              <div className="mt-2 text-[14px] leading-6 font-medium text-[#6F8278]">
                Получите код и отправьте команду боту. Код действует 15 минут.
              </div>

              {telegramBotUrl ? (
                <a
                  href={telegramBotUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="mt-4 inline-flex w-full items-center justify-center rounded-[18px] bg-[#6FC17A] px-5 py-3 text-[15px] font-semibold text-white shadow-[0_12px_24px_rgba(111,193,122,0.22)] transition hover:bg-[#5FB06A]"
                >
                  Открыть Telegram-бота
                </a>
              ) : (
                <div className="mt-4 rounded-[18px] border border-[#DCE9DE] bg-white px-4 py-3 text-sm font-semibold text-[#6F8278]">
                  Ссылка на бота не настроена.
                </div>
              )}

              {telegramLink ? (
                <div className="mt-4 rounded-[18px] border border-[#D5ECD9] bg-white p-4">
                  <div className="text-sm font-semibold text-[#8FA095]">
                    Команда для Telegram
                  </div>
                  <div className="mt-2 select-all text-[20px] font-extrabold text-[#4F9A61]">
                    {telegramLink.command}
                  </div>
                </div>
              ) : null}

              <div className="mt-4 flex flex-col gap-2 sm:flex-row">
                <Button
                  type="button"
                  variant="secondary"
                  disabled={telegramLoading}
                  onClick={handleCreateTelegramCode}
                  className="flex-1"
                >
                  {telegramLoading ? "Готовим..." : "Получить код"}
                </Button>

                {profile?.telegramLinked ? (
                  <Button
                    type="button"
                    variant="danger"
                    disabled={telegramLoading}
                    onClick={handleUnlinkTelegram}
                    className="flex-1"
                  >
                    Отвязать
                  </Button>
                ) : null}
              </div>
            </div>
          ) : null}
        </Card>

        <Card padding="p-6">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
              <div>
                <label className={labelClassName}>Имя</label>
                <input
                  name="firstName"
                  value={form.firstName}
                  onChange={handleChange}
                  className={controlClassName}
                />
              </div>

              <div>
                <label className={labelClassName}>Фамилия</label>
                <input
                  name="lastName"
                  value={form.lastName}
                  onChange={handleChange}
                  className={controlClassName}
                />
              </div>
            </div>

            <div>
              <label className={labelClassName}>Телефон</label>
              <input
                name="phone"
                value={form.phone}
                onChange={handleChange}
                className={controlClassName}
                placeholder="+7 777 123 45 67"
              />
            </div>

            {user?.role === "Patient" ? (
              <>
                <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                  <div>
                    <label className={labelClassName}>Дата рождения</label>
                    <input
                      type="date"
                      name="birthDate"
                      value={form.birthDate}
                      onChange={handleChange}
                      className={controlClassName}
                    />
                  </div>

                  <div>
                    <label className={labelClassName}>Пол</label>
                    <select
                      name="gender"
                      value={form.gender}
                      onChange={handleChange}
                      className={controlClassName}
                    >
                      <option value="">Не указан</option>
                      <option value="Male">Мужской</option>
                      <option value="Female">Женский</option>
                    </select>
                  </div>
                </div>

                <div>
                  <label className={labelClassName}>Адрес</label>
                  <input
                    name="address"
                    value={form.address}
                    onChange={handleChange}
                    className={controlClassName}
                  />
                </div>

                <div>
                  <label className={labelClassName}>Контакт для экстренной связи</label>
                  <input
                    name="emergencyContact"
                    value={form.emergencyContact}
                    onChange={handleChange}
                    className={controlClassName}
                  />
                </div>
              </>
            ) : null}

            {user?.role === "Doctor" ? (
              <>
                <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                  <div>
                    <label className={labelClassName}>Специализация</label>
                    <select
                      name="specializationId"
                      value={form.specializationId}
                      onChange={handleChange}
                      className={controlClassName}
                    >
                      <option value="">Выберите специализацию</option>
                      {specializations.map((item) => (
                        <option key={item.id} value={item.id}>
                          {item.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className={labelClassName}>Опыт, лет</label>
                    <input
                      type="number"
                      min="0"
                      name="experienceYears"
                      value={form.experienceYears}
                      onChange={handleChange}
                      className={controlClassName}
                    />
                  </div>
                </div>

                <div>
                  <label className={labelClassName}>Кабинет</label>
                  <input
                    name="cabinetNumber"
                    value={form.cabinetNumber}
                    onChange={handleChange}
                    className={controlClassName}
                  />
                </div>

                <div>
                  <label className={labelClassName}>О себе</label>
                  <Textarea
                    name="bio"
                    value={form.bio}
                    onChange={handleChange}
                    className={textareaClassName}
                  />
                </div>
              </>
            ) : null}

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

            <div className="flex justify-end">
              <Button type="submit" disabled={saving} className="min-w-[220px]">
                {saving ? "Сохраняем..." : "Сохранить изменения"}
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </div>
  );
}
