using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorServiceLink = DigiClinicApi.Models.DoctorService;

namespace DigiClinicApi.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ContextDb _context;

        public DoctorService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetAll(int? specializationId)
        {
            var doctors = await _context.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Specialization)
                .Include(x => x.DoctorServices)
                .Include(x => x.TimeSlots)
                .ToListAsync();

            if (specializationId != null)
            {
                doctors = doctors
                    .Where(x => x.SpecializationId == specializationId)
                    .ToList();
            }

            var result = doctors.Select(x => new
            {
                id = x.Id,
                firstName = x.User.FirstName,
                lastName = x.User.LastName,
                fullName = $"{x.User.FirstName} {x.User.LastName}",
                email = x.User.Email,
                phone = x.User.Phone,
                isActive = x.User.IsActive,
                specializationId = x.SpecializationId,
                specialization = x.Specialization.Name,
                specializationName = x.Specialization.Name,
                experienceYears = x.ExperienceYears,
                experience = x.ExperienceYears,
                cabinetNumber = x.CabinetNumber,
                cabinet = x.CabinetNumber,
                bio = x.Bio,
                serviceIds = x.DoctorServices.Select(ds => ds.ServiceId).ToList(),
                availableSlotsCount = x.TimeSlots.Count(ts =>
                    ts.Status == TimeSlotStatus.Available &&
                    ts.StartTime >= DateTime.UtcNow)
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _context.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Specialization)
                .Include(x => x.DoctorServices)
                    .ThenInclude(x => x.Service)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                return new NotFoundObjectResult("Doctor not found");

            var result = new
            {
                id = doctor.Id,
                firstName = doctor.User.FirstName,
                lastName = doctor.User.LastName,
                fullName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                email = doctor.User.Email,
                phone = doctor.User.Phone,
                isActive = doctor.User.IsActive,
                specializationId = doctor.SpecializationId,
                specialization = doctor.Specialization.Name,
                specializationName = doctor.Specialization.Name,
                experienceYears = doctor.ExperienceYears,
                experience = doctor.ExperienceYears,
                cabinetNumber = doctor.CabinetNumber,
                cabinet = doctor.CabinetNumber,
                bio = doctor.Bio,
                serviceIds = doctor.DoctorServices.Select(ds => ds.Service.Id).ToList(),
                services = doctor.DoctorServices.Select(ds => new
                {
                    id = ds.Service.Id,
                    name = ds.Service.Name,
                    description = ds.Service.Description,
                    price = ds.Service.Price,
                    durationMinutes = ds.Service.DurationMinutes
                }).ToList()
            };

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> Create(CreateDoctorRequest request)
        {
            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (emailExists)
                return new BadRequestObjectResult("Email already exists");

            var specialization = await _context.Specializations
                .FirstOrDefaultAsync(x => x.Id == request.SpecializationId);

            if (specialization == null)
                return new BadRequestObjectResult("Specialization not found");

            var serviceIds = request.ServiceIds
                .Distinct()
                .ToList();

            var services = await _context.Services
                .Where(x => serviceIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                return new BadRequestObjectResult("One or more services were not found");

            var role = await _context.Roles
                .FirstAsync(x => x.Id == (int)UserRole.Doctor);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctorProfile = new DoctorProfile
            {
                UserId = user.Id,
                SpecializationId = request.SpecializationId,
                ExperienceYears = request.ExperienceYears,
                CabinetNumber = request.CabinetNumber,
                Bio = request.Bio
            };

            _context.DoctorProfiles.Add(doctorProfile);
            await _context.SaveChangesAsync();

            await SyncDoctorServices(doctorProfile.Id, serviceIds);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Doctor created");
        }

        public async Task<IActionResult> Update(int id, UpdateDoctorReques request)
        {
            var doctor = await _context.DoctorProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                return new NotFoundObjectResult("Doctor not found");

            var specialization = await _context.Specializations
                .FirstOrDefaultAsync(x => x.Id == request.SpecializationId);

            if (specialization == null)
                return new BadRequestObjectResult("Specialization not found");

            var serviceIds = request.ServiceIds
                .Distinct()
                .ToList();

            var services = await _context.Services
                .Where(x => serviceIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                return new BadRequestObjectResult("One or more services were not found");

            doctor.User.FirstName = request.FirstName;
            doctor.User.LastName = request.LastName;
            doctor.User.Phone = request.Phone;
            doctor.User.IsActive = request.IsActive;
            doctor.User.UpdatedAt = DateTime.UtcNow;

            doctor.SpecializationId = request.SpecializationId;
            doctor.ExperienceYears = request.ExperienceYears;
            doctor.CabinetNumber = request.CabinetNumber;
            doctor.Bio = request.Bio;

            await SyncDoctorServices(id, serviceIds);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Doctor updated");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.DoctorProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                return new NotFoundObjectResult("Doctor not found");

            var hasAppointments = await _context.TimeSlots
                .AnyAsync(x => x.DoctorProfileId == id);

            if (hasAppointments)
            {
                doctor.User.IsActive = false;
                doctor.User.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new OkObjectResult("Doctor has related schedule data, so profile was deactivated instead of deleted");
            }

            _context.DoctorProfiles.Remove(doctor);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Doctor deleted");
        }

        private async Task SyncDoctorServices(int doctorId, List<int> serviceIds)
        {
            var existing = await _context.DoctorServices
                .Where(x => x.DoctorProfileId == doctorId)
                .ToListAsync();

            var toDelete = existing
                .Where(x => !serviceIds.Contains(x.ServiceId))
                .ToList();

            if (toDelete.Count > 0)
            {
                _context.DoctorServices.RemoveRange(toDelete);
            }

            var existingIds = existing
                .Select(x => x.ServiceId)
                .ToHashSet();

            var toAdd = serviceIds
                .Where(serviceId => !existingIds.Contains(serviceId))
                .Select(serviceId => new DoctorServiceLink
                {
                    DoctorProfileId = doctorId,
                    ServiceId = serviceId
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                await _context.DoctorServices.AddRangeAsync(toAdd);
            }
        }
    }
}
