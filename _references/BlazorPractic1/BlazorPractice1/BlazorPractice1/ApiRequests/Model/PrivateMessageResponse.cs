
namespace BlazorPractice1.ApiRequests.Model
{
    public class PrivateMessageResponse
    {
        public bool status {  get; set; }
        public List<PrivateMessageModel>? privateMessageModels { get; set; }
    }
}
