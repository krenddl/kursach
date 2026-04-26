using System.ComponentModel.DataAnnotations;

namespace DigiClinicApi.Models
{
    public class Specialization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public List<DoctorProfile> DoctorProfiles { get; set; } = new();
    }
}
