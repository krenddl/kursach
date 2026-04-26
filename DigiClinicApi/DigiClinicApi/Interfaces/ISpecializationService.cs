using DigiClinicApi.Requests;
using DigiClinicApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface ISpecializationService
    {
        Task<IActionResult> GetAll();
        Task<IActionResult> Create(CreateSpecializationRequest request);
        Task<IActionResult> Update(int id, UpdateSpecializationRequest request);
        Task<IActionResult> Delete(int id);
    }
}
