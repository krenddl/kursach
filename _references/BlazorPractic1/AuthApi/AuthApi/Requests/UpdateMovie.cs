using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class UpdateMovie
    {
        [Required(ErrorMessage = "Id фильма обязателен")]
        public int id_Movie { get; set; }
        [Required(ErrorMessage = "Название фильма обязательно")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Описание фильма обязательно")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Id жанра обязателен")]
        public int id_Genre { get; set; }
        [Required(ErrorMessage = "Дата создания фильма обязательна")]
        public DateOnly Date { get; set; }
        public double Rating { get; set; }
        public string? img { get; set; }
    }
}
