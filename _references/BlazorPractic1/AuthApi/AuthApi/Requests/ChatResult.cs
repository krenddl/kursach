namespace AuthApi.Requests
{
    public class ChatResult<T>
    {
        public bool status { get; set; }
        public string? error { get; set; }
        public T? message { get; set; }
    }
}
