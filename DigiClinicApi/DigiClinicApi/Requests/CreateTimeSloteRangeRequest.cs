namespace DigiClinicApi.Requests
{
    public class CreateTimeSloteRangeRequest
    {
        public int DoctorProfileId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan WorkStart { get; set; }
        public TimeSpan WorkEnd { get; set; }

        public int DurationMinutes { get; set; }
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
    }
}
