using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.SignalR;

namespace AuthApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatServices _chatServices;

        public ChatHub(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        public async Task Join(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task Leave(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task JoinPrivateChat(int userId1, int userId2)
        {
            int minId = Math.Min(userId1, userId2);
            int maxId = Math.Max(userId1, userId2);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"private_{minId}_{maxId}");
        }

        public async Task LeavePrivateChat(int userId1, int userId2)
        {
            int minId = Math.Min(userId1, userId2);
            int maxId = Math.Max(userId1, userId2);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"private_{minId}_{maxId}");
        }

        public async Task<List<Message>> GetMovieMessagesAsync(int movieId)
        {
            var result = await _chatServices.GetMessagesAsync(movieId);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка загрузки сообщений");
                return new List<Message>();
            }

            return result.message;
        }

        public async Task SendMovieMessage(SendMessageDto dto)
        {
            var result = await _chatServices.SendMessageAsync(dto);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка отправки");
                return;
            }

            await Clients.Group($"movie_{dto.movieId}")
                .SendAsync("ReceiveMovieMessage", result.message);
        }

        public async Task EditMovieMessage(UpdateMovieMessageRequest request)
        {
            var token = Context.GetHttpContext()?.Request.Query["token"].FirstOrDefault();

            var result = await _chatServices.UpdateMessageAsync(request, token);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка редактирования");
                return;
            }

            await Clients.Group($"movie_{result.message.movieId}")
                .SendAsync("UpdateMovieMessage", result.message);
        }

        public async Task DeleteMovieMessage(int messageId)
        {
            var token = Context.GetHttpContext()?.Request.Query["token"].FirstOrDefault();

            var result = await _chatServices.DeleteMessageAsync(messageId, token);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка удаления");
                return;
            }

            await Clients.Group($"movie_{result.message}")
                .SendAsync("DeleteMovieMessage", messageId);
        }

       
        public async Task<List<PrivateMessage>> GetPrivateMessagesAsync(int userId1, int userId2)
        {
            var result = await _chatServices.GetPrivateMessagesAsync(userId1, userId2);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка загрузки личных сообщений");
                return new List<PrivateMessage>();
            }

            return result.message;
        }

        public async Task SendPrivateMessage(PrivateMessageRequest request)
        {
            var result = await _chatServices.SendPrivateMessageAsync(request);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка отправки личного сообщения");
                return;
            }

            int minId = Math.Min(result.message.senderId, result.message.receiverId);
            int maxId = Math.Max(result.message.senderId, result.message.receiverId);

            await Clients.Group($"private_{minId}_{maxId}")
                .SendAsync("ReceivePrivateMessage", result.message);
        }

        public async Task EditPrivateMessage(UpdatePrivateMessageRequest request)
        {
            var token = Context.GetHttpContext()?.Request.Query["token"].FirstOrDefault();

            var result = await _chatServices.UpdatePrivateMessageAsync(request, token);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка редактирования личного сообщения");
                return;
            }

            int minId = Math.Min(result.message.senderId, result.message.receiverId);
            int maxId = Math.Max(result.message.senderId, result.message.receiverId);

            await Clients.Group($"private_{minId}_{maxId}")
                .SendAsync("UpdatePrivateMessage", result.message);
        }

        public async Task DeletePrivateMessage(int privateMessageId)
        {
            var token = Context.GetHttpContext()?.Request.Query["token"].FirstOrDefault();

            var result = await _chatServices.DeletePrivateMessageAsync(privateMessageId, token);

            if (!result.status || result.message == null)
            {
                await Clients.Caller.SendAsync("ChatError", result.error ?? "Ошибка удаления личного сообщения");
                return;
            }

            await Clients.Group($"private_{result.message.MinUserId}_{result.message.MaxUserId}")
                .SendAsync("DeletePrivateMessage", privateMessageId);
        }


    }
}