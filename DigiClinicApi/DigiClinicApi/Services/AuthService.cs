using DigiClinicApi.AppDbContext;
using DigiClinicApi.Enums;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using DigiClinicApi.Responses;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ContextDb _context;
        private readonly IJwtService _jwtService;

        public AuthService(ContextDb context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                throw new Exception("Email already exists");

            var patientRole = await _context.Roles
                .FirstAsync(x => x.Id == (int)UserRole.Patient);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = patientRole.Id,
                Role = patientRole
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.PatientProfiles.Add(new PatientProfile
            {
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = patientRole.Name
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                throw new Exception("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            if (!user.IsActive)
                throw new Exception("User is inactive");

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.Name
            };
        }
    }
}
