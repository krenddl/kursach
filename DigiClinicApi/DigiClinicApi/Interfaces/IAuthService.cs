using DigiClinicApi.Requests;
using DigiClinicApi.Responses;

namespace DigiClinicApi.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }
}
