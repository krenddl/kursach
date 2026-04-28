using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextDb>();

            await SeedRoles(context);
            var specializations = await SeedSpecializations(context);
            var services = await SeedServices(context);

            var adminRole = await context.Roles.FirstAsync(x => x.Name == "Admin");
            var doctorRole = await context.Roles.FirstAsync(x => x.Name == "Doctor");
            var patientRole = await context.Roles.FirstAsync(x => x.Name == "Patient");

            await EnsureUser(context, new User
            {
                FirstName = "Айгерим",
                LastName = "Серикова",
                Email = "admin1@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Phone = "+77010000001",
                RoleId = adminRole.Id,
                IsActive = true
            });

            await EnsureUser(context, new User
            {
                FirstName = "Данияр",
                LastName = "Нургалиев",
                Email = "admin2@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Phone = "+77010000002",
                RoleId = adminRole.Id,
                IsActive = true
            });

            var cardiology = specializations["Cardiologist"];
            var neurology = specializations["Neurologist"];
            var dentistry = specializations["Dentist"];

            var doctorUser1 = await EnsureUser(context, new User
            {
                FirstName = "Сара",
                LastName = "Джонсон",
                Email = "doctor1@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                Phone = "+77020000001",
                RoleId = doctorRole.Id,
                IsActive = true
            });

            var doctorUser2 = await EnsureUser(context, new User
            {
                FirstName = "Эмили",
                LastName = "Родригес",
                Email = "doctor2@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                Phone = "+77020000002",
                RoleId = doctorRole.Id,
                IsActive = true
            });

            var doctorUser3 = await EnsureUser(context, new User
            {
                FirstName = "Майкл",
                LastName = "Браун",
                Email = "doctor3@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                Phone = "+77020000003",
                RoleId = doctorRole.Id,
                IsActive = true
            });

            var doctorProfile1 = await EnsureDoctorProfile(context, doctorUser1.Id, new DoctorProfile
            {
                UserId = doctorUser1.Id,
                SpecializationId = cardiology.Id,
                ExperienceYears = 8,
                CabinetNumber = "204",
                Bio = "Кардиолог клиники «Лотос». Специализируется на диагностике и лечении сердечно-сосудистых заболеваний."
            });

            var doctorProfile2 = await EnsureDoctorProfile(context, doctorUser2.Id, new DoctorProfile
            {
                UserId = doctorUser2.Id,
                SpecializationId = neurology.Id,
                ExperienceYears = 11,
                CabinetNumber = "305",
                Bio = "Невролог с опытом работы более 10 лет. Ведёт первичные и повторные приёмы."
            });

            var doctorProfile3 = await EnsureDoctorProfile(context, doctorUser3.Id, new DoctorProfile
            {
                UserId = doctorUser3.Id,
                SpecializationId = dentistry.Id,
                ExperienceYears = 6,
                CabinetNumber = "102",
                Bio = "Стоматолог клиники «Лотос». Проводит консультации, профилактические осмотры и лечение."
            });

            var patientUser1 = await EnsureUser(context, new User
            {
                FirstName = "Иван",
                LastName = "Петров",
                Email = "patient1@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Phone = "+77030000001",
                RoleId = patientRole.Id,
                IsActive = true
            });

            var patientUser2 = await EnsureUser(context, new User
            {
                FirstName = "Алина",
                LastName = "Бронева",
                Email = "patient2@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Phone = "+77030000002",
                RoleId = patientRole.Id,
                IsActive = true
            });

            var patientUser3 = await EnsureUser(context, new User
            {
                FirstName = "Руслан",
                LastName = "Ахметов",
                Email = "patient3@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Phone = "+77030000003",
                RoleId = patientRole.Id,
                IsActive = true
            });

            var patientUser4 = await EnsureUser(context, new User
            {
                FirstName = "Марина",
                LastName = "Ким",
                Email = "patient4@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Phone = "+77030000004",
                RoleId = patientRole.Id,
                IsActive = true
            });

            var patientUser5 = await EnsureUser(context, new User
            {
                FirstName = "Тимур",
                LastName = "Садыков",
                Email = "patient5@lotos.kz",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Phone = "+77030000005",
                RoleId = patientRole.Id,
                IsActive = true
            });

            var patientProfile1 = await EnsurePatientProfile(context, patientUser1.Id, new PatientProfile
            {
                UserId = patientUser1.Id,
                BirthDate = DateTime.SpecifyKind(new DateTime(2002, 5, 14), DateTimeKind.Utc),
                Gender = "Male",
                Address = "Алматы",
                EmergencyContact = "Анна Петрова"
            });

            var patientProfile2 = await EnsurePatientProfile(context, patientUser2.Id, new PatientProfile
            {
                UserId = patientUser2.Id,
                BirthDate = DateTime.SpecifyKind(new DateTime(1999, 10, 5), DateTimeKind.Utc),
                Gender = "Female",
                Address = "Астана",
                EmergencyContact = "Серик Садыков"
            });

            var patientProfile3 = await EnsurePatientProfile(context, patientUser3.Id, new PatientProfile
            {
                UserId = patientUser3.Id,
                BirthDate = DateTime.SpecifyKind(new DateTime(1994, 3, 21), DateTimeKind.Utc),
                Gender = "Male",
                Address = "Шымкент",
                EmergencyContact = "Айжан Ахметова"
            });

            var patientProfile4 = await EnsurePatientProfile(context, patientUser4.Id, new PatientProfile
            {
                UserId = patientUser4.Id,
                BirthDate = DateTime.SpecifyKind(new DateTime(1991, 8, 2), DateTimeKind.Utc),
                Gender = "Female",
                Address = "Караганда",
                EmergencyContact = "Елена Ким"
            });

            var patientProfile5 = await EnsurePatientProfile(context, patientUser5.Id, new PatientProfile
            {
                UserId = patientUser5.Id,
                BirthDate = DateTime.SpecifyKind(new DateTime(1988, 1, 26), DateTimeKind.Utc),
                Gender = "Male",
                Address = "Алматы",
                EmergencyContact = "Гульмира Садыкова"
            });

            var primaryCardiology = services["Первичная консультация кардиолога"];
            var followUpCardiology = services["Повторная консультация кардиолога"];
            var ecgCheck = services["ЭКГ с расшифровкой"];
            var holterMonitoring = services["Холтер-мониторирование ЭКГ"];

            var primaryNeurology = services["Первичная консультация невролога"];
            var followUpNeurology = services["Повторная консультация невролога"];
            var mriReview = services["Консультация по результатам МРТ/КТ"];

            var dentalCheckup = services["Первичный осмотр стоматолога"];
            var dentalCleaning = services["Профессиональная гигиена полости рта"];
            var dentalTreatment = services["Лечение кариеса"];
            var toothExtraction = services["Удаление зуба"];

            await EnsureDoctorService(context, doctorProfile1.Id, primaryCardiology.Id);
            await EnsureDoctorService(context, doctorProfile1.Id, followUpCardiology.Id);
            await EnsureDoctorService(context, doctorProfile1.Id, ecgCheck.Id);
            await EnsureDoctorService(context, doctorProfile1.Id, holterMonitoring.Id);

            await EnsureDoctorService(context, doctorProfile2.Id, primaryNeurology.Id);
            await EnsureDoctorService(context, doctorProfile2.Id, followUpNeurology.Id);
            await EnsureDoctorService(context, doctorProfile2.Id, mriReview.Id);

            await EnsureDoctorService(context, doctorProfile3.Id, dentalCheckup.Id);
            await EnsureDoctorService(context, doctorProfile3.Id, dentalCleaning.Id);
            await EnsureDoctorService(context, doctorProfile3.Id, dentalTreatment.Id);
            await EnsureDoctorService(context, doctorProfile3.Id, toothExtraction.Id);

            await context.SaveChangesAsync();

            await EnsureDoctorSlotsRange(context, doctorProfile1.Id, -70, 95, new[] { 9, 10, 11, 14, 15, 16 }, 30);
            await EnsureDoctorSlotsRange(context, doctorProfile2.Id, -70, 95, new[] { 10, 11, 12, 15, 16 }, 30);
            await EnsureDoctorSlotsRange(context, doctorProfile3.Id, -70, 95, new[] { 9, 10, 12, 13, 15 }, 30);

            await context.SaveChangesAsync();

            var appointmentSeeds = new[]
            {
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile1.Id, primaryCardiology.Id, -65, 9, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile2.Id, ecgCheck.Id, -58, 10, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile3.Id, followUpCardiology.Id, -52, 14, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile4.Id, holterMonitoring.Id, -47, 15, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile5.Id, primaryCardiology.Id, -43, 9, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile1.Id, ecgCheck.Id, -39, 10, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile2.Id, followUpCardiology.Id, -34, 14, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile3.Id, holterMonitoring.Id, -29, 15, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile4.Id, primaryCardiology.Id, -24, 11, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile5.Id, ecgCheck.Id, -18, 10, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile1.Id, followUpCardiology.Id, -12, 14, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile2.Id, primaryCardiology.Id, -6, 9, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile3.Id, ecgCheck.Id, -2, 10, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile4.Id, primaryCardiology.Id, 2, 9, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile5.Id, holterMonitoring.Id, 6, 15, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile1.Id, patientProfile1.Id, followUpCardiology.Id, 11, 14, 0, AppointmentStatus.Scheduled),

                new AppointmentSeedItem(doctorProfile2.Id, patientProfile2.Id, primaryNeurology.Id, -63, 10, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile3.Id, mriReview.Id, -57, 11, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile4.Id, followUpNeurology.Id, -50, 15, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile5.Id, primaryNeurology.Id, -46, 10, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile1.Id, mriReview.Id, -40, 11, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile2.Id, followUpNeurology.Id, -35, 16, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile3.Id, primaryNeurology.Id, -30, 12, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile4.Id, mriReview.Id, -25, 11, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile5.Id, followUpNeurology.Id, -20, 15, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile1.Id, primaryNeurology.Id, -14, 10, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile2.Id, mriReview.Id, -9, 11, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile3.Id, followUpNeurology.Id, -4, 16, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile4.Id, primaryNeurology.Id, 3, 10, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile5.Id, mriReview.Id, 8, 11, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile2.Id, patientProfile2.Id, followUpNeurology.Id, 13, 15, 0, AppointmentStatus.Scheduled),

                new AppointmentSeedItem(doctorProfile3.Id, patientProfile3.Id, dentalCheckup.Id, -61, 9, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile4.Id, dentalCleaning.Id, -55, 12, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile5.Id, dentalTreatment.Id, -49, 13, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile1.Id, toothExtraction.Id, -44, 15, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile2.Id, dentalCheckup.Id, -38, 9, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile3.Id, dentalCleaning.Id, -33, 12, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile4.Id, dentalTreatment.Id, -28, 13, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile5.Id, toothExtraction.Id, -23, 15, 0, AppointmentStatus.NoShow),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile1.Id, dentalCheckup.Id, -17, 10, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile2.Id, dentalCleaning.Id, -11, 12, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile3.Id, dentalTreatment.Id, -5, 13, 0, AppointmentStatus.Completed),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile4.Id, dentalCheckup.Id, -1, 9, 0, AppointmentStatus.Cancelled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile5.Id, dentalCleaning.Id, 4, 12, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile1.Id, toothExtraction.Id, 9, 15, 0, AppointmentStatus.Scheduled),
                new AppointmentSeedItem(doctorProfile3.Id, patientProfile2.Id, dentalTreatment.Id, 15, 13, 0, AppointmentStatus.Scheduled),
            };

            await SeedAppointments(context, appointmentSeeds);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(ContextDb context)
        {
            if (context.Roles.Any()) return;

            context.Roles.AddRange(
                new Role { Id = (int)UserRole.Admin, Name = "Admin" },
                new Role { Id = (int)UserRole.Doctor, Name = "Doctor" },
                new Role { Id = (int)UserRole.Patient, Name = "Patient" }
            );

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, Specialization>> SeedSpecializations(ContextDb context)
        {
            var catalog = new[]
            {
                new SpecializationSeedItem(
                    "Cardiologist",
                    "Кардиолог",
                    new[] { "Cardiologist" },
                    "Диагностика и лечение сердечно-сосудистых заболеваний"
                ),
                new SpecializationSeedItem(
                    "Dentist",
                    "Стоматолог",
                    new[] { "Dentist" },
                    "Лечение и профилактика заболеваний зубов"
                ),
                new SpecializationSeedItem(
                    "Neurologist",
                    "Невролог",
                    new[] { "Neurologist" },
                    "Диагностика и лечение заболеваний нервной системы"
                )
            };

            var specializations = new Dictionary<string, Specialization>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in catalog)
            {
                var aliases = item.Aliases
                    .Append(item.Key)
                    .Append(item.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var existing = await context.Specializations
                    .FirstOrDefaultAsync(x => aliases.Contains(x.Name));

                if (existing == null)
                {
                    existing = new Specialization
                    {
                        Name = item.Name,
                        Description = item.Description
                    };

                    context.Specializations.Add(existing);
                }
                else if (string.IsNullOrWhiteSpace(existing.Description))
                {
                    existing.Description = item.Description;
                }

                specializations[item.Key] = existing;
            }

            await context.SaveChangesAsync();
            return specializations;
        }

        private static async Task<Dictionary<string, Service>> SeedServices(ContextDb context)
        {
            var catalog = new[]
            {
                new ServiceSeedItem(
                    "Первичная консультация кардиолога",
                    new[] { "Primary Consultation" },
                    "Первичный приём кардиолога с осмотром, сбором жалоб и назначением обследований.",
                    12000m,
                    40
                ),
                new ServiceSeedItem(
                    "Повторная консультация кардиолога",
                    new[] { "Follow-up Consultation" },
                    "Контрольный визит кардиолога по результатам обследований и коррекции лечения.",
                    9000m,
                    30
                ),
                new ServiceSeedItem(
                    "ЭКГ с расшифровкой",
                    new[] { "ECG Check" },
                    "Электрокардиограмма с подробной расшифровкой врача-кардиолога.",
                    8500m,
                    30
                ),
                new ServiceSeedItem(
                    "Холтер-мониторирование ЭКГ",
                    Array.Empty<string>(),
                    "Суточное мониторирование ЭКГ с заключением специалиста.",
                    16500m,
                    45
                ),
                new ServiceSeedItem(
                    "Первичная консультация невролога",
                    new[] { "Neurology Consultation" },
                    "Первичный приём невролога с оценкой неврологического статуса.",
                    12500m,
                    40
                ),
                new ServiceSeedItem(
                    "Повторная консультация невролога",
                    Array.Empty<string>(),
                    "Повторный осмотр невролога после обследований или курса лечения.",
                    9500m,
                    30
                ),
                new ServiceSeedItem(
                    "Консультация по результатам МРТ/КТ",
                    Array.Empty<string>(),
                    "Разбор результатов МРТ или КТ с рекомендациями по дальнейшему лечению.",
                    10500m,
                    30
                ),
                new ServiceSeedItem(
                    "Первичный осмотр стоматолога",
                    new[] { "Dental Checkup" },
                    "Первичный стоматологический осмотр, план лечения и рекомендации по уходу.",
                    6500m,
                    30
                ),
                new ServiceSeedItem(
                    "Профессиональная гигиена полости рта",
                    Array.Empty<string>(),
                    "Комплексная профессиональная чистка зубов и профилактика заболеваний дёсен.",
                    14000m,
                    60
                ),
                new ServiceSeedItem(
                    "Лечение кариеса",
                    new[] { "Dental Treatment" },
                    "Терапевтическое лечение кариеса с восстановлением зуба пломбировочным материалом.",
                    18000m,
                    60
                ),
                new ServiceSeedItem(
                    "Удаление зуба",
                    Array.Empty<string>(),
                    "Удаление зуба с послеоперационными рекомендациями.",
                    20000m,
                    60
                ),
            };

            var services = new Dictionary<string, Service>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in catalog)
            {
                var service = await EnsureService(context, item);
                services[item.Name] = service;
            }

            await context.SaveChangesAsync();
            return services;
        }

        private static async Task<Service> EnsureService(ContextDb context, ServiceSeedItem definition)
        {
            var aliases = definition.Aliases
                .Append(definition.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = await context.Services
                .FirstOrDefaultAsync(x => aliases.Contains(x.Name));

            if (existing == null)
            {
                existing = new Service();
                context.Services.Add(existing);
            }

            existing.Name = definition.Name;
            existing.Description = definition.Description;
            existing.Price = definition.Price;
            existing.DurationMinutes = definition.DurationMinutes;

            return existing;
        }

        private static async Task<User> EnsureUser(ContextDb context, User template)
        {
            var existing = await context.Users.FirstOrDefaultAsync(x => x.Email == template.Email);
            if (existing != null) return existing;

            context.Users.Add(template);
            await context.SaveChangesAsync();
            return template;
        }

        private static async Task<DoctorProfile> EnsureDoctorProfile(ContextDb context, int userId, DoctorProfile template)
        {
            var existing = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (existing != null) return existing;

            context.DoctorProfiles.Add(template);
            await context.SaveChangesAsync();
            return template;
        }

        private static async Task<PatientProfile> EnsurePatientProfile(ContextDb context, int userId, PatientProfile template)
        {
            var existing = await context.PatientProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (existing != null) return existing;

            context.PatientProfiles.Add(template);
            await context.SaveChangesAsync();
            return template;
        }

        private static async Task EnsureDoctorService(ContextDb context, int doctorProfileId, int serviceId)
        {
            var exists = await context.DoctorServices.AnyAsync(x =>
                x.DoctorProfileId == doctorProfileId &&
                x.ServiceId == serviceId);

            if (!exists)
            {
                context.DoctorServices.Add(new DoctorService
                {
                    DoctorProfileId = doctorProfileId,
                    ServiceId = serviceId
                });
            }
        }

        private static async Task EnsureDoctorSlotsRange(
            ContextDb context,
            int doctorProfileId,
            int startDayOffset,
            int daysCount,
            int[] hours,
            int durationMinutes)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(startDayOffset);

            for (int day = 0; day < daysCount; day++)
            {
                var currentDate = DateTime.SpecifyKind(startDate.AddDays(day), DateTimeKind.Utc);

                foreach (var hour in hours)
                {
                    var start = DateTime.SpecifyKind(
                        new DateTime(
                            currentDate.Year,
                            currentDate.Month,
                            currentDate.Day,
                            hour,
                            0,
                            0),
                        DateTimeKind.Utc);

                    var end = start.AddMinutes(durationMinutes);

                    var exists = await context.TimeSlots.AnyAsync(x =>
                        x.DoctorProfileId == doctorProfileId &&
                        x.StartTime == start &&
                        x.EndTime == end);

                    if (!exists)
                    {
                        context.TimeSlots.Add(new TimeSlot
                        {
                            DoctorProfileId = doctorProfileId,
                            StartTime = start,
                            EndTime = end,
                            Status = TimeSlotStatus.Available
                        });
                    }
                }
            }
        }

        private static async Task SeedAppointments(ContextDb context, IEnumerable<AppointmentSeedItem> seeds)
        {
            foreach (var item in seeds)
            {
                await EnsureAppointment(
                    context,
                    item.DoctorProfileId,
                    item.PatientProfileId,
                    item.ServiceId,
                    item.DayOffset,
                    item.Hour,
                    item.Minute,
                    item.Status,
                    item.Conclusion
                );
            }
        }

        private static async Task EnsureAppointment(
            ContextDb context,
            int doctorProfileId,
            int patientProfileId,
            int serviceId,
            int dayOffset,
            int hour,
            int minute,
            AppointmentStatus status,
            string? conclusion = null)
        {
            var slotDate = DateTime.UtcNow.Date.AddDays(dayOffset);
            var slotStart = DateTime.SpecifyKind(
                new DateTime(slotDate.Year, slotDate.Month, slotDate.Day, hour, minute, 0),
                DateTimeKind.Utc);

            var slot = await context.TimeSlots.FirstOrDefaultAsync(x =>
                x.DoctorProfileId == doctorProfileId &&
                x.StartTime == slotStart);

            if (slot == null)
                return;

            var exists = await context.Appointments.AnyAsync(x => x.TimeSlotId == slot.Id);
            if (exists)
                return;

            context.Appointments.Add(new Appointment
            {
                TimeSlotId = slot.Id,
                PatientProfileId = patientProfileId,
                ServiceId = serviceId,
                Status = status,
                DoctorConclusion = status == AppointmentStatus.Completed
                    ? conclusion ?? "Приём завершён. Пациенту даны рекомендации и назначен контрольный визит при необходимости."
                    : conclusion,
                CreatedAt = DateTime.UtcNow
            });

            if (status == AppointmentStatus.Scheduled ||
                status == AppointmentStatus.Completed ||
                status == AppointmentStatus.NoShow)
            {
                slot.Status = TimeSlotStatus.Booked;
            }
            else if (status == AppointmentStatus.Cancelled)
            {
                slot.Status = TimeSlotStatus.Available;
            }
        }

        private sealed record SpecializationSeedItem(
            string Key,
            string Name,
            string[] Aliases,
            string Description
        );

        private sealed record ServiceSeedItem(
            string Name,
            string[] Aliases,
            string Description,
            decimal Price,
            int DurationMinutes
        );

        private sealed record AppointmentSeedItem(
            int DoctorProfileId,
            int PatientProfileId,
            int ServiceId,
            int DayOffset,
            int Hour,
            int Minute,
            AppointmentStatus Status,
            string? Conclusion = null
        );
    }
}
