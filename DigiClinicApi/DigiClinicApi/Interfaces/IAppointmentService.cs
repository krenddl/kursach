using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IAppointmentService
    {
        Task<IActionResult> Create(CreateAppointmentRequest request, int userId);
        Task<IActionResult> GetMyAppointments(int userId);
        Task<IActionResult> GetDoctorAppointments(int userId);
        Task<IActionResult> Cancel(int appointmentId, int userId);
        Task<IActionResult> UpdateStatus(int appointmentId, UpdateAppointmentStatusRequest request, int userId);
        Task<IActionResult> AddConclusion(int appointmentId, AddDoctorConclusionRequest request, int userId);
        Task<IActionResult> GetAllAppointments();
        Task<IActionResult> GetDoctorAppointments(DateTime? from, DateTime? to, int userId);
    }
}
