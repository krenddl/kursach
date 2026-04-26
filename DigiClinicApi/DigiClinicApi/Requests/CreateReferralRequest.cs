namespace DigiClinicApi.Requests
{
    public class CreateReferralRequest
    {
        public int SourceAppointmentId { get; set; }
        public int ServiceId { get; set; }
        public string? Comment { get; set; }
        public int Type { get; set; } = 1;
    }
}