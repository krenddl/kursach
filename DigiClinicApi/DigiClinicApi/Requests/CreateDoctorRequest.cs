namespace DigiClinicApi.Requests
{
    public class CreateDoctorRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Phone { get; set; }

        public int SpecializationId { get; set; }
        public int ExperienceYears { get; set; }
        public string? CabinetNumber { get; set; }
        public string? Bio { get; set; }
        public List<int> ServiceIds { get; set; } = new();
    }
}
