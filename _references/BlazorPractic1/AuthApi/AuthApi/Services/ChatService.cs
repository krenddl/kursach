using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class ChatService : IChatServices
    {
        private readonly ContextDb _context;

        public ChatService(ContextDb context)
        {
            _context = context;
        }


        public async Task<ChatResult<List<Message>>> GetMessagesAsync(int movieId)
        {
            var result = await _context.Messages
                .Where(x => x.movieId == movieId)
                .OrderBy(x => x.createdAt)
                .ToListAsync();

            return new ChatResult<List<Message>>
            {
                status = true,
                message = result
            };
        }

        public async Task<ChatResult<Message>> SendMessageAsync(SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.text) && string.IsNullOrWhiteSpace(dto.imageBase64))
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Сообщение пустое"
                };
            }

            dto.createdAt = DateTime.UtcNow;

            var imageUrl = await SaveBase64ImageAsync(dto.imageBase64, dto.imageFileName, "chat");

            Message message = new Message
            {
                movieId = dto.movieId,
                userId = dto.userId,
                text = string.IsNullOrWhiteSpace(dto.text) ? null : dto.text.Trim(),
                imageUrl = imageUrl,
                createdAt = dto.createdAt,
                isEdited = false
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return new ChatResult<Message>
            {
                status = true,
                message = message
            };
        }

        public async Task<ChatResult<Message>> UpdateMessageAsync(UpdateMovieMessageRequest request, string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Нет токена авторизации"
                };
            }

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Сессия не найдена"
                };
            }

            var message = await _context.Messages.FirstOrDefaultAsync(x => x.id_Message == request.id_Message);

            if (message == null)
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Сообщение не найдено"
                };
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.userId)
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Нет доступа"
                };
            }

            if (request.text != null)
                message.text = request.text.Trim();

            if (request.removeCurrentImage && !string.IsNullOrWhiteSpace(message.imageUrl))
            {
                var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);

                message.imageUrl = null;
            }

            if (!string.IsNullOrWhiteSpace(request.imageBase64))
            {
                if (!string.IsNullOrWhiteSpace(message.imageUrl))
                {
                    var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                message.imageUrl = await SaveBase64ImageAsync(request.imageBase64, request.imageFileName, "chat");
            }

            if (string.IsNullOrWhiteSpace(message.text) && message.imageUrl == null)
            {
                return new ChatResult<Message>
                {
                    status = false,
                    error = "Сообщение не может быть пустым"
                };
            }

            message.isEdited = true;

            await _context.SaveChangesAsync();

            return new ChatResult<Message>
            {
                status = true,
                message = message
            };
        }

        public async Task<ChatResult<int>> DeleteMessageAsync(int messageId, string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ChatResult<int>
                {
                    status = false,
                    error = "Нет токена авторизации"
                };
            }

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new ChatResult<int>
                {
                    status = false,
                    error = "Сессия не найдена"
                };
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.id_Message == messageId);

            if (message == null)
            {
                return new ChatResult<int>
                {
                    status = false,
                    error = "Сообщение не найдено"
                };
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.userId)
            {
                return new ChatResult<int>
                {
                    status = false,
                    error = "Нет доступа"
                };
            }

            int movieId = message.movieId;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return new ChatResult<int>
            {
                status = true,
                message = movieId
            };
        }


        public async Task<ChatResult<List<PrivateMessage>>> GetPrivateMessagesAsync(int userId1, int userId2)
        {
            var result = await _context.PrivateMessages
                .Where(x =>
                    (x.senderId == userId1 && x.receiverId == userId2) ||
                    (x.senderId == userId2 && x.receiverId == userId1))
                .OrderBy(x => x.createdAt)
                .ToListAsync();

            return new ChatResult<List<PrivateMessage>>
            {
                status = true,
                message = result
            };
        }

        public async Task<ChatResult<PrivateMessage>> SendPrivateMessageAsync(PrivateMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.text) && string.IsNullOrWhiteSpace(request.imageBase64))
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Сообщение пустое"
                };
            }

            var imageUrl = await SaveBase64ImageAsync(request.imageBase64, request.imageFileName, "privatechats");

            PrivateMessage privateMessage = new PrivateMessage
            {
                senderId = request.senderId,
                receiverId = request.receiverId,
                text = string.IsNullOrWhiteSpace(request.text) ? null : request.text.Trim(),
                imageUrl = imageUrl,
                createdAt = DateTime.UtcNow,
                isEdited = false
            };

            await _context.PrivateMessages.AddAsync(privateMessage);
            await _context.SaveChangesAsync();

            return new ChatResult<PrivateMessage>
            {
                status = true,
                message = privateMessage
            };
        }

        public async Task<ChatResult<PrivateMessage>> UpdatePrivateMessageAsync(UpdatePrivateMessageRequest request, string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Нет токена авторизации"
                };
            }

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Сессия не найдена"
                };
            }

            var message = await _context.PrivateMessages.FirstOrDefaultAsync(x => x.id_PrivateMessage == request.id_PrivateMessage);

            if (message == null)
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Сообщение не найдено"
                };
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.senderId)
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Нет доступа"
                };
            }

            if (request.text != null)
                message.text = request.text.Trim();

            if (request.removeCurrentImage && !string.IsNullOrWhiteSpace(message.imageUrl))
            {
                var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);

                message.imageUrl = null;
            }

            if (!string.IsNullOrWhiteSpace(request.imageBase64))
            {
                if (!string.IsNullOrWhiteSpace(message.imageUrl))
                {
                    var oldPath = Path.Combine("wwwroot", message.imageUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                message.imageUrl = await SaveBase64ImageAsync(request.imageBase64, request.imageFileName, "privatechats");
            }

            if (string.IsNullOrWhiteSpace(message.text) && message.imageUrl == null)
            {
                return new ChatResult<PrivateMessage>
                {
                    status = false,
                    error = "Сообщение не может быть пустым"
                };
            }

            message.isEdited = true;

            await _context.SaveChangesAsync();

            return new ChatResult<PrivateMessage>
            {
                status = true,
                message = message
            };
        }

        public async Task<ChatResult<PrivateChatGroupInfo>> DeletePrivateMessageAsync(int privateMessageId, string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ChatResult<PrivateChatGroupInfo>
                {
                    status = false,
                    error = "Нет токена авторизации"
                };
            }

            var session = await _context.Sessions
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return new ChatResult<PrivateChatGroupInfo>
                {
                    status = false,
                    error = "Сессия не найдена"
                };
            }

            var message = await _context.PrivateMessages
                .FirstOrDefaultAsync(x => x.id_PrivateMessage == privateMessageId);

            if (message == null)
            {
                return new ChatResult<PrivateChatGroupInfo>
                {
                    status = false,
                    error = "Сообщение не найдено"
                };
            }

            var user = session.User;

            if (user.Role_Id != 1 && user.id_User != message.senderId)
            {
                return new ChatResult<PrivateChatGroupInfo>
                {
                    status = false,
                    error = "Нет доступа"
                };
            }

            int minId = Math.Min(message.senderId, message.receiverId);
            int maxId = Math.Max(message.senderId, message.receiverId);

            _context.PrivateMessages.Remove(message);
            await _context.SaveChangesAsync();

            return new ChatResult<PrivateChatGroupInfo>
            {
                status = true,
                message = new PrivateChatGroupInfo
                {
                    MinUserId = minId,
                    MaxUserId = maxId
                }
            };
        }

        private async Task<string?> SaveBase64ImageAsync(string? imageBase64, string? imageFileName, string folderName)
        {
            if (string.IsNullOrWhiteSpace(imageBase64))
                return null;

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(imageBase64);
            }
            catch
            {
                return null;
            }

            var extension = Path.GetExtension(imageFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".png";

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await File.WriteAllBytesAsync(filePath, bytes);

            return $"/images/{folderName}/{fileName}";
        }
    }
}