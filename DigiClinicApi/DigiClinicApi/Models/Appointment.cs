using DigiClinicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("TimeSlot")]
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        [ForeignKey("PatientProfile")]
        public int PatientProfileId { get; set; }
        public PatientProfile PatientProfile { get; set; } = null!;

        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public string? DoctorConclusion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        [ForeignKey("Referral")]
        public int? ReferralId { get; set; }
        public Referral? Referral { get; set; }

    }
}
