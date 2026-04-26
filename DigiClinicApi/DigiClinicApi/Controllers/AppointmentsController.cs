using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateAppointmentRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.Create(request, userId);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.GetMyAppointments(userId);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.GetDoctorAppointments(userId);
        }

        [Authorize(Roles = "Patient")]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.Cancel(id, userId);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateAppointmentStatusRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.UpdateStatus(id, request, userId);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPatch("{id}/conclusion")]
        public async Task<IActionResult> AddConclusion(int id, AddDoctorConclusionRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.AddConclusion(id, request, userId);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return await _service.GetAllAppointments();
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/filter")]
        public async Task<IActionResult> GetDoctorFiltered(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.GetDoctorAppointments(from, to, userId);
        }
    }
}

