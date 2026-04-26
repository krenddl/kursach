using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Models
{
    public class Message
    {
        [Key]
        public int id_Message { get; set; }

        public int movieId { get; set; } 

        public int userId { get; set; }

        public string? text { get; set; }

        public string? imageUrl { get; set; }

        public DateTime createdAt { get; set; }

        public bool isEdited { get; set; } = false;

    }
}
