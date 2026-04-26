using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DigiClinicApi.Telegram
{
    public class TelegramBotService
    {
        private readonly ContextDb _context;
        private readonly IAppointmentService _appointmentService;
        private readonly TelegramApiClient _telegram;

        public TelegramBotService(
            ContextDb context,
            IAppointmentService appointmentService,
            TelegramApiClient telegram)
        {
            _context = context;
            _appointmentService = appointmentService;
            _telegram = telegram;
        }

        public async Task HandleUpdate(TelegramUpdate update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chatId = message?.Chat?.Id;
            var text = message?.Text?.Trim();

            if (chatId == null || string.IsNullOrWhiteSpace(text))
                return;

            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].Split('@')[0].ToLowerInvariant();

            if (command == "/start")
            {
                if (parts.Length > 1)
                    await Link(chatId.Value, parts[1], cancellationToken);
                else
                    await SendStart(chatId.Value, cancellationToken);

                return;
            }

            if (command == "/help" || command == "помощь")
            {
                await SendHelp(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/link")
            {
                if (parts.Length < 2)
                    await _telegram.SendMessage(chatId.Value, "Отправьте код так: /link 123456", cancellationToken);
                else
                    await Link(chatId.Value, parts[1], cancellationToken);

                return;
            }

            if (command == "/me")
            {
                await SendMe(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/appointments")
            {
                await SendAppointments(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/doctors")
            {
                await SendDoctors(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/services")
            {
                await SendServices(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/slots")
            {
                if (parts.Length < 2 || !int.TryParse(parts[1], out var doctorId))
                    await _telegram.SendMessage(chatId.Value, "Отправьте ID врача так: /slots 3", cancellationToken);
                else
                    await SendSlots(chatId.Value, doctorId, cancellationToken);

                return;
            }

            if (command == "/book")
            {
                if (parts.Length < 3 ||
                    !int.TryParse(parts[1], out var slotId) ||
                    !int.TryParse(parts[2], out var serviceId))
                {
                    await _telegram.SendMessage(chatId.Value, "Формат записи: /book SLOT_ID SERVICE_ID", cancellationToken);
                }
                else
                {
                    await Book(chatId.Value, slotId, serviceId, null, cancellationToken);
                }

                return;
            }

            if (command == "/referrals")
            {
                await SendReferrals(chatId.Value, cancellationToken);
                return;
            }

            if (command == "/bookref")
            {
                if (parts.Length < 3 ||
                    !int.TryParse(parts[1], out var slotId) ||
                    !int.TryParse(parts[2], out var referralId))
                {
                    await _telegram.SendMessage(chatId.Value, "Формат записи по направлению: /bookref SLOT_ID REFERRAL_ID", cancellationToken);
                }
                else
                {
                    await BookByReferral(chatId.Value, slotId, referralId, cancellationToken);
                }

                return;
            }

            if (command == "/cancel")
            {
                if (parts.Length < 2 || !int.TryParse(parts[1], out var appointmentId))
                    await _telegram.SendMessage(chatId.Value, "Формат отмены: /cancel APPOINTMENT_ID", cancellationToken);
                else
                    await Cancel(chatId.Value, appointmentId, cancellationToken);

                return;
            }

            await SendHelp(chatId.Value, cancellationToken);
        }

        private async Task SendStart(long chatId, CancellationToken cancellationToken)
        {
            var user = await GetLinkedPatient(chatId, cancellationToken);

            if (user != null)
            {
                await _telegram.SendMessage(
                    chatId,
                    $"Вы уже привязаны как {user.FirstName} {user.LastName}.\n\n{BuildHelpText()}",
                    cancellationToken);
                return;
            }

            await _telegram.SendMessage(
                chatId,
                "Привет! Это бот клиники Лотос.\n\nЧтобы привязать профиль, откройте профиль в веб-приложении, получите Telegram-код и отправьте его сюда командой:\n/link 123456",
                cancellationToken);
        }

        private async Task SendHelp(long chatId, CancellationToken cancellationToken)
        {
            await _telegram.SendMessage(chatId, BuildHelpText(), cancellationToken);
        }

        private static string BuildHelpText()
        {
            return string.Join('\n', new[]
            {
                "Команды DigiClinic:",
                "/me - мой профиль",
                "/appointments - мои записи",
                "/doctors - список врачей",
                "/services - список услуг",
                "/slots DOCTOR_ID - свободные слоты врача",
                "/book SLOT_ID SERVICE_ID - записаться",
                "/referrals - мои направления",
                "/bookref SLOT_ID REFERRAL_ID - записаться по направлению",
                "/cancel APPOINTMENT_ID - отменить будущую запись"
            });
        }

        private async Task Link(long chatId, string code, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var user = await _context.Users
                .Include(x => x.Role)
                .Include(x => x.PatientProfile)
                .FirstOrDefaultAsync(
                    x => x.TelegramLinkCode == code &&
                         x.TelegramLinkCodeExpiresAt != null &&
                         x.TelegramLinkCodeExpiresAt >= now,
                    cancellationToken);

            if (user == null)
            {
                await _telegram.SendMessage(chatId, "Код не найден или уже истёк. Получите новый код в профиле.", cancellationToken);
                return;
            }

            if (user.Role.Name != "Patient" || user.PatientProfile == null)
            {
                await _telegram.SendMessage(chatId, "Telegram-бот сейчас доступен только для пациентов.", cancellationToken);
                return;
            }

            var existingLinks = await _context.Users
                .Where(x => x.TelegramChatId == chatId && x.Id != user.Id)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingLinks)
            {
                existing.TelegramChatId = null;
            }

            user.TelegramChatId = chatId;
            user.TelegramLinkCode = null;
            user.TelegramLinkCodeExpiresAt = null;
            user.UpdatedAt = now;

            await _context.SaveChangesAsync(cancellationToken);
            await _telegram.SendMessage(chatId, $"Профиль привязан: {user.FirstName} {user.LastName}.\n\n{BuildHelpText()}", cancellationToken);
        }

        private async Task SendMe(long chatId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            await _telegram.SendMessage(
                chatId,
                $"Профиль: {user.FirstName} {user.LastName}\nEmail: {user.Email}\nТелефон: {user.Phone ?? "не указан"}",
                cancellationToken);
        }

        private async Task SendAppointments(long chatId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user?.PatientProfile == null) return;

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.PatientProfileId == user.PatientProfile.Id)
                .OrderByDescending(x => x.TimeSlot.StartTime)
                .Take(8)
                .ToListAsync(cancellationToken);

            if (appointments.Count == 0)
            {
                await _telegram.SendMessage(chatId, "У вас пока нет записей.", cancellationToken);
                return;
            }

            var lines = appointments.Select(FormatAppointment);
            await _telegram.SendMessage(chatId, "Ваши последние записи:\n\n" + string.Join("\n\n", lines), cancellationToken);
        }

        private async Task SendDoctors(long chatId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            var doctors = await _context.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Specialization)
                .Where(x => x.User.IsActive)
                .OrderBy(x => x.User.LastName)
                .Take(12)
                .ToListAsync(cancellationToken);

            if (doctors.Count == 0)
            {
                await _telegram.SendMessage(chatId, "Активные врачи пока не найдены.", cancellationToken);
                return;
            }

            var lines = doctors.Select(x =>
                $"{x.Id}. {x.User.FirstName} {x.User.LastName} - {x.Specialization.Name}");

            await _telegram.SendMessage(
                chatId,
                "Врачи:\n" + string.Join('\n', lines) + "\n\nСвободные слоты: /slots DOCTOR_ID",
                cancellationToken);
        }

        private async Task SendServices(long chatId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            var services = await _context.Services
                .OrderBy(x => x.Name)
                .Take(20)
                .ToListAsync(cancellationToken);

            if (services.Count == 0)
            {
                await _telegram.SendMessage(chatId, "Услуги пока не найдены.", cancellationToken);
                return;
            }

            var lines = services.Select(x =>
                $"{x.Id}. {x.Name} - {x.Price.ToString("0", CultureInfo.InvariantCulture)} ₽");

            await _telegram.SendMessage(
                chatId,
                "Услуги:\n" + string.Join('\n', lines) + "\n\nЗапись: /book SLOT_ID SERVICE_ID",
                cancellationToken);
        }

        private async Task SendSlots(long chatId, int doctorId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            var slots = await _context.TimeSlots
                .Include(x => x.DoctorProfile)
                    .ThenInclude(x => x.User)
                .Where(x =>
                    x.DoctorProfileId == doctorId &&
                    x.Status == TimeSlotStatus.Available &&
                    x.StartTime > DateTime.UtcNow)
                .OrderBy(x => x.StartTime)
                .Take(10)
                .ToListAsync(cancellationToken);

            if (slots.Count == 0)
            {
                await _telegram.SendMessage(chatId, "У этого врача пока нет свободных будущих слотов.", cancellationToken);
                return;
            }

            var lines = slots.Select(x =>
                $"{x.Id}. {FormatDateTime(x.StartTime)} - {FormatTime(x.EndTime)}");

            await _telegram.SendMessage(
                chatId,
                "Свободные слоты:\n" + string.Join('\n', lines) + "\n\nЗапись: /book SLOT_ID SERVICE_ID",
                cancellationToken);
        }

        private async Task Book(long chatId, int slotId, int serviceId, int? referralId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            var result = await _appointmentService.Create(
                new CreateAppointmentRequest
                {
                    TimeSlotId = slotId,
                    ServiceId = serviceId,
                    ReferralId = referralId
                },
                user.Id);

            await _telegram.SendMessage(chatId, GetResultMessage(result), cancellationToken);
        }

        private async Task SendReferrals(long chatId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user?.PatientProfile == null) return;

            var referrals = await _context.Referrals
                .Include(x => x.Service)
                .Include(x => x.CreatedByDoctorProfile)
                    .ThenInclude(x => x.User)
                .Where(x => x.PatientProfileId == user.PatientProfile.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Take(8)
                .ToListAsync(cancellationToken);

            if (referrals.Count == 0)
            {
                await _telegram.SendMessage(chatId, "Направлений пока нет.", cancellationToken);
                return;
            }

            var lines = referrals.Select(x =>
                $"{x.Id}. {x.Service.Name}\nВрач: {x.CreatedByDoctorProfile.User.FirstName} {x.CreatedByDoctorProfile.User.LastName}\nТип: {FormatReferralType(x.Type)}\nСтатус: {FormatReferralStatus(x.Status)}");

            await _telegram.SendMessage(
                chatId,
                "Ваши направления:\n\n" + string.Join("\n\n", lines) + "\n\nДля внутреннего направления: /bookref SLOT_ID REFERRAL_ID",
                cancellationToken);
        }

        private async Task BookByReferral(long chatId, int slotId, int referralId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user?.PatientProfile == null) return;

            var referral = await _context.Referrals
                .FirstOrDefaultAsync(
                    x => x.Id == referralId && x.PatientProfileId == user.PatientProfile.Id,
                    cancellationToken);

            if (referral == null)
            {
                await _telegram.SendMessage(chatId, "Направление не найдено.", cancellationToken);
                return;
            }

            await Book(chatId, slotId, referral.ServiceId, referral.Id, cancellationToken);
        }

        private async Task Cancel(long chatId, int appointmentId, CancellationToken cancellationToken)
        {
            var user = await RequireLinkedPatient(chatId, cancellationToken);
            if (user == null) return;

            var result = await _appointmentService.Cancel(appointmentId, user.Id);
            await _telegram.SendMessage(chatId, GetResultMessage(result), cancellationToken);
        }

        private async Task<User?> RequireLinkedPatient(long chatId, CancellationToken cancellationToken)
        {
            var user = await GetLinkedPatient(chatId, cancellationToken);

            if (user != null)
                return user;

            await _telegram.SendMessage(
                chatId,
                "Профиль не привязан. Получите Telegram-код в веб-профиле и отправьте: /link 123456",
                cancellationToken);

            return null;
        }

        private async Task<User?> GetLinkedPatient(long chatId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.PatientProfile)
                .FirstOrDefaultAsync(
                    x => x.TelegramChatId == chatId &&
                         x.IsActive &&
                         x.Role.Name == "Patient",
                    cancellationToken);
        }

        private static string FormatAppointment(Appointment appointment)
        {
            return $"#{appointment.Id} {FormatDateTime(appointment.TimeSlot.StartTime)}\nВрач: {appointment.TimeSlot.DoctorProfile.User.FirstName} {appointment.TimeSlot.DoctorProfile.User.LastName}\nУслуга: {appointment.Service.Name}\nСтатус: {FormatAppointmentStatus(appointment.Status)}";
        }

        private static string FormatDateTime(DateTime value)
        {
            return value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        private static string FormatTime(DateTime value)
        {
            return value.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        private static string FormatAppointmentStatus(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => "запланирована",
                AppointmentStatus.Completed => "завершена",
                AppointmentStatus.Cancelled => "отменена",
                AppointmentStatus.NoShow => "неявка",
                _ => status.ToString()
            };
        }

        private static string FormatReferralStatus(ReferralStatus status)
        {
            return status switch
            {
                ReferralStatus.Pending => "ожидает",
                ReferralStatus.Booked => "записано",
                ReferralStatus.Completed => "завершено",
                ReferralStatus.Cancelled => "отменено",
                _ => status.ToString()
            };
        }

        private static string FormatReferralType(ReferralType type)
        {
            return type == ReferralType.Internal ? "внутри клиники" : "внешнее";
        }

        private static string GetResultMessage(IActionResult result)
        {
            if (result is ObjectResult objectResult)
            {
                return objectResult.Value?.ToString() ?? "Готово.";
            }

            if (result is StatusCodeResult statusCodeResult)
            {
                return statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300
                    ? "Готово."
                    : "Не удалось выполнить действие.";
            }

            return "Готово.";
        }
    }
}
