using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IChatServices
    {
        Task<ChatResult<List<Message>>> GetMessagesAsync(int movieId);
        Task<ChatResult<Message>> SendMessageAsync(SendMessageDto dto);
        Task<ChatResult<Message>> UpdateMessageAsync(UpdateMovieMessageRequest request, string? token);
        Task<ChatResult<int>> DeleteMessageAsync(int messageId, string? token);

        Task<ChatResult<List<PrivateMessage>>> GetPrivateMessagesAsync(int userId1, int userId2);
        Task<ChatResult<PrivateMessage>> SendPrivateMessageAsync(PrivateMessageRequest request);
        Task<ChatResult<PrivateMessage>> UpdatePrivateMessageAsync(UpdatePrivateMessageRequest request, string? token);
        Task<ChatResult<PrivateChatGroupInfo>> DeletePrivateMessageAsync(int privateMessageId, string? token);

    }
}
