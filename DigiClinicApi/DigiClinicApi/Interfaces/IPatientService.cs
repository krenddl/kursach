using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IPatientService
    {
        Task<IActionResult> GetAll();
        Task<IActionResult> GetById(int id);
        Task<IActionResult> GetHistory(int id, int doctorUserId);
        Task<IActionResult> Update(int id, UpdatePatientRequest request);
    }
}
