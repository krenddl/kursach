import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { loginUser } from "../../services/authService";
import { Button, Card, Input } from "../../components/ui";

function redirectByRole(role, navigate) {
  if (role === "Admin") navigate("/admin/dashboard");
  else if (role === "Doctor") navigate("/doctor/dashboard");
  else navigate("/patient/dashboard");
}

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [form, setForm] = useState({
    email: "",
    password: "",
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
    setLoading(true);

    try {
      const response = await loginUser(form);
      const token = response.data.token || response.data.accessToken || response.data.jwt;

      if (!token) {
        throw new Error("Не удалось получить токен.");
      }

      login(token);
      redirectByRole(response.data.role, navigate);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.response?.data ||
          "Не удалось войти в систему."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#F4F8F4] px-6 py-10">
      <div className="w-full max-w-[520px]">
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
              Вход
            </div>
          </div>

          <div className="mb-8">
            <h1 className="text-[30px] leading-[1.05] font-extrabold tracking-[-0.05em] text-[#183B2B]">
              Добро пожаловать
            </h1>
            <p className="mt-3 text-[18px] leading-8 font-medium text-[#73877C]">
              Войдите в систему, чтобы управлять записями, расписанием и медицинскими данными в одном аккуратном интерфейсе.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="mb-2 block text-[15px] font-bold text-[#2B4638]">
                Электронная почта
              </label>
              <Input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                placeholder="Например, admin1@lotos.kz"
              />
            </div>

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

            {error ? (
              <div className="rounded-[18px] border border-[#F1D4D4] bg-[#FFF4F4] px-4 py-3 text-sm font-medium text-[#D87474]">
                {String(error)}
              </div>
            ) : null}

            <Button type="submit" className="w-full py-4 text-[16px] font-bold" disabled={loading}>
              {loading ? "Входим..." : "Войти в систему"}
            </Button>
          </form>

          <div className="mt-6 text-center text-[16px] font-medium text-[#7E8F84]">
            Нет аккаунта?{" "}
            <Link to="/register" className="font-extrabold text-[#4F9A61] hover:text-[#3E8B53]">
              Создать профиль
            </Link>
          </div>

          <div className="mt-8 rounded-[24px] border border-[#E3ECE4] bg-[#F8FBF8] p-5">
            <div className="mb-3 text-[15px] font-bold text-[#2B4638]">
              Тестовые аккаунты
            </div>
            <div className="space-y-1 text-[15px] font-medium text-[#72857B]">
              <div>admin1@lotos.kz / admin123</div>
              <div>doctor1@lotos.kz / doctor123</div>
              <div>patient1@lotos.kz / patient123</div>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}
