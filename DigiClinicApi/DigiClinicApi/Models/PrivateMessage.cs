using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class PrivateMessage
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Sender))]
        public int SenderUserId { get; set; }
        public User Sender { get; set; } = null!;

        [ForeignKey(nameof(Receiver))]
        public int ReceiverUserId { get; set; }
        public User Receiver { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}
