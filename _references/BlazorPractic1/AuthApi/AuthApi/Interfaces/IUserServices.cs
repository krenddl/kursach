using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IUserServices
    {
        Task<IActionResult> Registration(Registration regUser);
        Task<IActionResult> Authorize(Auth authUser);
        Task<IActionResult> UpdateUser(UpdateUser updateUser, string token);
        Task<IActionResult> CreateNewUser(CreateNewUser regUser);
        Task<IActionResult> DeleteUser(int user_id, string token);
        Task<IActionResult> GetAllUsers();
        Task<IActionResult> GetUsersForChat(string token);
        Task<IActionResult> Profile(Profile profile, string token);
    }
}
