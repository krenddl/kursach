using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthApi.Models
{
    public class Genre
    {
        [Key]
        public int id_Genre { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public ICollection<Movies> movies { get; set; }
    }
}
