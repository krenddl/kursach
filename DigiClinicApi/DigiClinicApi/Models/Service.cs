using System.ComponentModel.DataAnnotations;

namespace DigiClinicApi.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public List<DoctorService> DoctorServices { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }
}
