namespace AuthApi.Requests
{
    public class PrivateMessageRequest
    {
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string? text { get; set; }

        public string? imageBase64 { get; set; }
        public string? imageFileName { get; set; }

        public DateTime createdAt { get; set; }
        public bool isEdited { get; set; } = false;
    }
}
