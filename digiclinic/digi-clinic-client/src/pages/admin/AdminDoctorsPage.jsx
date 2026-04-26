import { useEffect, useMemo, useState } from "react";
import {
  Badge,
  Button,
  Card,
  Input,
  SectionTitle,
  Textarea,
} from "../../components/ui";
import {
  createDoctor,
  deleteDoctor,
  getDoctors,
  updateDoctor,
} from "../../services/doctorService";
import {
  createSpecialization,
  deleteSpecialization,
  getSpecializations,
  updateSpecialization,
} from "../../services/specializationService";
import {
  createClinicService,
  deleteClinicService,
  getClinicServices,
  updateClinicService,
} from "../../services/clinicService";
import { formatCurrency } from "../../utils/clinic";
import {
  controlClassName,
  labelClassName,
  textareaClassName,
} from "../../utils/formStyles";

const doctorInitialState = {
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  password: "",
  specializationId: "",
  experienceYears: 0,
  cabinetNumber: "",
  bio: "",
  isActive: true,
  serviceIds: [],
};

const specializationInitialState = {
  name: "",
  description: "",
};

const clinicServiceInitialState = {
  name: "",
  description: "",
  price: "",
  durationMinutes: 30,
};

export default function AdminDoctorsPage() {
  const [tab, setTab] = useState("doctors");
  const [doctors, setDoctors] = useState([]);
  const [specializations, setSpecializations] = useState([]);
  const [clinicServices, setClinicServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");
  const [doctorForm, setDoctorForm] = useState(doctorInitialState);
  const [specializationForm, setSpecializationForm] = useState(
    specializationInitialState
  );
  const [clinicServiceForm, setClinicServiceForm] = useState(
    clinicServiceInitialState
  );
  const [editingDoctorId, setEditingDoctorId] = useState(null);
  const [editingSpecializationId, setEditingSpecializationId] = useState(null);
  const [editingServiceId, setEditingServiceId] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const loadData = async () => {
    setLoading(true);
    setError("");

    try {
      const [doctorsResponse, specializationsResponse, servicesResponse] =
        await Promise.all([
          getDoctors(),
          getSpecializations(),
          getClinicServices(),
        ]);

      setDoctors(Array.isArray(doctorsResponse.data) ? doctorsResponse.data : []);
      setSpecializations(
        Array.isArray(specializationsResponse.data) ? specializationsResponse.data : []
      );
      setClinicServices(Array.isArray(servicesResponse.data) ? servicesResponse.data : []);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось загрузить справочники администратора."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const filteredDoctors = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();
    if (!normalizedQuery) return doctors;

    return doctors.filter((doctor) => {
      return (
        doctor.fullName?.toLowerCase().includes(normalizedQuery) ||
        doctor.specialization?.toLowerCase().includes(normalizedQuery)
      );
    });
  }, [doctors, query]);

  const toggleService = (serviceId) => {
    setDoctorForm((prev) => {
      const exists = prev.serviceIds.includes(serviceId);
      return {
        ...prev,
        serviceIds: exists
          ? prev.serviceIds.filter((id) => id !== serviceId)
          : [...prev.serviceIds, serviceId],
      };
    });
  };

  const startEditDoctor = (doctor) => {
    setEditingDoctorId(doctor.id);
    setDoctorForm({
      firstName: doctor.firstName || "",
      lastName: doctor.lastName || "",
      email: doctor.email || "",
      phone: doctor.phone || "",
      password: "",
      specializationId: doctor.specializationId || "",
      experienceYears: doctor.experienceYears || 0,
      cabinetNumber: doctor.cabinetNumber || "",
      bio: doctor.bio || "",
      isActive: doctor.isActive ?? true,
      serviceIds: Array.isArray(doctor.serviceIds) ? doctor.serviceIds : [],
    });
    setTab("doctors");
  };

  const resetDoctorForm = () => {
    setEditingDoctorId(null);
    setDoctorForm(doctorInitialState);
  };

  const handleDoctorSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setMessage("");
    setError("");

    try {
      if (!doctorForm.specializationId) {
        throw new Error("Выберите специализацию.");
      }

      if (doctorForm.serviceIds.length === 0) {
        throw new Error("Назначьте врачу хотя бы одну услугу.");
      }

      const payload = {
        firstName: doctorForm.firstName,
        lastName: doctorForm.lastName,
        email: doctorForm.email,
        phone: doctorForm.phone || null,
        password: doctorForm.password,
        specializationId: Number(doctorForm.specializationId),
        experienceYears: Number(doctorForm.experienceYears || 0),
        cabinetNumber: doctorForm.cabinetNumber || null,
        bio: doctorForm.bio || null,
        isActive: doctorForm.isActive,
        serviceIds: doctorForm.serviceIds,
      };

      if (editingDoctorId) {
        await updateDoctor(editingDoctorId, payload);
        setMessage("Данные врача обновлены.");
      } else {
        if (!doctorForm.password) {
          throw new Error("Укажите временный пароль для нового врача.");
        }
        await createDoctor(payload);
        setMessage("Врач успешно создан.");
      }

      resetDoctorForm();
      await loadData();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          err?.message ||
          "Не удалось сохранить врача."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteDoctor = async (doctorId) => {
    const confirmed = window.confirm(
      "Удалить врача? Если у врача уже есть расписание, профиль будет деактивирован."
    );

    if (!confirmed) return;

    try {
      await deleteDoctor(doctorId);
      setMessage("Операция выполнена.");
      await loadData();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось удалить врача."
      );
    }
  };

  const handleSpecializationSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setMessage("");
    setError("");

    try {
      if (editingSpecializationId) {
        await updateSpecialization(editingSpecializationId, specializationForm);
        setMessage("Специализация обновлена.");
      } else {
        await createSpecialization(specializationForm);
        setMessage("Специализация создана.");
      }

      setEditingSpecializationId(null);
      setSpecializationForm(specializationInitialState);
      await loadData();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сохранить специализацию."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const handleServiceSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setMessage("");
    setError("");

    try {
      const payload = {
        name: clinicServiceForm.name,
        description: clinicServiceForm.description,
        price: Number(clinicServiceForm.price || 0),
        durationMinutes: Number(clinicServiceForm.durationMinutes || 0),
      };

      if (editingServiceId) {
        await updateClinicService(editingServiceId, payload);
        setMessage("Услуга обновлена.");
      } else {
        await createClinicService(payload);
        setMessage("Услуга создана.");
      }

      setEditingServiceId(null);
      setClinicServiceForm(clinicServiceInitialState);
      await loadData();
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось сохранить услугу."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const doctorServiceNames = (doctor) => {
    const ids = Array.isArray(doctor.serviceIds) ? doctor.serviceIds : [];
    return clinicServices
      .filter((item) => ids.includes(item.id))
      .map((item) => item.name);
  };

  return (
    <div className="space-y-7">
      <SectionTitle
        title="Управление врачами и услугами"
        subtitle="Поддерживайте актуальные данные по врачам, специализациям и медицинским услугам в одном разделе."
      />

      <div className="flex flex-wrap gap-3">
        {[
          { value: "doctors", label: "Врачи" },
          { value: "specializations", label: "Специализации" },
          { value: "services", label: "Услуги" },
        ].map((item) => (
          <button
            key={item.value}
            onClick={() => setTab(item.value)}
            className={`rounded-[18px] px-6 py-3 text-[16px] font-semibold transition ${
              tab === item.value
                ? "border border-[#D5ECD9] bg-[#EAF7EC] text-[#4F9A61]"
                : "border border-[#E3ECE4] bg-white text-[#6F8278] hover:bg-[#F7FBF7]"
            }`}
          >
            {item.label}
          </button>
        ))}
      </div>

      {tab === "doctors" ? (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1fr_1.15fr]">
          <Card padding="p-6">
            <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {editingDoctorId ? "Редактирование врача" : "Новый врач"}
            </div>

            <form onSubmit={handleDoctorSubmit} className="space-y-5">
              <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div>
                  <label className={labelClassName}>Имя</label>
                  <input
                    value={doctorForm.firstName}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        firstName: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>

                <div>
                  <label className={labelClassName}>Фамилия</label>
                  <input
                    value={doctorForm.lastName}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        lastName: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div>
                  <label className={labelClassName}>Email</label>
                  <input
                    value={doctorForm.email}
                    disabled={Boolean(editingDoctorId)}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        email: event.target.value,
                      }))
                    }
                    className={`${controlClassName} disabled:bg-[#F4F7F4] disabled:text-[#8FA095]`}
                  />
                </div>

                <div>
                  <label className={labelClassName}>
                    {editingDoctorId ? "Временный пароль не меняется" : "Временный пароль"}
                  </label>
                  <input
                    type="password"
                    value={doctorForm.password}
                    disabled={Boolean(editingDoctorId)}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        password: event.target.value,
                      }))
                    }
                    className={`${controlClassName} disabled:bg-[#F4F7F4] disabled:text-[#8FA095]`}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-3">
                <div>
                  <label className={labelClassName}>Телефон</label>
                  <input
                    value={doctorForm.phone}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        phone: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>

                <div>
                  <label className={labelClassName}>Специализация</label>
                  <select
                    value={doctorForm.specializationId}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        specializationId: event.target.value,
                      }))
                    }
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
                    value={doctorForm.experienceYears}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        experienceYears: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-[220px_1fr]">
                <div>
                  <label className={labelClassName}>Кабинет</label>
                  <input
                    value={doctorForm.cabinetNumber}
                    onChange={(event) =>
                      setDoctorForm((prev) => ({
                        ...prev,
                        cabinetNumber: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>

                <div className="flex items-end">
                  <label className="flex items-center gap-3 rounded-[18px] border border-[#DCE9DE] bg-[#FCFEFC] px-4 py-4 text-[15px] font-semibold text-[#355244]">
                    <input
                      type="checkbox"
                      checked={doctorForm.isActive}
                      onChange={(event) =>
                        setDoctorForm((prev) => ({
                          ...prev,
                          isActive: event.target.checked,
                        }))
                      }
                    />
                    Активный профиль
                  </label>
                </div>
              </div>

              <div>
                <label className={labelClassName}>О враче</label>
                <Textarea
                  value={doctorForm.bio}
                  onChange={(event) =>
                    setDoctorForm((prev) => ({
                      ...prev,
                      bio: event.target.value,
                    }))
                  }
                  className={textareaClassName}
                />
              </div>

              <div>
                <div className={labelClassName}>Доступные услуги врача</div>
                <div className="flex flex-wrap gap-3">
                  {clinicServices.map((service) => {
                    const isSelected = doctorForm.serviceIds.includes(service.id);

                    return (
                      <button
                        key={service.id}
                        type="button"
                        onClick={() => toggleService(service.id)}
                        className={`rounded-full border px-4 py-2 text-sm font-semibold transition ${
                          isSelected
                            ? "border-[#D5ECD9] bg-[#EAF7EC] text-[#4F9A61]"
                            : "border-[#E3ECE4] bg-white text-[#6F8278] hover:bg-[#F7FBF7]"
                        }`}
                      >
                        {service.name}
                      </button>
                    );
                  })}
                </div>
              </div>

              <div className="flex flex-wrap justify-end gap-3">
                {editingDoctorId ? (
                  <Button type="button" variant="ghost" onClick={resetDoctorForm}>
                    Отменить редактирование
                  </Button>
                ) : null}
                <Button type="submit" disabled={submitting}>
                  {submitting
                    ? "Сохраняем..."
                    : editingDoctorId
                    ? "Сохранить врача"
                    : "Создать врача"}
                </Button>
              </div>
            </form>
          </Card>

          <Card padding="p-6">
            <div className="mb-4 flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
              <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                Список врачей
              </div>
              <Input
                leftIcon="⌕"
                placeholder="Поиск по имени или специализации"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                className="w-full max-w-[320px]"
              />
            </div>

            <div className="space-y-4">
              {loading ? (
                <div className="text-[#8A94A6]">Загрузка списка врачей...</div>
              ) : filteredDoctors.length === 0 ? (
                <div className="rounded-[24px] border border-dashed border-[#DFE6E0] bg-[#FAFCFB] p-6 text-[#8A94A6]">
                  Врачи не найдены.
                </div>
              ) : (
                filteredDoctors.map((doctor) => (
                  <div
                    key={doctor.id}
                    className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                  >
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                      <div>
                        <div className="text-[20px] font-extrabold text-[#183B2B]">
                          {doctor.fullName}
                        </div>
                        <div className="mt-1 text-[15px] font-medium text-[#6F8278]">
                          {doctor.specialization} · Каб. {doctor.cabinetNumber || "—"} · Опыт {doctor.experienceYears || 0} лет
                        </div>
                        <div className="mt-3 flex flex-wrap gap-2">
                          {doctorServiceNames(doctor).map((name) => (
                            <Badge key={name} variant="gray">
                              {name}
                            </Badge>
                          ))}
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-3">
                        <Badge variant={doctor.isActive ? "green" : "red"}>
                          {doctor.isActive ? "Активен" : "Неактивен"}
                        </Badge>
                        <Button
                          variant="secondary"
                          onClick={() => startEditDoctor(doctor)}
                        >
                          Редактировать
                        </Button>
                        <Button
                          variant="danger"
                          onClick={() => handleDeleteDoctor(doctor.id)}
                        >
                          Удалить
                        </Button>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </Card>
        </div>
      ) : null}

      {tab === "specializations" ? (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-[0.95fr_1.15fr]">
          <Card padding="p-6">
            <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {editingSpecializationId
                ? "Редактирование специализации"
                : "Новая специализация"}
            </div>

            <form onSubmit={handleSpecializationSubmit} className="space-y-5">
              <div>
                <label className={labelClassName}>Название</label>
                <input
                  value={specializationForm.name}
                  onChange={(event) =>
                    setSpecializationForm((prev) => ({
                      ...prev,
                      name: event.target.value,
                    }))
                  }
                  className={controlClassName}
                />
              </div>

              <div>
                <label className={labelClassName}>Описание</label>
                <Textarea
                  value={specializationForm.description}
                  onChange={(event) =>
                    setSpecializationForm((prev) => ({
                      ...prev,
                      description: event.target.value,
                    }))
                  }
                  className={textareaClassName}
                />
              </div>

              <div className="flex flex-wrap justify-end gap-3">
                {editingSpecializationId ? (
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => {
                      setEditingSpecializationId(null);
                      setSpecializationForm(specializationInitialState);
                    }}
                  >
                    Отменить
                  </Button>
                ) : null}
                <Button type="submit" disabled={submitting}>
                  {submitting
                    ? "Сохраняем..."
                    : editingSpecializationId
                    ? "Сохранить"
                    : "Создать"}
                </Button>
              </div>
            </form>
          </Card>

          <Card padding="p-6">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Список специализаций
            </div>

            <div className="mt-5 space-y-4">
              {specializations.map((item) => (
                <div
                  key={item.id}
                  className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div>
                      <div className="text-[18px] font-bold text-[#183B2B]">
                        {item.name}
                      </div>
                      <div className="mt-2 text-[15px] leading-7 font-medium text-[#6F8278]">
                        {item.description || "Описание не заполнено."}
                      </div>
                    </div>

                    <div className="flex gap-3">
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setEditingSpecializationId(item.id);
                          setSpecializationForm({
                            name: item.name || "",
                            description: item.description || "",
                          });
                        }}
                      >
                        Редактировать
                      </Button>
                      <Button
                        variant="danger"
                        onClick={async () => {
                          const confirmed = window.confirm("Удалить специализацию?");
                          if (!confirmed) return;

                          try {
                            await deleteSpecialization(item.id);
                            setMessage("Специализация удалена.");
                            await loadData();
                          } catch (err) {
                            setError(
                              err?.response?.data?.message ||
                                err?.response?.data ||
                                "Не удалось удалить специализацию."
                            );
                          }
                        }}
                      >
                        Удалить
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      ) : null}

      {tab === "services" ? (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-[0.95fr_1.15fr]">
          <Card padding="p-6">
            <div className="mb-4 text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              {editingServiceId ? "Редактирование услуги" : "Новая услуга"}
            </div>

            <form onSubmit={handleServiceSubmit} className="space-y-5">
              <div>
                <label className={labelClassName}>Название</label>
                <input
                  value={clinicServiceForm.name}
                  onChange={(event) =>
                    setClinicServiceForm((prev) => ({
                      ...prev,
                      name: event.target.value,
                    }))
                  }
                  className={controlClassName}
                />
              </div>

              <div>
                <label className={labelClassName}>Описание</label>
                <Textarea
                  value={clinicServiceForm.description}
                  onChange={(event) =>
                    setClinicServiceForm((prev) => ({
                      ...prev,
                      description: event.target.value,
                    }))
                  }
                  className={textareaClassName}
                />
              </div>

              <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div>
                  <label className={labelClassName}>Цена</label>
                  <input
                    type="number"
                    min="0"
                    value={clinicServiceForm.price}
                    onChange={(event) =>
                      setClinicServiceForm((prev) => ({
                        ...prev,
                        price: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>

                <div>
                  <label className={labelClassName}>Длительность, мин</label>
                  <input
                    type="number"
                    min="5"
                    value={clinicServiceForm.durationMinutes}
                    onChange={(event) =>
                      setClinicServiceForm((prev) => ({
                        ...prev,
                        durationMinutes: event.target.value,
                      }))
                    }
                    className={controlClassName}
                  />
                </div>
              </div>

              <div className="flex flex-wrap justify-end gap-3">
                {editingServiceId ? (
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => {
                      setEditingServiceId(null);
                      setClinicServiceForm(clinicServiceInitialState);
                    }}
                  >
                    Отменить
                  </Button>
                ) : null}
                <Button type="submit" disabled={submitting}>
                  {submitting
                    ? "Сохраняем..."
                    : editingServiceId
                    ? "Сохранить"
                    : "Создать"}
                </Button>
              </div>
            </form>
          </Card>

          <Card padding="p-6">
            <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
              Список услуг
            </div>

            <div className="mt-5 space-y-4">
              {clinicServices.map((item) => (
                <div
                  key={item.id}
                  className="rounded-[22px] border border-[#E6EEE7] bg-white p-5"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div>
                      <div className="text-[18px] font-bold text-[#183B2B]">
                        {item.name}
                      </div>
                      <div className="mt-2 text-[15px] leading-7 font-medium text-[#6F8278]">
                        {item.description || "Описание не заполнено."}
                      </div>
                      <div className="mt-3 flex flex-wrap gap-2">
                        <Badge variant="gray">
                          {formatCurrency(item.price)}
                        </Badge>
                        <Badge variant="gray">
                          {item.durationMinutes || item.duration} мин
                        </Badge>
                      </div>
                    </div>

                    <div className="flex gap-3">
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setEditingServiceId(item.id);
                          setClinicServiceForm({
                            name: item.name || "",
                            description: item.description || "",
                            price: item.price || "",
                            durationMinutes:
                              item.durationMinutes || item.duration || 30,
                          });
                        }}
                      >
                        Редактировать
                      </Button>
                      <Button
                        variant="danger"
                        onClick={async () => {
                          const confirmed = window.confirm("Удалить услугу?");
                          if (!confirmed) return;

                          try {
                            await deleteClinicService(item.id);
                            setMessage("Услуга удалена.");
                            await loadData();
                          } catch (err) {
                            setError(
                              err?.response?.data?.message ||
                                err?.response?.data ||
                                "Не удалось удалить услугу."
                            );
                          }
                        }}
                      >
                        Удалить
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
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
    </div>
  );
}
