using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DigiClinicApi.Interfaces
{
    public interface IChatService
    {
        Task<IActionResult> GetContacts(int userId);
        Task<ChatResult<List<PrivateMessageItem>>> GetPrivateMessagesAsync(int currentUserId, int otherUserId);
        Task<ChatResult<PrivateMessageItem>> SendPrivateMessageAsync(int currentUserId, SendPrivateMessageRequest request);
    }
}
