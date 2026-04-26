using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Models
{
    public class User
    {
        [Key]
        public int id_User { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [ForeignKey("Role")]
        public int Role_Id { get; set; }
        public Role Role { get; set; }
        [JsonIgnore]
        public ICollection<Session> Sessions { get; set; }
        [JsonIgnore]
        public ICollection<Message> Messages { get; set; }

    }
}
