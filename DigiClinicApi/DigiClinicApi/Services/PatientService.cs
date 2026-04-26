using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class PatientService : IPatientService
    {
        private readonly ContextDb _context;

        public PatientService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetAll()
        {
            var patients = await _context.PatientProfiles
                .Include(x => x.User)
                .Include(x => x.Appointments)
                    .ThenInclude(x => x.TimeSlot)
                .Include(x => x.Appointments)
                    .ThenInclude(x => x.Service)
                .OrderBy(x => x.User.LastName)
                .ToListAsync();

            var result = patients.Select(x => new
            {
                id = x.Id,
                firstName = x.User.FirstName,
                lastName = x.User.LastName,
                fullName = $"{x.User.FirstName} {x.User.LastName}",
                email = x.User.Email,
                phone = x.User.Phone,
                birthDate = x.BirthDate,
                gender = x.Gender,
                address = x.Address,
                emergencyContact = x.EmergencyContact,
                isActive = x.User.IsActive,
                totalAppointments = x.Appointments.Count,
                lastVisit = x.Appointments
                    .Where(a => a.TimeSlot != null)
                    .OrderByDescending(a => a.TimeSlot.StartTime)
                    .Select(a => a.TimeSlot.StartTime)
                    .FirstOrDefault(),
                nextAppointment = x.Appointments
                    .Where(a => a.TimeSlot != null &&
                                a.Status == AppointmentStatus.Scheduled &&
                                a.TimeSlot.StartTime >= DateTime.UtcNow)
                    .OrderBy(a => a.TimeSlot.StartTime)
                    .Select(a => new
                    {
                        startTime = a.TimeSlot.StartTime,
                        service = a.Service.Name
                    })
                    .FirstOrDefault()
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.PatientProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient == null)
                return new NotFoundObjectResult("Пациент не найден");

            var result = new
            {
                id = patient.Id,
                firstName = patient.User.FirstName,
                lastName = patient.User.LastName,
                fullName = $"{patient.User.FirstName} {patient.User.LastName}",
                email = patient.User.Email,
                phone = patient.User.Phone,
                birthDate = patient.BirthDate,
                gender = patient.Gender,
                address = patient.Address,
                emergencyContact = patient.EmergencyContact,
                isActive = patient.User.IsActive
            };

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetHistory(int id, int doctorUserId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == doctorUserId);

            if (doctor == null)
                return new BadRequestObjectResult("Профиль врача не найден");

            var patient = await _context.PatientProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient == null)
                return new NotFoundObjectResult("Пациент не найден");

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.PatientProfileId == id)
                .OrderByDescending(x => x.TimeSlot.StartTime)
                .ToListAsync();

            var result = new
            {
                patient = new
                {
                    id = patient.Id,
                    fullName = $"{patient.User.FirstName} {patient.User.LastName}"
                },
                history = appointments.Select(x => new
                {
                    id = x.Id,
                    doctor = $"{x.TimeSlot.DoctorProfile.User.FirstName} {x.TimeSlot.DoctorProfile.User.LastName}",
                    service = x.Service.Name,
                    startTime = x.TimeSlot.StartTime,
                    endTime = x.TimeSlot.EndTime,
                    status = x.Status.ToString(),
                    conclusion = x.DoctorConclusion
                })
            };

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> Update(int id, UpdatePatientRequest request)
        {
            var patient = await _context.PatientProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient == null)
                return new NotFoundObjectResult("Пациент не найден");

            patient.User.FirstName = request.FirstName;
            patient.User.LastName = request.LastName;
            patient.User.Phone = request.Phone;
            patient.User.UpdatedAt = DateTime.UtcNow;

            patient.BirthDate = request.BirthDate;
            patient.Gender = request.Gender;
            patient.Address = request.Address;
            patient.EmergencyContact = request.EmergencyContact;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Данные пациента обновлены");
        }
    }
}
