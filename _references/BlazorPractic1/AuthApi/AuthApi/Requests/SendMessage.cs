namespace AuthApi.Requests
{
    public class SendMessage
    {
        public int movieId { get; set; }
        public int userId { get; set; }
        public string? text { get; set; }
        public string? imageUrl { get; set; }
    }
}
