using DigiClinicApi.AppDbContext;
using DigiClinicApi.Interfaces;
using DigiClinicApi.Models;
using DigiClinicApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiClinicApi.Services
{
    public class ChatService : IChatService
    {
        private readonly ContextDb _context;

        public ChatService(ContextDb context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetContacts(int userId)
        {
            var currentUser = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (currentUser == null)
                return new BadRequestObjectResult("Пользователь не найден");

            var contacts = currentUser.Role.Name switch
            {
                "Patient" => await GetPatientContacts(userId),
                "Doctor" => await GetDoctorContacts(userId),
                _ => new List<ChatContact>()
            };

            return new OkObjectResult(contacts);
        }

        public async Task<ChatResult<List<PrivateMessageItem>>> GetPrivateMessagesAsync(int currentUserId, int otherUserId)
        {
            if (!await CanUsersChatAsync(currentUserId, otherUserId))
            {
                return new ChatResult<List<PrivateMessageItem>>
                {
                    Status = false,
                    Error = "Этот диалог недоступен для текущего пользователя."
                };
            }

            var messages = await _context.PrivateMessages
                .Include(x => x.Sender)
                .Include(x => x.Receiver)
                .Where(x =>
                    (x.SenderUserId == currentUserId && x.ReceiverUserId == otherUserId) ||
                    (x.SenderUserId == otherUserId && x.ReceiverUserId == currentUserId))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return new ChatResult<List<PrivateMessageItem>>
            {
                Status = true,
                Message = messages.Select(MapMessage).ToList()
            };
        }

        public async Task<ChatResult<PrivateMessageItem>> SendPrivateMessageAsync(int currentUserId, SendPrivateMessageRequest request)
        {
            var text = request.Text?.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return new ChatResult<PrivateMessageItem>
                {
                    Status = false,
                    Error = "Сообщение не может быть пустым."
                };
            }

            if (!await CanUsersChatAsync(currentUserId, request.ReceiverUserId))
            {
                return new ChatResult<PrivateMessageItem>
                {
                    Status = false,
                    Error = "Нельзя отправить сообщение этому пользователю."
                };
            }

            var sender = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
            var receiver = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.ReceiverUserId);

            if (sender == null || receiver == null)
            {
                return new ChatResult<PrivateMessageItem>
                {
                    Status = false,
                    Error = "Участник диалога не найден."
                };
            }

            var message = new PrivateMessage
            {
                SenderUserId = currentUserId,
                ReceiverUserId = request.ReceiverUserId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            _context.PrivateMessages.Add(message);
            await _context.SaveChangesAsync();

            return new ChatResult<PrivateMessageItem>
            {
                Status = true,
                Message = new PrivateMessageItem
                {
                    Id = message.Id,
                    SenderUserId = message.SenderUserId,
                    ReceiverUserId = message.ReceiverUserId,
                    SenderName = $"{sender.FirstName} {sender.LastName}",
                    ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                    Text = message.Text,
                    CreatedAt = message.CreatedAt,
                    IsEdited = message.IsEdited
                }
            };
        }

        private async Task<List<ChatContact>> GetPatientContacts(int userId)
        {
            var patient = await _context.PatientProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (patient == null)
                return new List<ChatContact>();

            var appointments = await _context.Appointments
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.User)
                .Include(x => x.TimeSlot)
                    .ThenInclude(x => x.DoctorProfile)
                        .ThenInclude(x => x.Specialization)
                .Where(x => x.PatientProfileId == patient.Id)
                .ToListAsync();

            return await BuildContacts(
                userId,
                appointments
                    .Where(x => x.TimeSlot?.DoctorProfile?.User != null)
                    .GroupBy(x => x.TimeSlot.DoctorProfile.UserId)
                    .Select(group => new ChatContact
                    {
                        UserId = group.Key,
                        FullName = $"{group.First().TimeSlot.DoctorProfile.User.FirstName} {group.First().TimeSlot.DoctorProfile.User.LastName}",
                        Subtitle = group.First().TimeSlot.DoctorProfile.Specialization.Name,
                        Role = "Doctor",
                        LastAppointmentAt = group.Max(x => x.TimeSlot.StartTime)
                    })
                    .ToList()
            );
        }

        private async Task<List<ChatContact>> GetDoctorContacts(int userId)
        {
            var doctor = await _context.DoctorProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (doctor == null)
                return new List<ChatContact>();

            var appointments = await _context.Appointments
                .Include(x => x.PatientProfile)
                    .ThenInclude(x => x.User)
                .Include(x => x.TimeSlot)
                .Where(x => x.TimeSlot.DoctorProfileId == doctor.Id)
                .ToListAsync();

            return await BuildContacts(
                userId,
                appointments
                    .Where(x => x.PatientProfile?.User != null)
                    .GroupBy(x => x.PatientProfile.UserId)
                    .Select(group => new ChatContact
                    {
                        UserId = group.Key,
                        FullName = $"{group.First().PatientProfile.User.FirstName} {group.First().PatientProfile.User.LastName}",
                        Subtitle = "Пациент клиники",
                        Role = "Patient",
                        LastAppointmentAt = group.Max(x => x.TimeSlot.StartTime)
                    })
                    .ToList()
            );
        }

        private async Task<List<ChatContact>> BuildContacts(int currentUserId, List<ChatContact> contacts)
        {
            var contactIds = contacts
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            if (contactIds.Count == 0)
                return contacts;

            var messages = await _context.PrivateMessages
                .Where(x =>
                    (x.SenderUserId == currentUserId && contactIds.Contains(x.ReceiverUserId)) ||
                    (x.ReceiverUserId == currentUserId && contactIds.Contains(x.SenderUserId)))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            foreach (var contact in contacts)
            {
                var lastMessage = messages.FirstOrDefault(x =>
                    (x.SenderUserId == currentUserId && x.ReceiverUserId == contact.UserId) ||
                    (x.ReceiverUserId == currentUserId && x.SenderUserId == contact.UserId));

                contact.LastMessageText = lastMessage?.Text;
                contact.LastMessageAt = lastMessage?.CreatedAt;
            }

            return contacts
                .OrderByDescending(x => x.LastMessageAt ?? x.LastAppointmentAt)
                .ThenBy(x => x.FullName)
                .ToList();
        }

        private async Task<bool> CanUsersChatAsync(int currentUserId, int otherUserId)
        {
            if (currentUserId == otherUserId)
                return false;

            var users = await _context.Users
                .Include(x => x.Role)
                .Where(x => x.Id == currentUserId || x.Id == otherUserId)
                .ToListAsync();

            if (users.Count != 2)
                return false;

            var currentUser = users.First(x => x.Id == currentUserId);
            var otherUser = users.First(x => x.Id == otherUserId);

            var isDoctorPatientPair =
                (currentUser.Role.Name == "Doctor" && otherUser.Role.Name == "Patient") ||
                (currentUser.Role.Name == "Patient" && otherUser.Role.Name == "Doctor");

            if (!isDoctorPatientPair)
                return false;

            var doctorUserId = currentUser.Role.Name == "Doctor" ? currentUserId : otherUserId;
            var patientUserId = currentUser.Role.Name == "Patient" ? currentUserId : otherUserId;

            var doctorProfileId = await _context.DoctorProfiles
                .Where(x => x.UserId == doctorUserId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            var patientProfileId = await _context.PatientProfiles
                .Where(x => x.UserId == patientUserId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();

            if (!doctorProfileId.HasValue || !patientProfileId.HasValue)
                return false;

            return await _context.Appointments
                .AnyAsync(x =>
                    x.PatientProfileId == patientProfileId.Value &&
                    x.TimeSlot.DoctorProfileId == doctorProfileId.Value);
        }

        private static PrivateMessageItem MapMessage(PrivateMessage message)
        {
            return new PrivateMessageItem
            {
                Id = message.Id,
                SenderUserId = message.SenderUserId,
                ReceiverUserId = message.ReceiverUserId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                ReceiverName = $"{message.Receiver.FirstName} {message.Receiver.LastName}",
                Text = message.Text,
                CreatedAt = message.CreatedAt,
                IsEdited = message.IsEdited
            };
        }
    }
}
