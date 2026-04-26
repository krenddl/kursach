using DigiClinicApi.AppDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DigiClinicApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class TelegramController : ControllerBase
    {
        private readonly ContextDb _context;

        public TelegramController(ContextDb context)
        {
            _context = context;
        }

        [HttpPost("link-code")]
        public async Task<IActionResult> CreateLinkCode()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("Пользователь не найден");

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);

            user.TelegramLinkCode = code;
            user.TelegramLinkCodeExpiresAt = expiresAt;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                code,
                expiresAt,
                command = $"/link {code}",
                isLinked = user.TelegramChatId.HasValue
            });
        }

        [HttpDelete("link")]
        public async Task<IActionResult> Unlink()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("Пользователь не найден");

            user.TelegramChatId = null;
            user.TelegramLinkCode = null;
            user.TelegramLinkCodeExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Telegram отвязан от профиля");
        }
    }
}
