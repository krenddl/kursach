using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                service = "DigiClinic API",
                checkedAt = DateTime.UtcNow
            });
        }
    }
}
