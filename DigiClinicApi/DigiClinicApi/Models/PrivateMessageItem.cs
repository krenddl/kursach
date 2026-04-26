namespace DigiClinicApi.Models
{
    public class PrivateMessageItem
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
    }
}
