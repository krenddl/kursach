namespace DigiClinicApi.Responses
{
    public class DoctorCardResponse
    {
        public int DoctorProfileId { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }

        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string? CabinetNumber { get; set; }
    }
}
