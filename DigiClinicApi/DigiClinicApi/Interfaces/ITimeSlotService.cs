using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface ITimeSlotService
    {
        Task<IActionResult> GetByDoctor(int doctorId);
        Task<IActionResult> Create(CreateTimeSlotRequest request);
        Task<IActionResult> Delete(int id);
        Task<IActionResult> CreateRange(CreateTimeSloteRangeRequest request);
        Task<IActionResult> GetByDoctorAndDate(int doctorId, DateTime date);
    }
}
