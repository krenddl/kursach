using AuthApi.CustomAtributes;
using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatServices _chatServices;

        public ChatController(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }


        //[HttpDelete]
        //[Route("DeleteMovieMessage")]
        //[RoleAuthorize([1, 2])]
        //public async Task<IActionResult> DeleteMessageAsync(int messageId)
        //{
        //    return await _chatServices.DeleteMessageAsync(messageId);
        //}


        //[HttpGet]
        //[Route("GetPrivateMessages")]
        //[RoleAuthorize([1, 2])]
        //public async Task<IActionResult> GetPrivateMessagesAsync(int userId1, int userId2)
        //{
        //    return await _chatServices.GetPrivateMessagesAsync(userId1, userId2);
        //}

        //[HttpPost]
        //[Route("SendPrivateMessage")]
        //[RoleAuthorize([1, 2])]
        //public async Task<IActionResult> SendPrivateMessageAsync([FromBody] PrivateMessageRequest request)
        //{
        //    return await _chatServices.SendPrivateMessageAsync(request);
        //}

        //[HttpPut]
        //[Route("UpdatePrivateMessage")]
        //[RoleAuthorize([1, 2])]
        //public async Task<IActionResult> UpdatePrivateMessageAsync([FromBody] UpdatePrivateMessageRequest request)
        //{
        //    return await _chatServices.UpdatePrivateMessageAsync(request);
        //}

        //[HttpDelete]
        //[Route("DeletePrivateMessage")]
        //[RoleAuthorize([1, 2])]
        //public async Task<IActionResult> DeletePrivateMessageAsync(int privateMessageId)
        //{
        //    return await _chatServices.DeletePrivateMessageAsync(privateMessageId);
        //}
    }
}