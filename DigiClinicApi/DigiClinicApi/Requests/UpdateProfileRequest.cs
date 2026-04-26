namespace DigiClinicApi.Requests
{
    public class UpdateProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }

        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }

        public int? SpecializationId { get; set; }
        public int? ExperienceYears { get; set; }
        public string? CabinetNumber { get; set; }
        public string? Bio { get; set; }
    }
}
