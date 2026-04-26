using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class Session
    {
        [Key] public int id_session { get; set; }
        public string Token { get; set; }
        [Required]
        [ForeignKey("User")]
        public int User_id { get; set; }
        public User User { get; set; }
    }
}
