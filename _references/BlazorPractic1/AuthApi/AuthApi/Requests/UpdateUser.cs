using System.ComponentModel.DataAnnotations;

namespace AuthApi.Requests
{
    public class UpdateUser
    {
        [Required(ErrorMessage = "Id пользователя обязателен")]
        public int id_User { get; set; }
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Имя обязательно")]
        [MinLength(2, ErrorMessage = "Имя должно содержать минимум 2 символа")]
        public string Name { get; set; }
        public string? Description { get; set; }
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string? Password { get; set; }
        public int Role_id { get; set; }
    }
}
