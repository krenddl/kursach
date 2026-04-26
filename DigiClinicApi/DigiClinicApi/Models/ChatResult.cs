namespace DigiClinicApi.Models
{
    public class ChatResult<T>
    {
        public bool Status { get; set; }
        public T? Message { get; set; }
        public string? Error { get; set; }
    }
}
