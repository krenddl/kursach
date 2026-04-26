using DigiClinicApi.AppDbContext;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using DigiClinicApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly ContextDb _context;

        public SpecializationService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetAll()
        {
            var specializations = await _context.Specializations
                .OrderBy(x => x.Name)
                .ToListAsync();

            var result = specializations.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                description = x.Description
            });

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> Create(CreateSpecializationRequest request)
        {
            var exists = await _context.Specializations
                .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return new BadRequestObjectResult("Specialization already exists");

            var specialization = new Specialization
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Specialization created");
        }

        public async Task<IActionResult> Update(int id, UpdateSpecializationRequest request)
        {
            var specialization = await _context.Specializations
                .FirstOrDefaultAsync(x => x.Id == id);

            if (specialization == null)
                return new NotFoundObjectResult("Specialization not found");

            var exists = await _context.Specializations
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return new BadRequestObjectResult("Specialization with this name already exists");

            specialization.Name = request.Name;
            specialization.Description = request.Description;

            await _context.SaveChangesAsync();

            return new OkObjectResult("Specialization updated");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var specialization = await _context.Specializations
                .Include(x => x.DoctorProfiles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (specialization == null)
                return new NotFoundObjectResult("Specialization not found");

            if (specialization.DoctorProfiles.Any())
                return new BadRequestObjectResult("Cannot delete specialization because doctors are linked to it");

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Specialization deleted");
        }
    }
}
