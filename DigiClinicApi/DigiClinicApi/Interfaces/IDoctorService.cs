using DigiClinicApi.Requests;
using DigiClinicApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IDoctorService
    {
        Task<IActionResult> GetAll(int? specializationId);
        Task<IActionResult> GetById(int id);
        Task<IActionResult> Create(CreateDoctorRequest request);
        Task<IActionResult> Update(int id, UpdateDoctorReques request);
        Task<IActionResult> Delete(int id);
    }
}
