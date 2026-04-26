namespace DigiClinicApi.Models
{
    public class ChatContact
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public DateTime? LastAppointmentAt { get; set; }
    }
}
