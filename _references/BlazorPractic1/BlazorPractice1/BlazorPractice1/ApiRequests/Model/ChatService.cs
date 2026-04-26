namespace BlazorPractice1.ApiRequests.Model
{
    public class ChatService
    {

    }
    public class ChatUserDirectoryEntry
    {
        public int id_User { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
    }

    public class ChatUsersDirectoryResponse
    {
        public bool status { get; set; }
        public List<ChatUserDirectoryEntry>? users { get; set; }
        public string? message { get; set; }
    }
}
