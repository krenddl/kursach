using AuthApi.CustomAtributes;
using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    
    public class MovieController : Controller
    {
        private readonly IMovieServices _movieServices;

        public MovieController(IMovieServices movieServices)
        {
            _movieServices = movieServices;
        }

        [HttpGet]
        [Route("GetAllMovies")]
        [RoleAuthorize([1,2])]  
        public async Task<IActionResult> GetAllMovies()
        {
            return await _movieServices.GetAllMovies();
        }

        [HttpGet]
        [Route("GetMovieById")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> GetMovieById(int id)
        {
            return await _movieServices.GetMovieById(id);
        }

        [HttpPost]
        [Route("CreateMovie")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> CreateMovie([FromBody]CreateMovie newMovie)
        {
            return await _movieServices.CreateMovie(newMovie);
        }

        [HttpPut]
        [Route("UpdateMovie")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> UpdateMovie([FromBody]UpdateMovie updateMovie)
        {
            return await _movieServices.UpdateMovie(updateMovie);
        }

        [HttpDelete]
        [Route("DeleteMovie")]
        [RoleAuthorize([1])]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            return await _movieServices.DeleteMovie(id);
        }
        [HttpGet]
        [Route("GetAllGenres")]
        [RoleAuthorize([1,2])]
        public async Task<IActionResult> GetAllGenres()
        {
            return await _movieServices.GetAllGenre();
        }

    }
}
