namespace BlazorPractice1.ApiRequests.Model
{
    public class UpdateMovieMessageRequest
    {
        public int id_Message { get; set; }
        public string? text { get; set; }

        public string? imageBase64 { get; set; }
        public string? imageFileName { get; set; }

        public bool removeCurrentImage { get; set; } = false;
    }
}
