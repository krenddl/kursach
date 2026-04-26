using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Interfaces
{
    public interface IMovieServices
    {
        Task<IActionResult> GetAllMovies();
        Task<IActionResult> GetMovieById(int id);
        Task<IActionResult> CreateMovie(CreateMovie newMovie);
        Task<IActionResult> UpdateMovie(UpdateMovie updateMovie);
        Task<IActionResult> DeleteMovie(int id);
        Task<IActionResult> GetAllGenre();
    }
}
