using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await _patientService.GetAll();
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return await _patientService.GetById(id);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _patientService.GetHistory(id, userId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePatientRequest request)
        {
            return await _patientService.Update(id, request);
        }
    }
}
