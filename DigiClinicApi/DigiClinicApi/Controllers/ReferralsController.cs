using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralService _service;

        public ReferralsController(IReferralService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateReferralRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.Create(request, userId);
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.GetMyReferrals(userId);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctor()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.GetDoctorReferrals(userId);
        }

        [Authorize(Roles = "Patient")]
        [HttpPatch("{id}/booked")]
        public async Task<IActionResult> MarkBooked(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return await _service.MarkBooked(id, userId);
        }
    }
}