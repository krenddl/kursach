using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DigiClinicApi.Hubs
{
    [Authorize(Roles = "Patient,Doctor,Admin")]
    public class AppointmentHub : Hub
    {
        public const string AdminGroupName = "appointments_admins";

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
            }

            await base.OnConnectedAsync();
        }

        public Task<object> GetConnectionInfo()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            return Task.FromResult<object>(new
            {
                userId,
                role,
                connected = true
            });
        }
    }
}
