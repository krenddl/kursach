using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class PrivateMessage
    {
        [Key]
        public int id_PrivateMessage { get; set; }

        public int senderId { get; set; }

        public int receiverId { get; set; }

        public string? text { get; set; }

        public string? imageUrl { get; set; }

        public DateTime createdAt { get; set; }

        public bool isEdited { get; set; } = false;

    }
}
