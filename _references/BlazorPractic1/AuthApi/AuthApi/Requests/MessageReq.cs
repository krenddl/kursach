namespace AuthApi.Requests
{
    public class MessageReq
    {
        public int id_Message { get; set; }

        public int movieId { get; set; }

        public int userId { get; set; }

        public string? text { get; set; }

        public string? imagePath { get; set; }

        public DateTime createdAt { get; set; }

        public bool isEdited { get; set; } = false;
    }
}
