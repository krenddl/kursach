using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IReferralService
    {
        Task<IActionResult> Create(CreateReferralRequest request, int userId);
        Task<IActionResult> GetMyReferrals(int userId);
        Task<IActionResult> GetDoctorReferrals(int userId);
        Task<IActionResult> MarkBooked(int referralId, int userId);
    }
}