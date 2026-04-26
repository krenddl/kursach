using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? specializationId)
        {
            return await _doctorService.GetAll(specializationId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return await _doctorService.GetById(id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorRequest request)
        {
            return await _doctorService.Create(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDoctorReques request)
        {
            return await _doctorService.Update(id, request);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _doctorService.Delete(id);
        }
    }
}
