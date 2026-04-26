using AuthApi.DatabaseContext;
using AuthApi.Interfaces;
using AuthApi.Models;
using AuthApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class MovieServices : IMovieServices
    {

        private readonly ContextDb _contextDb;
        public MovieServices(ContextDb contextDb)
        {
            _contextDb = contextDb;
        }
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _contextDb.Movies.Include(x => x.Genre).ToListAsync();
            
            if(movies == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = "Фильмы не найдены"
                });
            }

            return new OkObjectResult(new
            {
                status = true,
                movies
            });

        }

        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _contextDb.Movies.Include(x => x.Genre).FirstOrDefaultAsync(x => x.id_Movie == id);

            if (movie == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = "Фильм не найден"
                });
            }

            return new OkObjectResult(new
            {
                status = true,
                movie
            });
        }

        public async Task<IActionResult> CreateMovie(CreateMovie newMovie)
        {
            var movie = new Movies()
            {
                name = newMovie.Name,
                description = newMovie.Description,
                id_Genre = newMovie.id_Genre,
                date = newMovie.Date,
                rating = newMovie.Rating,
                img = newMovie.img
            };

            

            await _contextDb.Movies.AddAsync(movie);
            await _contextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                movie
            });
        }

        public async Task<IActionResult> UpdateMovie(UpdateMovie updateMovie)
        {
            var movie = await _contextDb.Movies.Include(x => x.Genre).FirstOrDefaultAsync(x => x.id_Movie == updateMovie.id_Movie);

            movie.name = updateMovie.Name;
            movie.description = updateMovie.Description;
            movie.date = updateMovie.Date;
            movie.rating = updateMovie.Rating;
            movie.id_Genre = updateMovie.id_Genre;
            movie.img = updateMovie.img;
            if (movie == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = "Фильм не найден"
                });
            }

            await _contextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                movie
            });
        }

        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _contextDb.Movies.Include(x => x.Genre).FirstOrDefaultAsync(x => x.id_Movie == id);

            if( movie == null)
            {
                return new NotFoundObjectResult(new
                {
                    error = "Фильм не найден"
                });
            }

            _contextDb.Remove(movie);
            await _contextDb.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                movie
            });
        }

        public async Task<IActionResult> GetAllGenre()
        {
            var genres = await _contextDb.Genres.ToListAsync();

            if(genres == null)
            {
                return new NotFoundObjectResult(new
                {
                    status = false,
                    message = "Жанры не найдены"
                });
            }

            return new OkObjectResult(new
            {
                status = true,
                genres
            });
        }
    }
}
