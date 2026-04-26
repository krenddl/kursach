using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class DoctorProfile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [ForeignKey(nameof(Specialization))]
        public int SpecializationId { get; set; }
        public Specialization Specialization { get; set; } = null!;

        public int ExperienceYears { get; set; }

        [MaxLength(20)]
        public string? CabinetNumber { get; set; }

        public string? Bio { get; set; }

        public List<DoctorService> DoctorServices { get; set; } = new();
        public List<TimeSlot> TimeSlots { get; set; } = new();
    }
}
