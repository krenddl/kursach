using DigiClinicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class Referral
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientProfile")]
        public int PatientProfileId { get; set; }
        public PatientProfile PatientProfile { get; set; } = null!;

        [ForeignKey("CreatedByDoctorProfile")]
        public int CreatedByDoctorProfileId { get; set; }
        public DoctorProfile CreatedByDoctorProfile { get; set; } = null!;

        [ForeignKey("SourceAppointment")]
        public int? SourceAppointmentId { get; set; }
        public Appointment? SourceAppointment { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public string? Comment { get; set; }

        public ReferralType Type { get; set; } = ReferralType.Internal;
        public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}