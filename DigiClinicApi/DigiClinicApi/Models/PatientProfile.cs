using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class PatientProfile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(150)]
        public string? EmergencyContact { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
    }
}
