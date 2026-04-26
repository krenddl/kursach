using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class Registration
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать миниммум 6 символов")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Имя обязательно")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
