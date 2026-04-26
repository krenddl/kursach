using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class CreateNewUser
    {
        [Required(ErrorMessage = "Email обазятелен")]
        [EmailAddress(ErrorMessage = "Некорректный email формат")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Имя обязательно")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Role_id { get; set; }
    }
}
