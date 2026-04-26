using DigiClinicApi.AppDbContext;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ContextDb _context;

        public ProfileService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetMe(int userId)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .Include(x => x.PatientProfile)
                .Include(x => x.DoctorProfile)
                    .ThenInclude(x => x!.Specialization)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return new NotFoundObjectResult("Пользователь не найден");

            if (user.DoctorProfile != null)
            {
                return new OkObjectResult(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phone = user.Phone,
                    role = user.Role.Name,
                    isActive = user.IsActive,
                    telegramLinked = user.TelegramChatId.HasValue,
                    doctorProfile = new
                    {
                        id = user.DoctorProfile.Id,
                        specializationId = user.DoctorProfile.SpecializationId,
                        specialization = user.DoctorProfile.Specialization?.Name,
                        experienceYears = user.DoctorProfile.ExperienceYears,
                        cabinetNumber = user.DoctorProfile.CabinetNumber,
                        bio = user.DoctorProfile.Bio
                    }
                });
            }

            if (user.PatientProfile != null)
            {
                return new OkObjectResult(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phone = user.Phone,
                    role = user.Role.Name,
                    isActive = user.IsActive,
                    telegramLinked = user.TelegramChatId.HasValue,
                    patientProfile = new
                    {
                        id = user.PatientProfile.Id,
                        birthDate = user.PatientProfile.BirthDate,
                        gender = user.PatientProfile.Gender,
                        address = user.PatientProfile.Address,
                        emergencyContact = user.PatientProfile.EmergencyContact
                    }
                });
            }

            return new OkObjectResult(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phone = user.Phone,
                role = user.Role.Name,
                isActive = user.IsActive,
                telegramLinked = user.TelegramChatId.HasValue
            });
        }

        public async Task<IActionResult> UpdateMe(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .Include(x => x.PatientProfile)
                .Include(x => x.DoctorProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return new NotFoundObjectResult("Пользователь не найден");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.PatientProfile != null)
            {
                user.PatientProfile.BirthDate = request.BirthDate;
                user.PatientProfile.Gender = request.Gender;
                user.PatientProfile.Address = request.Address;
                user.PatientProfile.EmergencyContact = request.EmergencyContact;
            }

            if (user.DoctorProfile != null)
            {
                if (request.SpecializationId.HasValue)
                {
                    var specializationExists = await _context.Specializations
                        .AnyAsync(x => x.Id == request.SpecializationId.Value);

                    if (!specializationExists)
                        return new BadRequestObjectResult("Специализация не найдена");

                    user.DoctorProfile.SpecializationId = request.SpecializationId.Value;
                }

                user.DoctorProfile.ExperienceYears = request.ExperienceYears ?? user.DoctorProfile.ExperienceYears;
                user.DoctorProfile.CabinetNumber = request.CabinetNumber;
                user.DoctorProfile.Bio = request.Bio;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult("Профиль успешно обновлён");
        }
    }
}
