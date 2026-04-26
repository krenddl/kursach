namespace AuthApi.Requests
{
    public class SendPrivateMessage
    {
        public int senderId { get; set; }
        public int receiverId { get; set; }
        public string? text { get; set; }
        public string? imageUrl { get; set; }
    }
}
