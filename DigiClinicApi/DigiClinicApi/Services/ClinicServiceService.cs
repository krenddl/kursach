using DigiClinicApi.AppDbContext;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class ClinicServiceService : IClinicServiceService
    {
        private readonly ContextDb _context;

        public ClinicServiceService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetAll()
        {
            var services = await _context.Services
                .OrderBy(x => x.Name)
                .ToListAsync();

            var result = services.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                description = x.Description,
                price = x.Price,
                duration = x.DurationMinutes,
                durationMinutes = x.DurationMinutes
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> Create(CreateServiceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return new BadRequestObjectResult("Название услуги обязательно");

            var exists = await _context.Services
                .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return new BadRequestObjectResult("Такая услуга уже существует");

            var service = new Service
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                DurationMinutes = request.DurationMinutes
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Услуга успешно создана");
        }

        public async Task<IActionResult> Update(int id, UpdateServiceRequest request)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(x => x.Id == id);

            if (service == null)
                return new NotFoundObjectResult("Услуга не найдена");

            var exists = await _context.Services
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return new BadRequestObjectResult("Услуга с таким названием уже существует");

            service.Name = request.Name;
            service.Description = request.Description;
            service.Price = request.Price;
            service.DurationMinutes = request.DurationMinutes;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Услуга успешно обновлена");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services
                .Include(x => x.Appointments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (service == null)
                return new NotFoundObjectResult("Услуга не найдена");

            if (service.Appointments.Any())
                return new BadRequestObjectResult("Нельзя удалить услугу, так как она используется в записях");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Услуга успешно удалена");
        }
    }
}
