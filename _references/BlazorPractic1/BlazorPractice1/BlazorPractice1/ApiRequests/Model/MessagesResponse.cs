namespace BlazorPractice1.ApiRequests.Model
{
    public class MessagesResponse
    {
        public bool status { get; set; }
        public List<MessageModel>? result { get; set; }
    }
}
