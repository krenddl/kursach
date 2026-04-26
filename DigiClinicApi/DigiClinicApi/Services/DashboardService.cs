using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ContextDb _context;

        public DashboardService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetPatientDashboard(int userId)
        {
            var patient = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patient == null)
                return new BadRequestObjectResult("Patient profile not found");

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.Service)
                .Where(x => x.PatientProfileId == patient.Id)
                .OrderByDescending(x => x.TimeSlot.StartTime)
                .ToListAsync();

            var nextAppointment = appointments
                .Where(x => x.Status == AppointmentStatus.Scheduled && x.TimeSlot.StartTime >= DateTime.UtcNow)
                .OrderBy(x => x.TimeSlot.StartTime)
                .Select(x => new
                {
                    id = x.Id,
                    doctor = x.TimeSlot.DoctorProfile.User.FirstName + " " + x.TimeSlot.DoctorProfile.User.LastName,
                    service = x.Service.Name,
                    startTime = x.TimeSlot.StartTime,
                    status = x.Status.ToString()
                })
                .FirstOrDefault();

            var history = appointments
                .Select(x => new
                {
                    id = x.Id,
                    doctor = x.TimeSlot.DoctorProfile.User.FirstName + " " + x.TimeSlot.DoctorProfile.User.LastName,
                    service = x.Service.Name,
                    startTime = x.TimeSlot.StartTime,
                    status = x.Status.ToString()
                })
                .ToList();

            var result = new
            {
                total = appointments.Count,
                completed = appointments.Count(x => x.Status == AppointmentStatus.Completed),
                cancelled = appointments.Count(x => x.Status == AppointmentStatus.Cancelled),
                upcomingCount = appointments.Count(x => x.Status == AppointmentStatus.Scheduled),
                nextAppointment = nextAppointment,
                history = history
            };

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetDoctorDashboard(int userId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctor == null)
                return new BadRequestObjectResult("Doctor profile not found");

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                .Where(x => x.TimeSlot.DoctorProfileId == doctor.Id)
                .ToListAsync();

            var result = new
            {
                total = appointments.Count,
                completed = appointments.Count(x => x.Status == AppointmentStatus.Completed),
                cancelled = appointments.Count(x => x.Status == AppointmentStatus.Cancelled),
                noShow = appointments.Count(x => x.Status == AppointmentStatus.NoShow),
                upcoming = appointments.Count(x => x.Status == AppointmentStatus.Scheduled),

                byDay = appointments
                    .Where(x => x.TimeSlot != null)
                    .GroupBy(x => x.TimeSlot.StartTime.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        count = g.Count()
                    })
                    .OrderBy(x => x.date)
            };

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetAdminDashboard()
        {
            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                .ToListAsync();

            var doctors = await _context.DoctorProfiles
                .Include(x => x.User)
                .ToListAsync();

            var patientsCount = await _context.PatientProfiles.CountAsync();

            var timeSlots = await _context.TimeSlots.ToListAsync();

            var doctorLoad = doctors.Select(d =>
            {
                var doctorSlots = timeSlots.Where(ts => ts.DoctorProfileId == d.Id).ToList();
                var doctorSlotIds = doctorSlots.Select(ts => ts.Id).ToList();

                var bookedCount = appointments.Count(a => doctorSlotIds.Contains(a.TimeSlotId));

                return new
                {
                    doctorId = d.Id,
                    doctorName = $"{d.User.FirstName} {d.User.LastName}",
                    totalSlots = doctorSlots.Count,
                    bookedSlots = bookedCount,
                    freeSlots = doctorSlots.Count - bookedCount
                };
            });

            var result = new
            {
                totalAppointments = appointments.Count,
                totalDoctors = doctors.Count,
                totalPatients = patientsCount,

                byStatus = new
                {
                    scheduled = appointments.Count(x => x.Status == AppointmentStatus.Scheduled),
                    completed = appointments.Count(x => x.Status == AppointmentStatus.Completed),
                    cancelled = appointments.Count(x => x.Status == AppointmentStatus.Cancelled),
                    noShow = appointments.Count(x => x.Status == AppointmentStatus.NoShow)
                },

                doctorLoad = doctorLoad
            };

            return new OkObjectResult(result);
        }
    }
}
