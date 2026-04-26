import { useEffect, useMemo, useState } from "react";
import {
  Badge,
  Card,
  Input,
  SectionTitle,
} from "../../components/ui";
import { getPatientHistory, getPatients } from "../../services/patientService";
import {
  calculateAge,
  formatDate,
  getAppointmentStatusLabel,
  getAppointmentStatusVariant,
  getGenderLabel,
  getInitials,
} from "../../utils/clinic";

export default function DoctorPatientsPage() {
  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState(null);
  const [history, setHistory] = useState([]);
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(true);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    getPatients()
      .then((response) => {
        const data = Array.isArray(response.data) ? response.data : [];
        setPatients(data);
        if (data.length > 0) {
          setSelectedPatientId(data[0].id);
        }
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить список пациентов."
        );
      })
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!selectedPatientId) return;

    setHistoryLoading(true);
    getPatientHistory(selectedPatientId)
      .then((response) => {
        setHistory(Array.isArray(response.data?.history) ? response.data.history : []);
      })
      .catch((err) => {
        setError(
          err?.response?.data?.message ||
            err?.response?.data ||
            "Не удалось загрузить историю пациента."
        );
      })
      .finally(() => setHistoryLoading(false));
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

  const selectedPatient = useMemo(() => {
    return patients.find((item) => item.id === selectedPatientId) || null;
  }, [patients, selectedPatientId]);

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Пациенты"
        subtitle="Просматривайте карточки пациентов и историю посещений перед приёмом."
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
                Подходящие пациенты не найдены.
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
                  <div className="flex items-center gap-4">
                    <div className="flex h-14 w-14 items-center justify-center rounded-full bg-white text-sm font-extrabold text-[#4F9A61] shadow-[0_8px_20px_rgba(111,193,122,0.08)]">
                      {getInitials(patient.fullName, "П")}
                    </div>
                    <div className="min-w-0">
                      <div className="truncate text-[17px] font-bold text-[#183B2B]">
                        {patient.fullName}
                      </div>
                      <div className="mt-1 text-[14px] font-medium text-[#7A8F84]">
                        {patient.email}
                      </div>
                    </div>
                  </div>
                </button>
              ))
            )}
          </div>
        </Card>

        <Card padding="p-6">
          {selectedPatient ? (
            <>
              <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div>
                  <div className="text-[30px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                    {selectedPatient.fullName}
                  </div>
                  <div className="mt-2 text-[16px] font-medium text-[#6F8278]">
                    {selectedPatient.email}
                  </div>
                </div>

                <Badge>{getGenderLabel(selectedPatient.gender)}</Badge>
              </div>

              <div className="mt-6 grid grid-cols-1 gap-4 md:grid-cols-3">
                <div className="rounded-[20px] border border-[#E6EEE7] bg-[#FCFEFC] p-4">
                  <div className="text-sm font-semibold text-[#8FA095]">Возраст</div>
                  <div className="mt-2 text-[20px] font-extrabold text-[#183B2B]">
                    {calculateAge(selectedPatient.birthDate)}
                  </div>
                </div>
                <div className="rounded-[20px] border border-[#E6EEE7] bg-[#FCFEFC] p-4">
                  <div className="text-sm font-semibold text-[#8FA095]">Телефон</div>
                  <div className="mt-2 text-[20px] font-extrabold text-[#183B2B]">
                    {selectedPatient.phone || "Не указан"}
                  </div>
                </div>
                <div className="rounded-[20px] border border-[#E6EEE7] bg-[#FCFEFC] p-4">
                  <div className="text-sm font-semibold text-[#8FA095]">Последний визит</div>
                  <div className="mt-2 text-[20px] font-extrabold text-[#183B2B]">
                    {formatDate(selectedPatient.lastVisit)}
                  </div>
                </div>
              </div>

              <div className="mt-6">
                <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                  История посещений
                </div>

                <div className="mt-4 space-y-4">
                  {historyLoading ? (
                    <div className="text-[#8A94A6]">Загрузка истории...</div>
                  ) : history.length === 0 ? (
                    <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                      История посещений пока отсутствует.
                    </div>
                  ) : (
                    history.map((item) => (
                      <div
                        key={item.id}
                        className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                      >
                        <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                          <div>
                            <div className="text-[17px] font-bold text-[#183B2B]">
                              {item.service}
                            </div>
                            <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                              Врач: {item.doctor}
                            </div>
                            <div className="mt-2 text-[15px] font-semibold text-[#355244]">
                              {formatDate(item.startTime)}
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
                    ))
                  )}
                </div>
              </div>
            </>
          ) : (
            <div className="text-[#8A94A6]">Выберите пациента из списка.</div>
          )}
        </Card>
      </div>

      {error ? (
        <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
          {error}
        </div>
      ) : null}
    </div>
  );
}
