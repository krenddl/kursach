namespace BlazorPractice1.ApiRequests.Model
{
    public class PrivateMessageModel
    {
        public int id_PrivateMessage { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string? text { get; set; }
        public string? imageBase64 { get; set; }
        public string? imageUrl { get; set; }
        public bool isEdited { get; set; }
    }
}
