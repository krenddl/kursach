using System.ComponentModel.DataAnnotations.Schema;

namespace DigiClinicApi.Models
{
    public class DoctorService
    {
        [ForeignKey("DoctorProfile")]
        public int DoctorProfileId { get; set; }
        public DoctorProfile DoctorProfile { get; set; } = null!;

        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;
    }
}
