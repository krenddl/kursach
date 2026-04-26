using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class ReferralService : IReferralService
    {
        private readonly ContextDb _context;

        public ReferralService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(CreateReferralRequest request, int userId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctor == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var appointment = await _context.Appointments
                .Include(x => x.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == request.SourceAppointmentId);

            if (appointment == null)
                return new NotFoundObjectResult("Приём не найден");

            if (appointment.TimeSlot.DoctorProfileId != doctor.Id)
                return new BadRequestObjectResult("Нельзя создать направление для чужого приёма");

            var service = await _context.Services
                .FirstOrDefaultAsync(x => x.Id == request.ServiceId);

            if (service == null)
                return new NotFoundObjectResult("Услуга не найдена");

            var referral = new Referral
            {
                PatientProfileId = appointment.PatientProfileId,
                CreatedByDoctorProfileId = doctor.Id,
                SourceAppointmentId = appointment.Id,
                ServiceId = service.Id,
                Comment = request.Comment,
                Type = (ReferralType)request.Type,
                Status = ReferralStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                id = referral.Id,
                patientProfileId = referral.PatientProfileId,
                createdByDoctorProfileId = referral.CreatedByDoctorProfileId,
                sourceAppointmentId = referral.SourceAppointmentId,
                serviceId = referral.ServiceId,
                serviceName = service.Name,
                comment = referral.Comment,
                status = referral.Status.ToString(),
                createdAt = referral.CreatedAt,
                type = referral.Type.ToString()
            });
        }

        public async Task<IActionResult> GetMyReferrals(int userId)
        {
            var patient = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patient == null)
                return new BadRequestObjectResult("Профиль пациента не найден");

            var referrals = await _context.Referrals
                .Include(x => x.Service)
                .Include(x => x.CreatedByDoctorProfile)
                    .ThenInclude(x => x.User)
                .Include(x => x.SourceAppointment)
                    .ThenInclude(x => x!.TimeSlot)
                .Where(x => x.PatientProfileId == patient.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = referrals.Select(x => new
            {
                id = x.Id,
                sourceAppointmentId = x.SourceAppointmentId,
                serviceId = x.ServiceId,
                serviceName = x.Service.Name,
                createdByDoctor = x.CreatedByDoctorProfile.User.FirstName + " " + x.CreatedByDoctorProfile.User.LastName,
                comment = x.Comment,
                status = x.Status.ToString(),
                type = x.Type.ToString(),
                createdAt = x.CreatedAt,
                sourceAppointmentDate = x.SourceAppointment != null
    ? (DateTime?)x.SourceAppointment.TimeSlot.StartTime
    : null
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetDoctorReferrals(int userId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctor == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var referrals = await _context.Referrals
                .Include(x => x.Service)
                .Include(x => x.PatientProfile)
                    .ThenInclude(x => x.User)
                .Where(x => x.CreatedByDoctorProfileId == doctor.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = referrals.Select(x => new
            {
                id = x.Id,
                sourceAppointmentId = x.SourceAppointmentId,
                serviceId = x.ServiceId,
                serviceName = x.Service.Name,
                patient = x.PatientProfile.User.FirstName + " " + x.PatientProfile.User.LastName,
                comment = x.Comment,
                type = x.Type.ToString(),
                status = x.Status.ToString(),
                createdAt = x.CreatedAt
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> MarkBooked(int referralId, int userId)
        {
            var patient = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patient == null)
                return new BadRequestObjectResult("Профиль пациента не найден");

            var referral = await _context.Referrals
                .FirstOrDefaultAsync(x => x.Id == referralId);

            if (referral == null)
                return new NotFoundObjectResult("Направление не найдено");

            if (referral.PatientProfileId != patient.Id)
                return new BadRequestObjectResult("Нельзя изменить чужое направление");

            referral.Status = ReferralStatus.Booked;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                id = referral.Id,
                status = referral.Status.ToString()
            });
        }
    }
}
