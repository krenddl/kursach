using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IDashboardService
    {
        Task<IActionResult> GetPatientDashboard(int userId);
        Task<IActionResult> GetDoctorDashboard(int userId);
        Task<IActionResult> GetAdminDashboard();
    }
}
