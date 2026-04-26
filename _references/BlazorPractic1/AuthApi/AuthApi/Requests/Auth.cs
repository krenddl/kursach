using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class Auth
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }
    }
}
