import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  Badge,
  Button,
  Card,
  Input,
  SectionTitle,
} from "../../components/ui";
import { getDoctors } from "../../services/doctorService";
import { getSpecializations } from "../../services/specializationService";
import { getInitials } from "../../utils/clinic";

function normalizeDoctor(item) {
  if (!item) return null;

  return {
    id: item.id,
    fullName:
      item.fullName ||
      `${item.firstName || ""} ${item.lastName || ""}`.trim() ||
      "Врач клиники",
    specialization:
      item.specializationName ||
      item.specialization ||
      "Специалист",
    specializationId: item.specializationId || "",
    experienceYears: item.experienceYears ?? item.experience ?? 0,
    cabinetNumber: item.cabinetNumber || item.cabinet || "—",
    availableSlotsCount: item.availableSlotsCount ?? 0,
    bio: item.bio || "",
    isActive: item.isActive ?? true,
    serviceIds: Array.isArray(item.serviceIds) ? item.serviceIds : [],
  };
}

function DoctorCard({ doctor, referral, onBook }) {
  return (
    <Card
      padding="p-5"
      className="transition hover:shadow-[0_14px_34px_rgba(111,193,122,0.09)]"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex min-w-0 items-center gap-4">
          <div className="flex h-[84px] w-[84px] shrink-0 items-center justify-center rounded-[24px] bg-[#EAF7EC] text-[22px] font-bold text-[#4F9A61]">
            {getInitials(doctor.fullName, "В")}
          </div>

          <div className="min-w-0">
            <div className="truncate text-[24px] leading-tight font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {doctor.fullName}
            </div>
            <div className="mt-3 flex flex-wrap gap-2">
              <Badge>{doctor.specialization}</Badge>
              {referral ? <Badge variant="green">Подходит по направлению</Badge> : null}
            </div>
          </div>
        </div>

        <div className="shrink-0 rounded-full border border-[#E2EAE3] bg-[#F4F7F4] px-3 py-1.5 text-[15px] font-bold text-[#7A8F84]">
          Каб. {doctor.cabinetNumber}
        </div>
      </div>

      <div className="mt-5 space-y-2">
        <div className="text-[15px] font-medium text-[#6F8278]">
          Опыт: {doctor.experienceYears} лет
        </div>
        <div className="text-[15px] font-medium text-[#6F8278]">
          Свободных слотов: {doctor.availableSlotsCount}
        </div>
      </div>

      {doctor.bio ? (
        <div className="mt-4 line-clamp-3 text-[14px] leading-6 text-[#8DA094]">
          {doctor.bio}
        </div>
      ) : null}

      <div className="mt-6 flex items-center justify-between gap-4">
        <div className="text-[14px] text-[#8DA094]">Клиника «Лотос»</div>
        <Button onClick={onBook}>Выбрать время</Button>
      </div>
    </Card>
  );
}

export default function PatientDoctorsPage() {
  const navigate = useNavigate();
  const { state } = useLocation();
  const referral = state?.referral || null;

  const [doctors, setDoctors] = useState([]);
  const [specializations, setSpecializations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");
  const [specializationId, setSpecializationId] = useState("");

  useEffect(() => {
    Promise.all([getDoctors(), getSpecializations()])
      .then(([doctorsResponse, specializationsResponse]) => {
        const doctorsData = Array.isArray(doctorsResponse.data)
          ? doctorsResponse.data
          : [];
        const specializationsData = Array.isArray(specializationsResponse.data)
          ? specializationsResponse.data
          : [];

        setDoctors(doctorsData.map(normalizeDoctor).filter(Boolean));
        setSpecializations(specializationsData);
      })
      .catch((error) => {
        console.error("Ошибка загрузки врачей:", error);
      })
      .finally(() => setLoading(false));
  }, []);

  const filteredDoctors = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return doctors.filter((doctor) => {
      if (!doctor.isActive) return false;

      const matchesQuery =
        !normalizedQuery ||
        doctor.fullName.toLowerCase().includes(normalizedQuery) ||
        doctor.specialization.toLowerCase().includes(normalizedQuery);

      const matchesSpecialization =
        !specializationId ||
        String(doctor.specializationId) === String(specializationId);

      const matchesReferral =
        !referral?.serviceId ||
        doctor.serviceIds.includes(Number(referral.serviceId));

      return matchesQuery && matchesSpecialization && matchesReferral;
    });
  }, [doctors, query, referral?.serviceId, specializationId]);

  return (
    <div className="space-y-7">
      <SectionTitle
        title={referral ? "Выбор врача по направлению" : "Выбор врача"}
        subtitle={
          referral
            ? `Подберите врача для услуги "${referral.serviceName}".`
            : "Найдите специалиста по имени или специализации и перейдите к записи на приём."
        }
      />

      {referral ? (
        <Card padding="p-5">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="text-[18px] font-bold text-[#183B2B]">
                Активное направление
              </div>
              <div className="mt-2 text-[15px] font-medium text-[#6F8278]">
                Услуга: {referral.serviceName}
              </div>
              {referral.comment ? (
                <div className="mt-2 text-[14px] leading-6 text-[#8A94A6]">
                  {referral.comment}
                </div>
              ) : null}
            </div>
            <Badge variant="green">Внутри клиники</Badge>
          </div>
        </Card>
      ) : null}

      <Card padding="p-5">
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-[1fr_260px]">
          <Input
            leftIcon="⌕"
            placeholder="Поиск по имени или специализации..."
            value={query}
            onChange={(event) => setQuery(event.target.value)}
          />

          <select
            value={specializationId}
            onChange={(event) => setSpecializationId(event.target.value)}
            className="h-[54px] rounded-[18px] border border-[#DCE9DE] bg-[#FCFEFC] px-4 text-[16px] font-medium text-[#2E463A] outline-none transition focus:border-[#6FC17A] focus:ring-4 focus:ring-[#6FC17A]/10"
          >
            <option value="">Все специализации</option>
            {specializations.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
        </div>
      </Card>

      {loading ? (
        <div className="text-[#8A94A6]">Загрузка списка врачей...</div>
      ) : filteredDoctors.length === 0 ? (
        <Card padding="p-6">
          <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
            Подходящие врачи не найдены. Попробуйте изменить фильтр или поисковый запрос.
          </div>
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
          {filteredDoctors.map((doctor) => (
            <DoctorCard
              key={doctor.id}
              doctor={doctor}
              referral={referral}
              onBook={() =>
                navigate("/patient/book", {
                  state: { doctor, referral },
                })
              }
            />
          ))}
        </div>
      )}
    </div>
  );
}
