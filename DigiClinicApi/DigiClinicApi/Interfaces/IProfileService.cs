using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IProfileService
    {
        Task<IActionResult> GetMe(int userId);
        Task<IActionResult> UpdateMe(int userId, UpdateProfileRequest request);
    }
}
