using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class Profile
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Имя обязательно")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
