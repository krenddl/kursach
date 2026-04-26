namespace AuthApi.Requests
{
    public class MessageRequest
    {
        public int id_Message { get; set; }

        public int movieId { get; set; }

        public int userId { get; set; }

        public string? text { get; set; }

        public string? imageUrl { get; set; }

        public DateTime createdAt { get; set; }

        public bool isEdited { get; set; } = false;
    }
}
