namespace BlazorPractice1.ApiRequests.Model
{
    public class SendMessageDto
    {
        public int movieId { get; set; }
        public int userId { get; set; }
        public string? text { get; set; }

        public string? imageBase64 { get; set; }
        public string? imageFileName { get; set; }

        public DateTime createdAt { get; set; }
        public bool isEdited { get; set; } = false;
    }
}
