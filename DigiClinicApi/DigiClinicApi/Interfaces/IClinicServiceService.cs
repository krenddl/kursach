using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IClinicServiceService
    {
        Task<IActionResult> GetAll();
        Task<IActionResult> Create(CreateServiceRequest request);
        Task<IActionResult> Update(int id, UpdateServiceRequest request);
        Task<IActionResult> Delete(int id);
    }
}
