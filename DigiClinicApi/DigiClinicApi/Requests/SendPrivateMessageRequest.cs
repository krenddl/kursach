using System.ComponentModel.DataAnnotations;

namespace DigiClinicApi.Requests
{
    public class SendPrivateMessageRequest
    {
        public int ReceiverUserId { get; set; }

        [MaxLength(4000)]
        public string? Text { get; set; }
    }
}
