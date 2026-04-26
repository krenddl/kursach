using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ContextDb _context;

        public TimeSlotService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var exists = await _context.DoctorProfiles
                .AnyAsync(x => x.Id == doctorId);

            if (!exists)
                return new NotFoundObjectResult("Врач не найден");

            var slots = await _context.TimeSlots
                .Where(x => x.DoctorProfileId == doctorId && x.EndTime > DateTime.UtcNow)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            var result = slots.Select(x => new
            {
                id = x.Id,
                start = x.StartTime,
                end = x.EndTime,
                status = x.Status.ToString()
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> Create(CreateTimeSlotRequest request)
        {
            if (request.StartTime >= request.EndTime)
                return new BadRequestObjectResult("Некорректный диапазон времени");

            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.Id == request.DoctorProfileId);

            if (doctor == null)
                return new NotFoundObjectResult("Врач не найден");

            var overlap = await _context.TimeSlots.AnyAsync(x =>
                x.DoctorProfileId == request.DoctorProfileId &&
                request.StartTime < x.EndTime &&
                request.EndTime > x.StartTime
            );

            if (overlap)
                return new BadRequestObjectResult("Слот пересекается с существующим");

            var slot = new TimeSlot
            {
                DoctorProfileId = request.DoctorProfileId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = TimeSlotStatus.Available
            };

            _context.TimeSlots.Add(slot);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Слот успешно создан");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _context.TimeSlots
                .Include(x => x.Appointment)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (slot == null)
                return new NotFoundObjectResult("Слот не найден");

            if (slot.Appointment != null)
                return new BadRequestObjectResult("Нельзя удалить занятый слот");

            _context.TimeSlots.Remove(slot);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Слот удалён");
        }

        public async Task<IActionResult> CreateRange(CreateTimeSloteRangeRequest request)
        {
            if (request.StartDate > request.EndDate)
                return new BadRequestObjectResult("Неверный диапазон дат");

            var doctorExists = await _context.DoctorProfiles
                .AnyAsync(x => x.Id == request.DoctorProfileId);

            if (!doctorExists)
                return new BadRequestObjectResult("Врач не найден");

            var createdCount = 0;

            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                var currentStart = date + request.WorkStart;
                var workEnd = date + request.WorkEnd;

                while (currentStart.AddMinutes(request.DurationMinutes) <= workEnd)
                {
                    var currentEnd = currentStart.AddMinutes(request.DurationMinutes);

                    if (request.BreakStart.HasValue && request.BreakEnd.HasValue)
                    {
                        var breakStartTime = date + request.BreakStart.Value;
                        var breakEndTime = date + request.BreakEnd.Value;

                        bool isOverlappingBreak =
                            currentStart < breakEndTime &&
                            currentEnd > breakStartTime;

                        if (isOverlappingBreak)
                        {
                            currentStart = currentEnd;
                            continue;
                        }
                    }

                    var exists = await _context.TimeSlots.AnyAsync(x =>
                        x.DoctorProfileId == request.DoctorProfileId &&
                        currentStart < x.EndTime &&
                        currentEnd > x.StartTime
                    );

                    if (!exists)
                    {
                        _context.TimeSlots.Add(new TimeSlot
                        {
                            DoctorProfileId = request.DoctorProfileId,
                            StartTime = currentStart,
                            EndTime = currentEnd,
                            Status = TimeSlotStatus.Available
                        });

                        createdCount++;
                    }

                    currentStart = currentEnd;
                }
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = $"Создано слотов: {createdCount}"
            });
        }

        public async Task<IActionResult> GetByDoctorAndDate(int doctorId, DateTime date)
        {
            var targetDate = DateOnly.FromDateTime(date);

            var slots = await _context.TimeSlots
                .Where(x => x.DoctorProfileId == doctorId)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            var filtered = slots
                .Where(x => DateOnly.FromDateTime(x.StartTime) == targetDate)
                .Select(x => new
                {
                    id = x.Id,
                    doctorProfileId = x.DoctorProfileId,
                    startTime = x.StartTime,
                    endTime = x.EndTime,
                    status = x.Status.ToString()
                })
                .ToList();

            return new OkObjectResult(filtered);
        }
    }
}
