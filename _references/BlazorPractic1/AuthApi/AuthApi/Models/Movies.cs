using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Models
{
    public class Movies
    {
        [Key]
        public int id_Movie { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        [ForeignKey("Genre")]
        public int id_Genre { get; set; }
        public Genre Genre { get; set; }
        public DateOnly date { get; set; }
        public double rating { get; set; }
        public string? img { get; set; }

    }
}
