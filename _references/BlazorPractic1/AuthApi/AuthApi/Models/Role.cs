using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthApi.Models
{
    public class Role
    {
        [Key]
        public int id_Role { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<User> users { get; set; }
    }
}
