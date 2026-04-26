using AuthApi.CustomAtributes;
using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserServices _userServices;
        
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration([FromBody]Registration regUser)
        {
            return await _userServices.Registration(regUser);
        }

        [HttpPost]
        [Route("Authorize")]
        public async Task<IActionResult> Authorize([FromBody] Auth auth)
        {
            return await _userServices.Authorize(auth);
        }
        [HttpPost]
        [Route("CreateNewUser")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateNewUser createNewUser)
        {
            return await _userServices.CreateNewUser(createNewUser);
        }
        [HttpPut]
        [Route("UpdateUser")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser updateUser)
        {
            var token = Request.Headers["Authorization"].ToString();
            return await _userServices.UpdateUser(updateUser, token);
        }
        [HttpPut]
        [Route("Profile")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> Profile([FromBody] Profile profile)
        {
            var token = Request.Headers["Authorization"].ToString();

            return await _userServices.Profile(profile, token);
        }
        [HttpGet]
        [Route("GetAllUsers")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> GetAllUsers()
        {
            return await _userServices.GetAllUsers();
        }

        [HttpGet]
        [Route("GetUsersForChat")]
        [RoleAuthorize([1, 2])]
        public async Task<IActionResult> GetUsersForChat()
        {
            var token = Request.Headers["Authorization"].ToString();
            return await _userServices.GetUsersForChat(token);
        }
        [HttpDelete]
        [Route("DeleteUsers")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> DeleteUsers(int user_id)
        {
            var token = Request.Headers["Authorization"].ToString();
            return await _userServices.DeleteUser(user_id, token);
        }


    }
}
