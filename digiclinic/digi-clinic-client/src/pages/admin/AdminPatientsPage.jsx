import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Input,
  SectionTitle,
} from "../../components/ui";
import {
  getPatientById,
  getPatients,
  updatePatient,
} from "../../services/patientService";
import {
  calculateAge,
  formatDate,
  getGenderLabel,
  toDateInputValue,
} from "../../utils/clinic";
import { controlClassName, labelClassName } from "../../utils/formStyles";

function buildPatientForm(patient) {
  return {
    firstName: patient?.firstName || "",
    lastName: patient?.lastName || "",
    phone: patient?.phone || "",
    birthDate: toDateInputValue(patient?.birthDate),
    gender: patient?.gender || "",
    address: patient?.address || "",
    emergencyContact: patient?.emergencyContact || "",
  };
}

export default function AdminPatientsPage() {
  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState(null);
  const [selectedPatient, setSelectedPatient] = useState(null);
  const [form, setForm] = useState(buildPatientForm(null));
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const loadPatients = async () => {
    setLoading(true);

    try {
      const response = await getPatients();
      const data = Array.isArray(response.data) ? response.data : [];
      setPatients(data);

      if (!selectedPatientId && data.length > 0) {
        setSelectedPatientId(data[0].id);
      }
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось загрузить пациентов."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPatients();
  }, []);

  useEffect(() => {
    if (!selectedPatientId) return;

    getPatientById(selectedPatientId)
      .then((response) => {
        setSelectedPatient(response.data);
        setForm(buildPatientForm(response.data));
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить карточку пациента."
        );
      });
  }, [selectedPatientId]);

  const filteredPatients = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();
    if (!normalizedQuery) return patients;

    return patients.filter((patient) => {
      return (
        patient.fullName?.toLowerCase().includes(normalizedQuery) ||
        patient.email?.toLowerCase().includes(normalizedQuery)
      );
    });
  }, [patients, query]);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!selectedPatientId) return;

    setSaving(true);
    setMessage("");
    setError("");

    try {
      await updatePatient(selectedPatientId, {
        firstName: form.firstName,
        lastName: form.lastName,
        phone: form.phone || null,
        birthDate: form.birthDate ? `${form.birthDate}T00:00:00Z` : null,
        gender: form.gender || null,
        address: form.address || null,
        emergencyContact: form.emergencyContact || null,
      });

      setMessage("Данные пациента обновлены.");
      await loadPatients();
      const refreshed = await getPatientById(selectedPatientId);
      setSelectedPatient(refreshed.data);
      setForm(buildPatientForm(refreshed.data));
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сохранить данные пациента."
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Пациенты"
        subtitle="Просматривайте карточки пациентов и редактируйте контактные данные при необходимости."
      />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[0.95fr_1.25fr]">
        <Card padding="p-6">
          <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
            Список пациентов
          </div>

          <Input
            leftIcon="⌕"
            placeholder="Поиск по имени или email"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />

          <div className="mt-5 space-y-3">
            {loading ? (
              <div className="text-[#8A94A6]">Загрузка пациентов...</div>
            ) : filteredPatients.length === 0 ? (
              <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                Пациенты не найдены.
              </div>
            ) : (
              filteredPatients.map((patient) => (
                <button
                  key={patient.id}
                  onClick={() => setSelectedPatientId(patient.id)}
                  className={`w-full rounded-[22px] border p-4 text-left transition ${
                    selectedPatientId === patient.id
                      ? "border-[#D5ECD9] bg-[#EAF7EC]"
                      : "border-[#E6EEE7] bg-white hover:bg-[#F7FBF7]"
                  }`}
                >
                  <div className="text-[17px] font-bold text-[#183B2B]">
                    {patient.fullName}
                  </div>
                  <div className="mt-1 text-[14px] font-medium text-[#7A8F84]">
                    {patient.email}
                  </div>
                  <div className="mt-2 text-[14px] font-medium text-[#8FA095]">
                    Последний визит: {formatDate(patient.lastVisit)}
                  </div>
                </button>
              ))
            )}
          </div>
        </Card>

        <Card padding="p-6">
          {selectedPatient ? (
            <>
              <div className="mb-6 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div>
                  <div className="text-[30px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                    {selectedPatient.firstName} {selectedPatient.lastName}
                  </div>
                  <div className="mt-2 text-[16px] font-medium text-[#6F8278]">
                    {selectedPatient.email}
                  </div>
                </div>

                <div className="rounded-[20px] border border-[#E6EEE7] bg-[#FCFEFC] px-4 py-3 text-sm font-semibold text-[#4F9A61]">
                  Возраст: {calculateAge(selectedPatient.birthDate)}
                </div>
              </div>

              <form onSubmit={handleSubmit} className="space-y-5">
                <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                  <div>
                    <label className={labelClassName}>Имя</label>
                    <input
                      value={form.firstName}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, firstName: event.target.value }))
                      }
                      className={controlClassName}
                    />
                  </div>
                  <div>
                    <label className={labelClassName}>Фамилия</label>
                    <input
                      value={form.lastName}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, lastName: event.target.value }))
                      }
                      className={controlClassName}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                  <div>
                    <label className={labelClassName}>Телефон</label>
                    <input
                      value={form.phone}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, phone: event.target.value }))
                      }
                      className={controlClassName}
                    />
                  </div>
                  <div>
                    <label className={labelClassName}>Дата рождения</label>
                    <input
                      type="date"
                      value={form.birthDate}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, birthDate: event.target.value }))
                      }
                      className={controlClassName}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                  <div>
                    <label className={labelClassName}>Пол</label>
                    <select
                      value={form.gender}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, gender: event.target.value }))
                      }
                      className={controlClassName}
                    >
                      <option value="">Не указан</option>
                      <option value="Male">Мужской</option>
                      <option value="Female">Женский</option>
                    </select>
                  </div>

                  <div>
                    <label className={labelClassName}>Экстренный контакт</label>
                    <input
                      value={form.emergencyContact}
                      onChange={(event) =>
                        setForm((prev) => ({
                          ...prev,
                          emergencyContact: event.target.value,
                        }))
                      }
                      className={controlClassName}
                    />
                  </div>
                </div>

                <div>
                  <label className={labelClassName}>Адрес</label>
                  <input
                    value={form.address}
                    onChange={(event) =>
                      setForm((prev) => ({ ...prev, address: event.target.value }))
                    }
                    className={controlClassName}
                  />
                </div>

                <div className="rounded-[22px] border border-[#E6EEE7] bg-[#F9FCF9] p-4">
                  <div className="text-sm font-semibold text-[#8FA095]">Текущий статус</div>
                  <div className="mt-2 text-[16px] font-semibold text-[#355244]">
                    Пол: {getGenderLabel(selectedPatient.gender)} · Активность профиля: {selectedPatient.isActive ? "активен" : "неактивен"}
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button type="submit" disabled={saving} className="min-w-[220px]">
                    {saving ? "Сохраняем..." : "Сохранить изменения"}
                  </Button>
                </div>
              </form>
            </>
          ) : (
            <div className="text-[#8A94A6]">Выберите пациента из списка.</div>
          )}
        </Card>
      </div>

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
    </div>
  );
}
