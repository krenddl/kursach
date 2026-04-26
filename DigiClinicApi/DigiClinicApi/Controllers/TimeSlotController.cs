using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _service;

        public TimeSlotsController(ITimeSlotService service)
        {
            _service = service;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            return await _service.GetByDoctor(doctorId);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("doctor/{doctorId}/by-date")]
        public async Task<IActionResult> GetByDoctorAndDate(int doctorId, [FromQuery] DateTime date)
        {
            return await _service.GetByDoctorAndDate(doctorId, date);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTimeSlotRequest request)
        {
            return await _service.Create(request);
        }

        

        [Authorize(Roles = "Admin")]
        [HttpPost("range")]
        public async Task<IActionResult> CreateRange(CreateTimeSloteRangeRequest request)
        {
            return await _service.CreateRange(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _service.Delete(id);
        }
    }
}