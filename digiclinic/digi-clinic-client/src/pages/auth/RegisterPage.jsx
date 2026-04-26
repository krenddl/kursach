import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { registerUser } from "../../services/authService";
import { Button, Card, Input } from "../../components/ui";

export default function RegisterPage() {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    password: "",
    confirmPassword: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");

    if (form.password !== form.confirmPassword) {
      setError("Пароли не совпадают.");
      return;
    }

    setLoading(true);

    try {
      await registerUser({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        phone: form.phone,
        password: form.password,
      });

      navigate("/login");
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось зарегистрироваться."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#F4F8F4] px-6 py-10">
      <div className="w-full max-w-[560px]">
        <Card className="rounded-[34px]" padding="p-9">
          <div className="mb-8 flex items-start justify-between gap-4">
            <div className="flex items-center gap-4">
              <div className="flex h-14 w-14 items-center justify-center rounded-[18px] bg-[#6FC17A] text-xl font-extrabold text-white shadow-[0_10px_24px_rgba(111,193,122,0.22)]">
                +
              </div>
              <div>
                <div className="text-[24px] font-extrabold tracking-[-0.04em] text-[#183B2B]">
                  Лотос
                </div>
                <div className="text-[15px] font-medium text-[#7E8F84]">
                  Цифровая клиника нового поколения
                </div>
              </div>
            </div>

            <div className="rounded-full border border-[#D8E8DA] bg-[#EAF7EC] px-4 py-2 text-sm font-bold text-[#4F9A61]">
              Регистрация
            </div>
          </div>

          <div className="mb-8">
            <h1 className="text-[30px] leading-[1.05] font-extrabold tracking-[-0.05em] text-[#183B2B]">
              Создать профиль пациента
            </h1>
            <p className="mt-3 text-[18px] leading-8 font-medium text-[#73877C]">
              Заполните базовые данные, чтобы получить доступ к онлайн-записи, истории посещений и личному кабинету.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
              <div>
                <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                  Имя
                </label>
                <Input
                  name="firstName"
                  value={form.firstName}
                  onChange={handleChange}
                  placeholder="Введите имя"
                />
              </div>

              <div>
                <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                  Фамилия
                </label>
                <Input
                  name="lastName"
                  value={form.lastName}
                  onChange={handleChange}
                  placeholder="Введите фамилию"
                />
              </div>
            </div>

            <div>
              <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                Электронная почта
              </label>
              <Input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                placeholder="Например, patient@lotos.kz"
              />
            </div>

            <div>
              <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                Телефон
              </label>
              <Input
                name="phone"
                value={form.phone}
                onChange={handleChange}
                placeholder="+7 777 123 45 67"
              />
            </div>

            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
              <div>
                <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                  Пароль
                </label>
                <Input
                  type="password"
                  name="password"
                  value={form.password}
                  onChange={handleChange}
                  placeholder="Введите пароль"
                />
              </div>

              <div>
                <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                  Повторите пароль
                </label>
                <Input
                  type="password"
                  name="confirmPassword"
                  value={form.confirmPassword}
                  onChange={handleChange}
                  placeholder="Повторите пароль"
                />
              </div>
            </div>

            {error ? (
              <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
                {String(error)}
              </div>
            ) : null}

            <Button type="submit" className="w-full py-4 text-[16px] font-bold" disabled={loading}>
              {loading ? "Создаём профиль..." : "Создать профиль"}
            </Button>
          </form>

          <div className="mt-6 text-center text-[16px] font-medium text-[#7E8F84]">
            Уже есть аккаунт?{" "}
            <Link to="/login" className="font-extrabold text-[#4F9A61] hover:text-[#3E8B53]">
              Войти
            </Link>
          </div>
        </Card>
      </div>
    </div>
  );
}
