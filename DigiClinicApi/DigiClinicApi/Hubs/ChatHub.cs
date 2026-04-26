using DigiClinicApi.Interfaces;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DigiClinicApi.Hubs
{
    [Authorize(Roles = "Patient,Doctor")]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinPrivateChat(int userId1, int userId2)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId != userId1 && currentUserId != userId2)
            {
                await Clients.Caller.SendAsync("ChatError", "Нельзя подключиться к чужому диалогу.");
                return;
            }

            var otherUserId = currentUserId == userId1 ? userId2 : userId1;
            var result = await _chatService.GetPrivateMessagesAsync(currentUserId, otherUserId);

            if (!result.Status)
            {
                await Clients.Caller.SendAsync("ChatError", result.Error ?? "Диалог недоступен.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, BuildPrivateGroupName(userId1, userId2));
        }

        public async Task LeavePrivateChat(int userId1, int userId2)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildPrivateGroupName(userId1, userId2));
        }

        public async Task<object> GetPrivateMessagesAsync(int userId1, int userId2)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId != userId1 && currentUserId != userId2)
            {
                await Clients.Caller.SendAsync("ChatError", "Нельзя просматривать чужой диалог.");
                return Array.Empty<object>();
            }

            var otherUserId = currentUserId == userId1 ? userId2 : userId1;
            var result = await _chatService.GetPrivateMessagesAsync(currentUserId, otherUserId);

            if (!result.Status || result.Message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.Error ?? "Ошибка загрузки сообщений.");
                return Array.Empty<object>();
            }

            return result.Message;
        }

        public async Task SendPrivateMessage(SendPrivateMessageRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _chatService.SendPrivateMessageAsync(currentUserId, request);

            if (!result.Status || result.Message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.Error ?? "Ошибка отправки сообщения.");
                return;
            }

            await Clients.Group(BuildPrivateGroupName(currentUserId, request.ReceiverUserId))
                .SendAsync("ReceivePrivateMessage", result.Message);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        private static string BuildPrivateGroupName(int userId1, int userId2)
        {
            var minId = Math.Min(userId1, userId2);
            var maxId = Math.Max(userId1, userId2);
            return $"private_{minId}_{maxId}";
        }
    }
}
