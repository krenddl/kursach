namespace BlazorPractice1.ApiRequests.Model
{
    public class GenreResponse
    {
        public int id_Genre { get; set; }
        public string name { get; set; }
    }
    public class MovieResponse
    {
        public int id_Movie { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int id_Genre { get; set; }
        public GenreResponse Genre { get; set; }
        public DateOnly date { get; set; }
        public double rating { get; set; }
        public string? img { get; set; }
    }

    public class MoviesListResponse
    {
        public bool status { get; set; }
        public List<MovieResponse> movies { get; set; } 
    }
    public class GenresListResponse
    {
        public bool status { get; set; }
        public List<GenreResponse> genres { get; set; }
    }
    
    public class CreateMovieResponse
    {
        public bool status { get; set; }
        public MovieResponse movie { get; set; }
    }

    public class GetMovieByIdResponse
    {
        public bool status { get; set; }
        public MovieResponse movie { get; set; }
    }
    
    public class UpdateMovieResponse
    {
        public bool status { get; set; }
        public MovieResponse movie { get; set; }
    }

    public class UploadImageResponse
    {
        public bool status { get; set; }
        public string? img { get; set; }
        public string? message { get; set; }
    }
    public class CreateMovieRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int id_Genre { get; set; }
        public DateOnly Date { get; set; }
        public double Rating { get; set; }
        public string? img { get; set; }
    }

    public class UpdateMovieRequest
    {
        public int id_Movie { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int id_Genre { get; set; }
        public DateOnly Date { get; set; }
        public double Rating { get; set; }
        public string? img { get; set; }
    }
}
