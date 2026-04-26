using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Hubs;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ContextDb _context;
        private readonly IHubContext<AppointmentHub> _appointmentHub;

        public AppointmentService(ContextDb context, IHubContext<AppointmentHub> appointmentHub)
        {
            _context = context;
            _appointmentHub = appointmentHub;
        }

        public async Task<IActionResult> Create(CreateAppointmentRequest request, int userId)
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patientProfile == null)
                return new BadRequestObjectResult("Профиль пациента не найден");

            var timeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(x => x.Id == request.TimeSlotId);

            if (timeSlot == null)
                return new NotFoundObjectResult("Слот не найден");

            if (timeSlot.StartTime <= DateTime.UtcNow)
                return new BadRequestObjectResult("Нельзя записаться на прошедшее время");

            if (timeSlot.Status != TimeSlotStatus.Available)
                return new BadRequestObjectResult("Слот уже занят или недоступен");

            var service = await _context.Services
                .FirstOrDefaultAsync(x => x.Id == request.ServiceId);

            if (service == null)
                return new NotFoundObjectResult("Услуга не найдена");

            var doctorHasService = await _context.DoctorServices
                .AnyAsync(x => x.DoctorProfileId == timeSlot.DoctorProfileId && x.ServiceId == request.ServiceId);

            if (!doctorHasService)
                return new BadRequestObjectResult("Эта услуга недоступна у выбранного врача");

            var appointmentExists = await _context.Appointments
                .AnyAsync(x => x.TimeSlotId == request.TimeSlotId);

            if (appointmentExists)
                return new BadRequestObjectResult("На этот слот уже есть запись");

            Referral? referral = null;

            if (request.ReferralId.HasValue)
            {
                referral = await _context.Referrals
                    .FirstOrDefaultAsync(x => x.Id == request.ReferralId.Value);

                if (referral == null)
                    return new BadRequestObjectResult("Направление не найдено");

                if (referral.PatientProfileId != patientProfile.Id)
                    return new BadRequestObjectResult("Это направление принадлежит другому пациенту");

                if (referral.Type != ReferralType.Internal)
                    return new BadRequestObjectResult("По внешнему направлению нельзя записаться внутри клиники");

                if (referral.Status != ReferralStatus.Pending)
                    return new BadRequestObjectResult("Направление уже использовано или недоступно");

                if (referral.ServiceId != request.ServiceId)
                    return new BadRequestObjectResult("Услуга записи не совпадает с услугой в направлении");
            }

            var appointment = new Appointment
            {
                TimeSlotId = request.TimeSlotId,
                PatientProfileId = patientProfile.Id,
                ServiceId = request.ServiceId,
                ReferralId = request.ReferralId,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            timeSlot.Status = TimeSlotStatus.Booked;

            if (referral != null)
            {
                referral.Status = ReferralStatus.Booked;
            }

            await _context.SaveChangesAsync();
            await NotifyAppointmentChanged(appointment.Id, "created");

            return new OkObjectResult("Запись на приём успешно создана");
        }

        public async Task<IActionResult> GetMyAppointments(int userId)
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patientProfile == null)
                return new BadRequestObjectResult("Профиль пациента не найден");

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.PatientProfileId == patientProfile.Id)
                .OrderByDescending(x => x.TimeSlot.StartTime)
                .ToListAsync();

            var result = appointments.Select(x => new
            {
                id = x.Id,
                doctor = $"{x.TimeSlot.DoctorProfile.User.FirstName} {x.TimeSlot.DoctorProfile.User.LastName}",
                service = x.Service.Name,
                startTime = x.TimeSlot.StartTime,
                endTime = x.TimeSlot.EndTime,
                status = x.Status.ToString(),
                conclusion = x.DoctorConclusion
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetDoctorAppointments(int userId)
        {
            var doctorProfile = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctorProfile == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                .Include(x => x.PatientProfile)
                    .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.TimeSlot.DoctorProfileId == doctorProfile.Id)
                .OrderBy(x => x.TimeSlot.StartTime)
                .ToListAsync();

            return new OkObjectResult(MapDoctorAppointments(appointments));
        }

        public async Task<IActionResult> Cancel(int appointmentId, int userId)
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patientProfile == null)
                return new BadRequestObjectResult("Профиль пациента не найден");

            var appointment = await _context.Appointments
                .Include(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientProfileId == patientProfile.Id);

            if (appointment == null)
                return new NotFoundObjectResult("Запись не найдена");

            if (appointment.Status != AppointmentStatus.Scheduled)
                return new BadRequestObjectResult("Можно отменить только запланированную запись");

            if (appointment.TimeSlot.StartTime <= DateTime.UtcNow)
                return new BadRequestObjectResult("Нельзя отменить запись после начала приёма");

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.TimeSlot.Status = TimeSlotStatus.Available;

            await _context.SaveChangesAsync();
            await NotifyAppointmentChanged(appointment.Id, "cancelled");

            return new OkObjectResult("Запись успешно отменена");
        }

        public async Task<IActionResult> UpdateStatus(int appointmentId, UpdateAppointmentStatusRequest request, int userId)
        {
            if (!Enum.IsDefined(typeof(AppointmentStatus), request.Status))
                return new BadRequestObjectResult("Некорректный статус");

            var doctorProfile = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctorProfile == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var appointment = await _context.Appointments
                .Include(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.TimeSlot.DoctorProfileId == doctorProfile.Id);

            if (appointment == null)
                return new NotFoundObjectResult("Запись не найдена");

            appointment.Status = (AppointmentStatus)request.Status;
            appointment.UpdatedAt = DateTime.UtcNow;

            appointment.TimeSlot.Status =
                appointment.Status == AppointmentStatus.Cancelled
                    ? TimeSlotStatus.Available
                    : TimeSlotStatus.Booked;

            await _context.SaveChangesAsync();
            await NotifyAppointmentChanged(appointment.Id, "status-updated");

            return new OkObjectResult("Статус записи успешно обновлён");
        }

        public async Task<IActionResult> AddConclusion(int appointmentId, AddDoctorConclusionRequest request, int userId)
        {
            var doctorProfile = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctorProfile == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var appointment = await _context.Appointments
                .Include(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.TimeSlot.DoctorProfileId == doctorProfile.Id);

            if (appointment == null)
                return new NotFoundObjectResult("Запись не найдена");

            if (appointment.Status != AppointmentStatus.Completed)
                return new BadRequestObjectResult("Невозможно добавить заключение до завершения приёма");

            appointment.DoctorConclusion = request.DoctorConclusion;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await NotifyAppointmentChanged(appointment.Id, "conclusion-updated");

            return new OkObjectResult("Заключение врача успешно сохранено");
        }

        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.PatientProfile)
                    .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .OrderByDescending(x => x.TimeSlot.StartTime)
                .ToListAsync();

            var result = appointments.Select(x => new
            {
                id = x.Id,
                patientId = x.PatientProfileId,
                doctorId = x.TimeSlot.DoctorProfileId,
                patient = $"{x.PatientProfile.User.FirstName} {x.PatientProfile.User.LastName}",
                doctor = $"{x.TimeSlot.DoctorProfile.User.FirstName} {x.TimeSlot.DoctorProfile.User.LastName}",
                service = x.Service.Name,
                startTime = x.TimeSlot.StartTime,
                endTime = x.TimeSlot.EndTime,
                status = x.Status.ToString(),
                conclusion = x.DoctorConclusion
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetDoctorAppointments(DateTime? from, DateTime? to, int userId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctor == null)
                return new BadRequestObjectResult("Доктор не найден");

            var query = _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.PatientProfile)
                    .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.TimeSlot.DoctorProfileId == doctor.Id)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(x => x.TimeSlot.StartTime >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.TimeSlot.StartTime <= to.Value);

            var appointments = await query
                .OrderBy(x => x.TimeSlot.StartTime)
                .ToListAsync();

            return new OkObjectResult(MapDoctorAppointments(appointments));
        }

        private static object MapDoctorAppointments(IEnumerable<Appointment> appointments)
        {
            return appointments.Select(x => new
            {
                id = x.Id,
                patientId = x.PatientProfileId,
                patient = $"{x.PatientProfile.User.FirstName} {x.PatientProfile.User.LastName}",
                service = x.Service.Name,
                startTime = x.TimeSlot.StartTime,
                endTime = x.TimeSlot.EndTime,
                status = x.Status.ToString(),
                conclusion = x.DoctorConclusion
            });
        }

        private async Task NotifyAppointmentChanged(int appointmentId, string action)
        {
            var appointment = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                .Include(x => x.PatientProfile)
                .FirstOrDefaultAsync(x => x.Id == appointmentId);

            if (appointment == null)
                return;

            var payload = new
            {
                appointmentId = appointment.Id,
                action,
                patientUserId = appointment.PatientProfile.UserId,
                doctorUserId = appointment.TimeSlot.DoctorProfile.UserId,
                startTime = appointment.TimeSlot.StartTime,
                endTime = appointment.TimeSlot.EndTime,
                status = appointment.Status.ToString()
            };

            await _appointmentHub.Clients.User(payload.patientUserId.ToString())
                .SendAsync("AppointmentChanged", payload);

            if (payload.doctorUserId != payload.patientUserId)
            {
                await _appointmentHub.Clients.User(payload.doctorUserId.ToString())
                    .SendAsync("AppointmentChanged", payload);
            }

            await _appointmentHub.Clients.Group(AppointmentHub.AdminGroupName)
                .SendAsync("AppointmentChanged", payload);
        }
    }
}
