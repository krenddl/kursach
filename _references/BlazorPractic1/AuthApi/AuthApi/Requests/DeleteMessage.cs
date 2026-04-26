namespace AuthApi.Requests
{
    public class DeleteMessage
    {
        public int messageId { get; set; }
        public int userId { get; set; }
        public string userRole { get; set; }
    }
}
