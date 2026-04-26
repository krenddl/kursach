using DigiClinicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DoctorProfile")]
        public int DoctorProfileId { get; set; }
        public DoctorProfile DoctorProfile { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSlotStatus Status { get; set; } = TimeSlotStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Appointment? Appointment { get; set; }
    }
}
